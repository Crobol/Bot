using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Bot.Core.Commands;
using Bot.Core.Processors;
using Bot.Commands;
using Bot.Processors;
using Bot.Core.Plugins;
using Meebey.SmartIrc4net;
using HtmlAgilityPack;
using Nini.Config;

namespace Bot
{
    class Bot
    {
        #region Members

        protected static bool quit = false;

        [ImportMany(AllowRecomposition = true)]
        public IEnumerable<IPlugin> Plugins { get; set; }

        protected Dictionary<string, Command> commands = new Dictionary<string, Command>();
        protected IList<AsyncProcessor> asyncProcessors = new List<AsyncProcessor>();
        protected ServerDescriptor server = null;
        protected IConfig config = null;
        protected IrcClient irc = null;
        protected bool silent = true;
        protected string commandIdentifier = "!";

        #endregion

        #region Ctor

        public Bot()
        {
            RegisterCommands();
        }

        public Bot(ServerDescriptor server)
        {
            this.server = server;
            RegisterCommands();
        }

        public Bot(ServerDescriptor server, IConfig config)
        {
            this.server = server;
            this.config = config;
            Console.WriteLine("Loading plugins...");
            Compose(config.GetString("plugin-folder", "Plugins"));
            RegisterCommands(config);
        }

        public Bot(string host, int port, bool useSsl, string[] channels)
        {
            server = new ServerDescriptor(host, port, useSsl, channels);
            RegisterCommands();
        }

        #endregion

        #region Initialization

        private void Compose(string pluginFolder = "Plugins")
        {
            var catalog = new DirectoryCatalog(pluginFolder);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }

        /// <summary>
        /// Connects to server according to "server"
        /// </summary>
        public void Connect()
        {
            if (server == null)
                throw new NullReferenceException();

            Thread.CurrentThread.Name = server.Host;
            irc = new IrcClient();

            // Settings
            irc.Encoding = System.Text.Encoding.UTF8;
            irc.SendDelay = 0;
            irc.ActiveChannelSyncing = false;
            irc.UseSsl = server.UseSsl;

            // Bind event handlers
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            irc.OnError += new Meebey.SmartIrc4net.ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

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
                irc.Login("slave", "slave", 0, "slave");

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
        private void RegisterCommands()
        {
            
        }

        /// <summary>
        /// Register user invocable commands
        /// </summary>
        private void RegisterCommands(IConfig config)
        {
            Console.WriteLine("Registering Commands...");

            // TODO: Make this process automatic
            AsyncCommand nowPlaying = new NowPlaying(config);
            nowPlaying.CommandCompleted += OnAsyncCommandComplete;
            commands.Add(commandIdentifier + nowPlaying.Name(), nowPlaying);

            AsyncCommand wikipedia = new Wikipedia();
            wikipedia.CommandCompleted += OnAsyncCommandComplete;
            commands.Add(commandIdentifier + wikipedia.Name(), wikipedia);

            AsyncCommand tyda = new Tyda();
            tyda.CommandCompleted += OnAsyncCommandComplete;
            commands.Add(commandIdentifier + tyda.Name(), tyda);

            commands.Add(commandIdentifier + "say", new Say());
            commands.Add(commandIdentifier + "join", new Join());
            commands.Add(commandIdentifier + "part", new Part());
            commands.Add(commandIdentifier + "set", new Set(config));

            // Register processors
            AsyncProcessor urlTitles = new UrlTitles(config);
            asyncProcessors.Add(urlTitles);
        }

        #endregion

        #region Getters/setters

        public IrcClient Irc 
        {
            get
            {
                return irc;
            }
        }

        #endregion

        #region Event handlers

        public void OnAsyncCommandComplete(object sender, AsyncCommandCompletedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.Destination) && !string.IsNullOrWhiteSpace(e.Message))
                irc.SendMessage(e.SendType, e.Destination, e.Message);
        }

        public void OnQueryMessage(object sender, IrcEventArgs e)
        {

        }

        public void OnChannelMessage(object sender, IrcEventArgs e)
        {
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

            // TODO: Real authentication
            if (e.Data.Nick == "wqz" && e.Data.MessageArray[0] == "!quit" || e.Data.MessageArray[0] == "!die")
            {
                irc.Disconnect();
                quit = true;
            }

            foreach (AsyncProcessor processor in asyncProcessors)
            {
                processor.Execute(e);
            }
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
        }

        #endregion

        #region Utilities

        public static void Exit()
        {
            System.Console.WriteLine("Exiting...");
            System.Environment.Exit(0);
        }

        #endregion
    }
}
