using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using Bot.Core;
using Bot.Core.Commands;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    [CommandAttributes("Help", "help")]
    class Help_ : Command
    {
        readonly IDictionary<string, ICommand> commands;
        string commandIdentifier = "!"; // TODO: Don't hard code this

        [ImportingConstructor]
        public Help_([Import("Commands")] IDictionary<string, ICommand> commands)
        {
            this.commands = commands;
        }

        public string Help
        {
            get { return "Lists available commands or displays a help message for the command given as parameter. Parameters: [<command>]"; }
        }

        public string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(DefaultOption)); // TODO: Move to Command?
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            string message = "";

            // If parameter is given, show help message of command <parameter>
            if (e.Data.MessageArray.Count() > 1 && commands.ContainsKey(commandIdentifier + e.Data.MessageArray[1]))
            {
                message = commands[commandIdentifier + e.Data.MessageArray[1]].Name + ": " +
                          commands[commandIdentifier + e.Data.MessageArray[1]].Description;
            }
            else
            {
                // Else list available commands
                message = "Available commands: " + AvailableCommands();
            }

            return new [] {message};
        }

        private string AvailableCommands()
        {
            var availableCommands = new StringBuilder();
            foreach (ICommand command in commands.Values.Distinct())
            {
                availableCommands.Append(command.Name);

                if (command.Aliases != null)
                    availableCommands.Append(" (" + string.Join(", ", command.Aliases) + ")");

                if (command != commands.Values.Last())
                    availableCommands.Append(", ");
            }
            return availableCommands.ToString();
        }
    }
}
