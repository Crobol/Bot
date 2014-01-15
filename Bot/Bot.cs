using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Bot.Components.Components;
using Bot.Core;
using Bot.Core.Component;
using Bot.Components;
//using Bot.Core.Messages;
using Nini.Config;
using log4net;
using log4net.Config;
using TinyMessenger;

namespace Bot
{
    class Bot
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Bot));

        private readonly List<Component> components = new List<Component>();
        private readonly IrcComponent irc;
        private readonly ITinyMessengerHub hub = new TinyMessengerHub();
        private readonly IConfigSource configSource;
        private readonly IPersistentStore store;

        public Bot(string configFilePath)
        {
            BasicConfigurator.Configure();
            
            log.Info("Initalizing bot...");

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            configSource = new IniConfigSource(configFilePath);
            IConfig config = configSource.Configs["global"];

            store = new JsonPersistentStore("store.json");

            components.Add(new CommandComponent(hub, store));
            components.Add(new ProcessorComponent(hub, config));
            components.Add(new CliComponent(hub));
            components.Add(new TaskComponent(hub, store));
            components.Add(new ClojureComponent(hub));

            irc = new IrcComponent(hub, config);
            components.Add(irc);
        }

        public void Run()
        {
            irc.Connect(GetServers(configSource).ToArray());
            irc.Run();

            foreach (var component in components)
            {
                IDisposable disposable = component as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }

        private static IList<ServerDescriptor> GetServers(IConfigSource source)
        {
            List<ServerDescriptor> servers = new List<ServerDescriptor>();
            IEnumerator enumerator = source.Configs.GetEnumerator();

            IConfig global = source.Configs["global"];
            string defaultNick = global.GetString("nick", "slave");

            while (enumerator.MoveNext())
            {
                IConfig config = (IConfig)enumerator.Current;
                if (!string.IsNullOrEmpty(config.GetString("host")) && !string.IsNullOrEmpty(config.GetString("channels")))
                {
                    servers.Add(
                        new ServerDescriptor(
                            config.GetString("host"),
                            config.GetInt("port", 6667),
                            config.GetBoolean("ssl", false),
                            config.GetString("channels").Split(','),
                            config.GetString("nick", defaultNick)
                        )
                    );
                }
            }

            return servers;
        }
    }
}