using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bot.Core;
using Bot.Core.Commands;
using Bot.Core.Processors;
using Bot.Core.Plugins;
using Meebey.SmartIrc4net;
using HtmlAgilityPack;
using Nini.Config;
using log4net;
using log4net.Config;

namespace Bot
{
    class Bot
    {
        #region Members

        private IrcClient irc;
        private ILog log = LogManager.GetLogger(typeof(Bot));
        private ServerDescriptor server;

        private UserSystem userSystem;
        private BotDataContext db;

        [ImportMany] private IEnumerable<IPlugin> Plugins { get; set; }
        [ImportMany] private IEnumerable<ICommand> Commands { get; set; }
        [ImportMany] private IEnumerable<Processor> Processors { get; set; }
        private Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>(); // Name <-> Command mapping

        private IConfig config;

        private string commandIdentifier = "!";
        private static readonly int responseBufferLimit = 10;

        private static readonly DateTime startTime = DateTime.Now; // For uptime
        private static bool quit = false;

        #endregion

        #region Ctor

        public Bot()
        {
            log.Info("Starting bot instance...");

            db = new BotDataContext(new SQLiteConnection("DbLinqProvider=Sqlite;Data Source=Bot.db;"));
            userSystem = new UserSystem(db);
        }

        public Bot(ServerDescriptor server) : this()
        {
            this.server = server;
        }

        public Bot(ServerDescriptor server, IConfig config) : this(server)
        {
            this.config = config;

            LoadPlugins(config.GetString("plugin-folder", "Plugins"));
            MapCommands(config);
        }

        public Bot(string host, int port, bool useSsl, string[] channels)
        {
            userSystem = new UserSystem(db);
            server = new ServerDescriptor(host, port, useSsl, channels);
        }

        #endregion
        
        #region Initialization

        private void LoadPlugins(string pluginFolder = "Plugins")
        {
            log.Info("Loading plugins...");
            var catalog = new DirectoryCatalog(pluginFolder);
            var container = new CompositionContainer(catalog);
            container.ComposeExportedValue<Dictionary<string, ICommand>>("Commands", commands);
            container.ComposeExportedValue<IConfig>("Config", config);
            container.ComposeExportedValue<CommandCompletedEventHandler>("CommandCompletedEventHandler", OnCommandComplete);
            container.ComposeExportedValue<UserSystem>("UserSystem", userSystem);
            container.ComposeParts(this);
        }

        public void Connect()
        {
            Connect(server);
        }

        /// <summary>
        /// Creates and sets up an IrcClient instance and connects to a server according to "server"
        /// </summary>
        public void Connect(ServerDescriptor server)
        {
            if (server == null)
                throw new ArgumentNullException();

            irc = new IrcClient();

            // Settings
            irc.Encoding = System.Text.Encoding.UTF8;
            irc.SendDelay = config.GetInt("send-delay", 200);
            irc.ActiveChannelSyncing = config.GetBoolean("use-active-channel-syncing", false);
            irc.UseSsl = server.UseSsl;

            // Bind event handlers
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            irc.OnError += new Meebey.SmartIrc4net.ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            irc.OnPart += new PartEventHandler(OnPart);

            log.Info("Initializing plugins...");
            foreach (var plugin in Plugins)
            {
                plugin.Initialize(config);
                irc.OnChannelMessage += new IrcEventHandler(plugin.OnChannelMessage);
                irc.OnRawMessage += new IrcEventHandler(plugin.OnRawMessage);
            }

            try
            {
                log.Info("Connecting to server " + irc.Address);
                irc.Connect(server.Host, server.Port);
            }
            catch (ConnectionException e)
            {
                log.Error("Could not connect to server " + irc.Address, e);
                Exit();
            }

            try
            {
                irc.Login(config.GetString("nick", "slave"),
                    config.GetString("realname", "slave"),
                    0,
                    config.GetString("username", "slave")
                );

                foreach (string channel in server.Channels)
                    irc.RfcJoin(channel);

                irc.Listen();

                irc.Disconnect();
            }
            catch (ThreadAbortException e)
            {
                log.Info("Aborting thread", e);
                irc.Disconnect();
                Thread.CurrentThread.Abort();
            }
            catch (ConnectionException e)
            {
                log.Error("Error", e);
                Thread.CurrentThread.Abort();
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                Thread.CurrentThread.Abort();
            }
        }
        
