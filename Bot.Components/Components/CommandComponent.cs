using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Bot.Core.Component;
using Bot.Core.Messages;
using log4net;
using Meebey.SmartIrc4net;
using TinyMessenger;

namespace Bot.Components
{
    public class CommandComponent : Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CommandComponent));

        [ImportMany]
        private IEnumerable<ICommand> Commands { get; set; }
        private readonly Dictionary<string, ICommand> commands = new Dictionary<string, ICommand>(); // Name <-> Command mapping

        public CommandComponent(ITinyMessengerHub hub)
            : base(hub)
        {
            if (hub == null)
                throw new ArgumentNullException("hub");

            log.Info("Initializing command component...");

            hub.Subscribe<InvokeCommandMessage>(this.OnBotCommandMessage);

            using (var catalog = new DirectoryCatalog("Plugins"))
            {
                var container = new CompositionContainer(catalog);
                container.ComposeExportedValue<Core.Commands.EventHandler>("CommandCompletedEventHandler", OnCommandComplete);
                container.ComposeExportedValue<Dictionary<string, ICommand>>("Commands", commands);
                container.ComposeParts(this);
            }

            this.RegisterCommands();
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            ProcessCommand(message.Command, message.IrcEventArgs);
        }

        private void ProcessCommand(string command, IrcEventArgs e)
        {
            command = command.ToLower();
            if (!commands.ContainsKey(command))
            {
                var matches = commands.Keys.Where(x => x.StartsWith(command)).ToList();
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
                    log.Info("Invoking command " + command);
                    commands[command].Execute(e);
                }
                catch (Exception ex)
                {
                    log.Error("Could not execute command", ex);
                }
            }
        }

        private void OnCommandComplete(object sender, CommandCompletedEventArgs e)
        {
            if (e != null
                && !string.IsNullOrWhiteSpace(e.Channel)
                && e.MessageLines.Count > 0)
            {
                try
                {
                    foreach (string line in e.MessageLines.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                       hub.Publish<IrcSendMessage>(new IrcSendMessage(this, e.SendType, e.Server, e.Channel, line)); 
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not send message", ex);
                }
            }
            else
            {
                log.Debug("Incomplete CommandCompletedEventArgs");
            }
        }

        /// <summary>
        /// Register user invocable commands
        /// </summary>
        private void RegisterCommands()
        {
            // Create name -> command mapping
            foreach (ICommand command in Commands)
            {
                if (command.Aliases != null)
                {
                    foreach (string alias in command.Aliases)
                        commands["!" + alias] = command;
                }
            }
        }
    }
}
