using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using Bot.Core;
using Bot.Core.Commands;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(Command))]
    class Help_ : Command
    {
        Dictionary<string, Command> commands;
        string commandIdentifier = "!"; // TODO: Don't hard code this

        [ImportingConstructor]
        public Help_([Import("Commands")] Dictionary<string, Command> commands)
        {
            this.commands = commands;
        }

        public override string Name()
        {
            return "help";
        }

        public override string Help()
        {
            return "Lists available commands and displays help message for commands given as parameter. Parameters: [<command>]";
        }

        public override void Execute(IrcEventArgs e)
        {
            string message = "";

            // If parameter is given, show help message of command <parameter>
            if (e.Data.MessageArray.Count() > 1 && commands.ContainsKey(commandIdentifier + e.Data.MessageArray[1]))
                message = commands[commandIdentifier + e.Data.MessageArray[1]].Name() + ": " + commands[commandIdentifier + e.Data.MessageArray[1]].Help();
            else
            {
                // Else list available commands
                message = "Available commands: ";
                foreach (Command command in commands.Values)
                {
                    message += command.Name();
                    if (command != commands.Values.Last())
                        message += ", ";
                }
            }

            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
        }
    }
}
