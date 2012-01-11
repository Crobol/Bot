using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
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

namespace Bot
{
    class Bot
    {
        #region Members

        private IrcClient irc = null;
        protected ServerDescriptor server = null;

        UserService userService = null;
        BotEntities db = new BotEntities();

        [ImportMany]
        private IEnumerable<IPlugin> Plugins { get; set; }

        [ImportMany]
        private IEnumerable<Command> Commands { get; set; }
        private Dictionary<string, Command> commands = new Dictionary<string, Command>();

        [ImportMany]
        private IEnumerable<Processor> Processors { get; set; }

        private IConfig config = null;

        private bool silent = true;
        private string commandIdentifier = "!";

        private static readonly DateTime startTime = DateTime.Now;
        private static bool quit = false;

        #endregion

        #region Ctor

        public Bot()
        {
            userService = new UserService(db);
        }

        public Bot(ServerDescriptor server)
        {
            this.server = server;
            userService = new UserService(db);
        }

        public Bot(ServerDescriptor server, IConfig config)
        {
            this.server = server;
            this.config = config;
            userService = new UserService(db);
            Console.WriteLine("Loading plugins...");
            LoadPlugins(config.GetString("plugin-folder", "Plugins"));
            MapCommands(config);
        }

        public Bot(string host, int port, bool useSsl, string[] channels)
        {
            userService = new UserService(db);
            server = new ServerDescriptor(host, port, useSsl, channels);
        }

        #endregion

        #region Initialization

        private void LoadPlugins(string pluginFolder = "Plugins")
        {
            var catalog = new DirectoryCatalog(pluginFolder);
            var container = new CompositionContainer(catalog);
            container.ComposeExportedValue<Dictionary<string, Command>>("Commands", commands);
            container.ComposeExportedValue<IConfig>("Config", config);
            container.ComposeExportedValue<AsyncCommand.AsyncCommandCompletedEventHandler>("AsyncCommandCompletedEventHandler", OnAsyncCommandComplete);
            container.ComposeExportedValue<UserService>("UserService", userService);
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

            Thread.CurrentThread.Name = server.Host;
            irc = new IrcClient();

            // Settings
            irc.Encoding = System.Text.Encoding.UTF8;
            irc.SendDelay = config.GetInt("send-delay", 200);
            irc.ActiveChannelSyncing = true;
            irc.UseSsl = server.UseSsl;
            IrcUser user = irc.GetIrcUser("wqz");

            // Bind event handlers
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            irc.OnError += new Meebey.SmartIrc4net.ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);
            irc.OnPart += new PartEventHandler(OnPart);

            Console.WriteLine("Initializing plugins...");
            foreach (var plugin in Plugins)
            {
                plugin.Initialize(config);
                irc.OnChannelMessage += new IrcEventHandler(plugin.OnChannelMessage);
                irc.OnRawMessage += new IrcEventHandler(plugin.OnRawMessage);
            }

            try
            {
                Console.WriteLine("Connecting to server...");
                irc.Connect(server.Host, server.Port);
            }
            catch (ConnectionException e)
            {
                System.Console.WriteLine("Error | Could not connect - Reason: " + e.Message);
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
            catch (ConnectionException)
            {
                Thread.CurrentThread.Abort();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Error | Message: " + e.Message);
                System.Console.WriteLine("Exception: " + e.StackTrace);
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

            Console.WriteLine("Starting Bot...");
            foreach (ServerDescriptor server in servers)
            {
                Bot instance = new Bot(server, globalSettings); // TODO: Save instances for disconnection
                ThreadStart threadStart = delegate { instance.Connect(); };
                Thread thread = new Thread(threadStart);
                thread.Start();
            }

            do
            {
                string input = Console.ReadLine();
                if (input == "die")
                    quit = true;
            } while (!quit);

            Exit();
        }

        /// <summary>
        /// Connects to all servers specified in the config file located at "configFilePath"
        /// </summary>
        /// <param name="configFilePath"></param>
        public static void Run(string configFilePath)
        {
            IConfigSource source = new IniConfigSource(configFilePath);

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
            Console.WriteLine("Mapping Commands...");

            // Create name -> command mapping
            foreach (Command command in Commands)
            {
                commands[commandIdentifier + command.Name()] = command;
            }
        }

        #endregion

        #region Event handlers

        public void OnAsyncCommandComplete(object sender, AsyncCommandCompletedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.Destination) && !string.IsNullOrWhiteSpace(e.Message))
                irc.SendMessage(e.SendType, e.Destination, e.Message);
        }

        public void OnPart(object sender, PartEventArgs e)
        {
            if (e.Data.Irc.ActiveChannelSyncing)
            {
                IrcUser user = e.Data.Irc.GetIrcUser(e.Who);
                if (user != null)
                {
                    userService.DeauthenticateUser(user.Host);
                }
            }
            else
            {
                e.Data.Irc.RfcNames(e.Data.Channel);
            }
        }

        public void OnQueryMessage(object sender, IrcEventArgs e)
        {
            ProcessIrcEvent(e);
        }

        public void OnChannelMessage(object sender, IrcEventArgs e)
        {
            ProcessIrcEvent(e);
        }

        public void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e)
        {
            System.Console.WriteLine("Error | Message: " + e.ErrorMessage);
            Exit();
        }

        public void OnRawMessage(object sender, IrcEventArgs e)
        {
            if (!silent)
                System.Console.WriteLine("Received | Message: " + e.Data.RawMessage);

            if (e.Data.Irc.PassiveChannelSyncing && e.Data.Type == ReceiveType.Name)
            {
                foreach (string channel in e.Data.Irc.JoinedChannels)
                {
                    // Keep track of nicks and deauth if user who parted doesn't exist in any of joined channels
                }
            }
        }

        #endregion

        #region Utilities

        public void ProcessIrcEvent(IrcEventArgs e)
        {
            User user = userService.GetAuthenticatedUser(e.Data.From);

            if (e.Data.Message.StartsWith(commandIdentifier) && commands.ContainsKey(e.Data.MessageArray[0]))
            {
                try
                {
                    commands[e.Data.MessageArray[0]].Execute(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error | Message: " + ex.Message);
                }
            }
            else if (e.Data.Message.StartsWith(commandIdentifier + "uptime"))
            {
                TimeSpan uptime = DateTime.Now - startTime;
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, uptime.Days + " days " + uptime.Hours + "h " + uptime.Minutes + "m");
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
                irc.Disconnect();
                quit = true;
            }
        }

        public static void Exit()
        {
            System.Console.WriteLine("Exiting...");
            System.Environment.Exit(0);
        }

        #endregion
    }
}