        /// <summary>
        /// Connects to all servers according to ServerDescriptors in separate threads and suspends itself in a loop waiting for console input
        /// </summary>
        public static void Run(ServerDescriptor[] servers, IConfig globalSettings)
        {
            Bot.Run(servers.ToList(), globalSettings);
        }

        /// <summary>
        /// Connects to all servers according to ServerDescriptors in separate threads and suspends itself in a loop waiting for console input
        /// </summary>
        public static void Run(List<ServerDescriptor> servers, IConfig globalSettings)
        {
            Thread.CurrentThread.Name = "Main";
            BasicConfigurator.Configure();

            foreach (ServerDescriptor server in servers)
            {
                ServerDescriptor local = (ServerDescriptor)server.Clone();
                ThreadStart threadStart = delegate 
                {
                    Thread.CurrentThread.Name = local.Host;
                    Bot instance = new Bot(local, globalSettings);
                    instance.Connect();
                };
                Thread thread = new Thread(threadStart);
                thread.Start();
            }
            
            do
            {
                string input = Console.ReadLine();
                if (input == "die" || input == "quit" || input == "exit")
                    quit = true;
            } while (!quit);

            Exit();
        }

        /// <summary>
        /// Connects to all servers specified in the config file located at "configFilePath"
        /// </summary>
        /// <param name="configFilePath"></param>
        public static void Run(string filePath)
        {
            IConfigSource source = null;
			
            if (!File.Exists(filePath))
            {
                source = CreateDefaultConfig(filePath);
            }
            else
                source = new IniConfigSource(filePath);

            IConfig global = source.Configs["global"];

            List<ServerDescriptor> servers = new List<ServerDescriptor>();
            IEnumerator enumerator = source.Configs.GetEnumerator();

            while (enumerator.MoveNext())
            {
                IConfig config = (IConfig)enumerator.Current;
                if (!string.IsNullOrWhiteSpace(config.GetString("host")))
                {
                    servers.Add(
                        new ServerDescriptor(
                            config.GetString("host"),
                            config.GetInt("port", 6667),
                            config.GetBoolean("ssl", false),
                            config.GetString("channels").Split(',')
                        )
                    );
                }
            }

            Bot.Run(servers, global);
        }
        
        /// <summary>
        /// Register user invocable commands
        /// </summary>
        private void MapCommands(IConfig config)
        {
            log.Info("Mapping commands...");

            // Create name -> command mapping
            foreach (ICommand command in Commands)
            {
                //commands[commandIdentifier + command.Name] = command;
                if (command.Aliases != null)
                {
                    foreach (string alias in command.Aliases)
                        commands[commandIdentifier + alias] = command;
                }
            }
        }

        #endregion

        #region Event handlers

        private void OnCommandComplete(object sender, CommandCompletedEventArgs e)
        {
            if (e != null
                && !string.IsNullOrWhiteSpace(e.Destination)
                && e.MessageLines.Count > 0)
            {
                foreach (string line in e.MessageLines.Where(x => !string.IsNullOrWhiteSpace(x)))
                    irc.SendMessage(e.SendType, e.Destination, line);
            }
        }

        private void OnPart(object sender, PartEventArgs e)
        {
            if (e.Data.Irc.ActiveChannelSyncing)
            {
                IrcUser user = e.Data.Irc.GetIrcUser(e.Who);
                if (user != null)
                {
                    userSystem.DeauthenticateUser(user.Host);
                }
            }
            else
            {
                e.Data.Irc.RfcNames(e.Data.Channel);
            }
        }

