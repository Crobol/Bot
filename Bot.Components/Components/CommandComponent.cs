﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml.Linq;
using Bot.Core;
using Bot.Core.Commands;
using Bot.Core.Component;
using Bot.Core.Messages;
using log4net;
using Meebey.SmartIrc4net;
using TinyMessenger;

namespace Bot.Components
{
    public class CommandComponent : Core.Component.Component
    {
        protected delegate IEnumerable<string> Execute(IrcEventArgs e);

        private readonly ILog log = LogManager.GetLogger(typeof (CommandComponent));

        [ImportMany]
        private IList<ICommand> Commands { get; set; }

        private readonly IDictionary<string, ICommand> commands = new Dictionary<string, ICommand>();

        private readonly IPersistentStore store;

        public CommandComponent(ITinyMessengerHub hub, IPersistentStore store)
            : base(hub)
        {
            if (hub == null)
                throw new ArgumentNullException("hub");

            log.Info("Initializing Command component...");

            this.store = store;

            hub.Subscribe<InvokeCommandMessage>(this.OnBotCommandMessage);

            Commands = new List<ICommand>();

            LoadCommands();

            RegisterCommands();
        }

        private void LoadCommands()
        {
            if (Commands != null)
                Commands.Clear();

            if (commands != null)
                commands.Clear();

            using (var catalog = new DirectoryCatalog("Plugins"))
            {
                log.Info("Loading plugins...");
                var container = new CompositionContainer(catalog);
                container.ComposeExportedValue("Store", store);
                container.ComposeExportedValue("Commands", commands);
                container.ComposeParts(this);
            }

            log.Info("Registering Python scripts...");

            string[] files = Directory.GetFiles("Scripts", "*.py");
            foreach (string file in files)
            {
                try
                {
                    string name = string.Join("", Path.GetFileNameWithoutExtension(file));
                    log.Info("Found script \"" + name + "\"");

                    var scriptCommand = new PythonScriptCommand(file, name, true, name.ToLower());

                    Commands.Add(scriptCommand);
                }
                catch (Exception e)
                {
                    log.Error("Failed to load script file \"" + file + "\"", e);
                }
            }

            RegisterCommands();
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
                        commands[alias] = command;
                }
            }
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            if (message.Command == "reload")
            {
                log.Debug("Reloading commands...");
                LoadCommands();
                return;
            }

            string commandName = message.Command;
            if (!commands.ContainsKey(commandName))
            {
                var matches = commands.Keys.Where(x => x.StartsWith(commandName)).ToList();
                if (matches.Count() == 1)
                {
                    commandName = matches.First();
                }
                else if (matches.Count() > 1)
                {
                    string response = "Did you mean " +
                        matches.Aggregate(
                            (sentence, x) =>
                                (x == matches.Last() ? sentence + " or " + x : sentence + ", " + x)
                        );

                    SendIrcResponse(message, new [] {response});
                }
            }

            if (!string.IsNullOrWhiteSpace(commandName) && commands.ContainsKey(commandName))
            {
                InvokeCommand(commandName, message);
            }
        }

        private void InvokeCommand(string commandName, InvokeCommandMessage message)
        {
            ICommand command = commands[commandName];

            if (command.Async)
            {
                var worker = new Execute(command.Execute);
                var callback = new AsyncCallback(CommandCompletedCallback);

                AsyncOperation async = AsyncOperationManager.CreateOperation(message);

                log.DebugFormat("Starting asynchronous exeucution of \"{0}\"", message.Command);
                worker.BeginInvoke(message.IrcEventArgs, callback, async);
            }
            else
            {
                IEnumerable<string> response = commands[commandName].Execute(message.IrcEventArgs);
                log.DebugFormat("Synchronous execution of \"{0}\" complete. Sending response...", message.Command);

                if (response != null && response.Any())
                    SendIrcResponse(message, response);
            }
        }

        private void CommandCompletedCallback(IAsyncResult ar)
        {
            Execute worker = (Execute)((AsyncResult)ar).AsyncDelegate;
            AsyncOperation async = (AsyncOperation)ar.AsyncState;
            InvokeCommandMessage message = (InvokeCommandMessage)async.UserSuppliedState;

            IEnumerable<string> response = worker.EndInvoke(ar);

            log.DebugFormat("Asynchronous execution of \"{0}\" complete. Sending response...", message.Command);

            if (response != null && response.Any())
                SendIrcResponse(message, response);
        }

        private void SendIrcResponse(InvokeCommandMessage message, IEnumerable<string> response)
        {
            if (message.IrcEventArgs.Data.Type == ReceiveType.ChannelMessage)
                SendIrcResponse(SendType.Message, message.IrcEventArgs.Data.Irc.Address, message.IrcEventArgs.Data.Channel, response);
            else if (message.IrcEventArgs.Data.Type == ReceiveType.QueryNotice)
                SendIrcResponse(SendType.Notice, message.IrcEventArgs.Data.Irc.Address, message.IrcEventArgs.Data.Nick, response);
        }

        private void SendIrcResponse(SendType sendType, string address, string channel, IEnumerable<string> response)
        {
            foreach (string line in response)
            {
                hub.Publish(new IrcSendMessage(this, sendType, address, channel, line.Trim()));
            }
        }

        private void Reload()
        {
            throw new NotImplementedException();
        }
    }
}
