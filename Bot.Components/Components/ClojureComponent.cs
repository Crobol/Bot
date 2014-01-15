using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bot.Core.Component;
using Bot.Core.Messages;
using TinyMessenger;
using log4net;

namespace Bot.Components.Components
{
    public class ClojureComponent : Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ClojureComponent));

        public ClojureComponent(ITinyMessengerHub hub) : base(hub)
        {
            if (hub == null)
                throw new ArgumentNullException("hub");
            
            log.Info("Initializing Clojure component...");

            LoadScripts("Scripts");

            hub.Subscribe<InvokeCommandMessage>(OnBotCommandMessage);
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            if (message.Command == "reload")
            {
                LoadScripts("Scripts");
                return;
            }

            try
            {
                var fn = clojure.lang.RT.var("bot.plugin", message.Command);

                if (!fn.isBound)
                    return;

                object result = null;
                if (message.IrcEventArgs.Data.MessageArray.Length > 1)
                {
                    result = fn.invoke(string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(1)));
                }
                else
                {
                    result = fn.invoke();
                }

                var lines = result as IEnumerable<object>;
                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        hub.Publish<IrcSendMessage>(new IrcSendMessage(this, Meebey.SmartIrc4net.SendType.Message,
                                                                 message.IrcEventArgs.Data.Irc.Address,
                                                                 message.IrcEventArgs.Data.Channel, line.ToString()));
                    }
                }
                else
                {
                    hub.Publish<IrcSendMessage>(new IrcSendMessage(this, Meebey.SmartIrc4net.SendType.Message,
                                                              message.IrcEventArgs.Data.Irc.Address,
                                                              message.IrcEventArgs.Data.Channel, result.ToString()));
                }
            }
            catch (Exception e)
            {
                log.Error(e);
            }
        }

        private void LoadScripts(string directory)
        {
            log.Info("Loading Clojure scripts...");

            string[] files = Directory.GetFiles(directory, "*.clj");
            foreach (string file in files)
            {
                try
                {
                    var path = file.Substring(0, file.LastIndexOf('.'));
                    clojure.lang.RT.load(path);
                }
                catch (Exception e)
                {
                    log.Error("Failed to load script file \"" + file + "\"", e);
                }
            }
        }
    }
}