        private void OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (config.GetBoolean("show-channel-messages", true))
                System.Console.WriteLine(config.GetString("channel-message-indicator", "   ") + e.Data.Nick + " -> " + e.Data.Nick + ": " + e.Data.Message);

            ProcessIrcEvent(e);
        }

        private void OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (config.GetBoolean("show-channel-messages", true))
                System.Console.WriteLine(config.GetString("channel-message-indicator", "   ") + e.Data.Nick + " -> " + e.Data.Channel + ": " + e.Data.Message);

            ProcessIrcEvent(e);
        }

        private void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e)
        {
            log.Error(e.ErrorMessage);
            Exit();
        }

        private void OnRawMessage(object sender, IrcEventArgs e)
        {
            if (config.GetBoolean("show-raw-messages", false))
                System.Console.WriteLine(config.GetString("raw-message-indicator", "   ") + e.Data.RawMessage);

            if (e.Data.Irc.PassiveChannelSyncing && e.Data.Type == ReceiveType.Name)
            {
                foreach (string channel in e.Data.Irc.JoinedChannels)
                {
                    // Keep track of nicks and deauth if user who parted doesn't exist in any of joined channels
                }
            }
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            while (!quit)
            {
                Thread.Sleep(5000);
                this.Connect();
            }
        }

        #endregion

        #region Utilities

        private void ProcessIrcEvent(IrcEventArgs e)
        {
            User user = userSystem.GetAuthenticatedUser(e.Data.From);

            if (e.Data.Message.StartsWith(commandIdentifier + "uptime"))
            {
                TimeSpan uptime = DateTime.Now - startTime;
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, uptime.Days + "d " + uptime.Hours + "h " + uptime.Minutes + "m");
            }
            else if (e.Data.Message.StartsWith(commandIdentifier))
            {
                ProcessCommand(e.Data.MessageArray[0], e);
            }
            else
            {
                foreach (Processor processor in Processors)
                {
                    processor.Execute(e);
                }
            }

            if (user != null && user.UserLevel == 10 && (e.Data.MessageArray[0] == "!quit" || e.Data.MessageArray[0] == "!die"))
            {
                irc.RfcQuit();
            }
        }

        private void ProcessCommand(string command, IrcEventArgs e)
        {
            command = command.ToLower();
            if (!commands.ContainsKey(command))
            {
                IEnumerable<string> matches = commands.Keys.Where(x => x.StartsWith(command));
                if (matches.Count() == 1)
                {
                    command = matches.First();
                }
                else if (matches.Count() > 1)
                {
                    string message = "Did you mean " +
                        matches.Aggregate(
                            (sentence, x) =>
                                (x == matches.Last() ? sentence + " or " + x : sentence + ", " + x)
                        );

                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
                }
            }

            if (!string.IsNullOrWhiteSpace(command) && commands.ContainsKey(command))
            {
                try
                {
                    commands[command].Execute(e);
                }
                catch (Exception ex)
                {
                    log.Warn("Could not execute command \"" + commands[command].Name + "\"", ex);
                }
            }
        }
		
        private static IConfigSource CreateDefaultConfig(string filePath)
        {
            File.Create(filePath);
            IConfigSource source = new IniConfigSource(filePath);
            IConfig global = source.AddConfig("global");
			
            global.Set("nick", "bot");
            global.Set("script-folder", "Scripts");
            global.Set("plugin-floder", "Plugins");
            global.Set("title-whitelist", @"https?://(www.)?youtube\.com\S*, https?://open\.spotify\.com\S*");
			
            source.Save();
			
            return source;
        }

        public static void Exit()
        {
            System.Console.WriteLine("Exiting...");
            System.Environment.Exit(0);
        }

        #endregion
    }
}
