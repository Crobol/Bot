using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using HtmlAgilityPack;
using Nini.Config;
using Bot.Commands;
using Bot.Processors;

namespace Bot
{
    delegate void Command(IrcEventArgs e);

    class Bot
    {
        #region Members

        protected static bool quit = false;

        protected Dictionary<string, AsyncCommand> asyncCommands = new Dictionary<string,AsyncCommand>();
        protected Dictionary<string, Command> commands = new Dictionary<string,Command>();
        protected IList<AsyncProcessor> asyncProcessors = new List<AsyncProcessor>();
        protected ServerDescriptor server = null;
        protected IrcClient irc = null;
        protected bool silent = false;

        #endregion

        #region Ctor

        public Bot()
        {
            
        }

        public Bot(ServerDescriptor server)
        {
            this.server = server; 
        }

        public Bot(ServerDescriptor server, IConfig globalSettings)
        {
            this.server = server;

            AsyncCommand nowPlaying = new NowPlaying();
            nowPlaying.CommandCompleted += OnAsyncCommandComplete;
            asyncCommands.Add(nowPlaying.Name(), nowPlaying);

            AsyncCommand wikipedia = new Wikipedia();
            wikipedia.CommandCompleted += OnAsyncCommandComplete;
            asyncCommands.Add(wikipedia.Name(), wikipedia);

            AsyncProcessor urlTitles = new UrlTitles(globalSettings);
            asyncProcessors.Add(urlTitles);
        }

        public Bot(string host, int port, bool useSsl, string[] channels)
        {
            server = new ServerDescriptor(host, port, useSsl, channels);
        }

        #endregion

        #region Initialization

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
            irc.SendDelay = 200;
            irc.ActiveChannelSyncing = false;
            irc.UseSsl = server.UseSsl;

            // Bind event handlers
            irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
            irc.OnChannelMessage += new IrcEventHandler(OnChannelMessage);
            irc.OnError += new ErrorEventHandler(OnError);
            irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

            RegisterCommands();
            RegisterProcessors();

            try
            {
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
        public static void Start(ServerDescriptor[] servers, IConfig globalSettings)
        {
            Bot.Start(servers.ToList(), globalSettings);
        }

        /// <summary>
        /// Connects to all servers according to ServerDescriptors in separate threads and suspends itself in a loop waiting for console input
        /// </summary>
        public static void Start(List<ServerDescriptor> servers, IConfig globalSettings)
        {
            Thread.CurrentThread.Name = "Main";

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
        public static void Start(string configFilePath)
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

            Bot.Start(servers, global);
        }

        /// <summary>
        /// Register user invocable commands
        /// </summary>
        private void RegisterCommands()
        {
            // TODO: Make this process automatic
            Command silent = Silent;
            commands.Add("!silent", silent);
        }

        /// <summary>
        /// Register message processing functions
        /// </summary>
        private void RegisterProcessors()
        {

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

        #region Commands/processors

        private void Silent(IrcEventArgs e)
        {
            if (e.Data.MessageArray.Count() > 1 && e.Data.MessageArray[1] == "false")
                silent = false;
            else
                silent = true;
        }

        #endregion

        #region Event handlers

        public void OnAsyncCommandComplete(object sender, AsyncCommandCompletedEventArgs e)
        {
            if (e != null && !string.IsNullOrWhiteSpace(e.Destination) && !string.IsNullOrWhiteSpace(e.Message))
                irc.SendMessage(SendType.Message, e.Destination, e.Message);
        }

        public void OnQueryMessage(object sender, IrcEventArgs e)
        {

        }

        public void OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (commands.ContainsKey(e.Data.MessageArray[0]))
            {
                commands[e.Data.MessageArray[0]].DynamicInvoke(e);
            }

            if (asyncCommands.ContainsKey(e.Data.MessageArray[0]))
            {
                asyncCommands[e.Data.MessageArray[0]].Execute(e);
            }

            foreach (AsyncProcessor processor in asyncProcessors)
            {
                processor.Execute(e);
            }

            if (e.Data.Message.StartsWith("!") && e.Data.Nick == "wqz")
            {
                if (e.Data.MessageArray[0] == "!quit" || e.Data.MessageArray[0] == "!die")
                {
                    irc.Disconnect();
                    quit = true;
                }
            }
        }

        public void OnError(object sender, ErrorEventArgs e)
        {
            System.Console.WriteLine("Error | Message: " + e.ErrorMessage);
            Exit();
        }

        public void OnRawMessage(object sender, IrcEventArgs e)
        {
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
