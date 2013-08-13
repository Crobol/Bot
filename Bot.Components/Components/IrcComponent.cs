using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bot.Core;
using Bot.Core.Component;
using Bot.Core.Messages;
//using CircularBuffer;
using log4net;
using Nini.Config;
using Meebey.SmartIrc4net;
using TinyMessenger;

namespace Bot.Components
{
    public class IrcComponent : Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(IrcComponent));

        private IDictionary<string, IrcClient> ircs = new Dictionary<string, IrcClient>();
        private IConfig config;
        private string commandIdentifier = "!";
        private bool quit = false;

        private Dictionary<string, C5.CircularQueue<string>> scrollback = new Dictionary<string, C5.CircularQueue<string>>();

        public IrcComponent(ITinyMessengerHub hub, IConfig config) : base(hub)
        {
            if (hub == null)
                throw new ArgumentNullException("hub");
            log.Info("Initializing IRC component...");
            this.config = config;

            hub.Subscribe<IrcSendMessage>(OnSendIrcMessage);
        }

        private void OnSendIrcMessage(IrcSendMessage message)
        {
            if (config.GetBoolean("show-channel-messages", true))
                Console.WriteLine(config.GetString("channel-message-indicator", "   ") + ircs[message.server].Address + " -> " + message.channel + ": " + message.message);

            ircs[message.server].SendMessage(message.sendType, message.channel, message.message);
            scrollback[message.server].Enqueue(message.message);
        }

        private void OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (config.GetBoolean("show-channel-messages", true))
                Console.WriteLine(config.GetString("channel-message-indicator", "   ") + e.Data.Nick + " -> " + e.Data.Channel + ": " + e.Data.Message);

            ProcessIrcEvent(e);
            scrollback[e.Data.Irc.Address].Enqueue(e.Data.Message);
        }

        private void OnQueryMessage(object sender, IrcEventArgs e)
        {
            if (config.GetBoolean("show-channel-messages", true))
                Console.WriteLine(config.GetString("channel-message-indicator", "   ") + e.Data.Nick + " -> " + e.Data.Nick + ": " + e.Data.Message);

            ProcessIrcEvent(e);
        }

        private void OnChannelNotice(object sender, IrcEventArgs e)
        {
            OnQueryMessage(sender, e);
        }

        private void OnQueryNotice(object sender, IrcEventArgs e)
        {
            OnQueryMessage(sender, e);
        }

        private void OnDisconnected(object sender, EventArgs e)
        {
            var irc = sender as IrcClient;
            if (irc != null)
            {
                log.InfoFormat("Disconnected from {0}", irc.Address);
            }
        }

        private void ProcessIrcEvent(IrcEventArgs e)
        {
            if (e.Data.Message.StartsWith(commandIdentifier + "version"))
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "0.2.0");
            }
            else if (e.Data.Message.StartsWith(commandIdentifier + "quit") && e.Data.Nick == "wqz" && e.Data.Host == "wan-ip")
            {
                quit = true;
            }
            else if (e.Data.Message.StartsWith(commandIdentifier) && !e.Data.Message.Equals(commandIdentifier))
            {
                hub.Publish(new InvokeCommandMessage(this, e.Data.MessageArray[0], e));
            }
            else
            {
                hub.Publish(new IrcMessage(this, e));
            }
        }

        public void Connect(ServerDescriptor[] servers)
        {
            foreach (var server in servers)
                Connect(server);
        }

        /// <summary>
        /// Creates and sets up an IrcClient instance and connects to a server according to "server"
        /// </summary>
        public void Connect(ServerDescriptor server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            IrcClient irc = new IrcClient();

            // Settings
            irc.AutoReconnect = true;
            irc.Encoding = System.Text.Encoding.UTF8;
            irc.SendDelay = config.GetInt("send-delay", 200);
            irc.ActiveChannelSyncing = config.GetBoolean("use-active-channel-syncing", false);
            irc.UseSsl = server.UseSsl;

            // Bind event handlers
            irc.OnChannelMessage += OnChannelMessage;
            irc.OnQueryMessage += OnQueryMessage;
            irc.OnQueryNotice += OnQueryNotice;
            irc.OnDisconnected += OnDisconnected;

            // Create scrollback buffer
            scrollback[server.Host] = new C5.CircularQueue<string>(1000);

            try
            {
                log.Info("Connecting to server " + server.Host);
                irc.Connect(server.Host, server.Port);
            }
            catch (ConnectionException e)
            {
                log.Error("Could not connect to server " + irc.Address, e);
                Exit();
            }

            try
            {
                irc.Login(server.Nick,
                    config.GetString("realname", "slave"),
                    0,
                    config.GetString("username", "slave")
                );

                foreach (string channel in server.Channels)
                    irc.RfcJoin(channel);

                ircs[irc.Address] = irc;
            }
            catch (ConnectionException e)
            {
                log.Error("Connection error", e);
                Exit();
            }
            catch (Exception e)
            {
                Exit();
            }
        }

        public void Run()
        {
            log.Info("Entering listening loop...");

            while (!quit)
            {
                foreach (var irc in ircs.Values)
                {
                    irc.ListenOnce(false);
                }
                Thread.Sleep(200);
            }
        }

        public static void Exit()
        {
            System.Console.WriteLine("Exiting...");
            System.Environment.Exit(0);
        }
    }
}
