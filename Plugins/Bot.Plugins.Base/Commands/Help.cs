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
    class Help_ : Command
    {
        Dictionary<string, ICommand> commands;
        string commandIdentifier = "!"; // TODO: Don't hard code this

        [ImportingConstructor]
        public Help_([Import("Commands")] Dictionary<string, ICommand> commands)
        {
            this.commands = commands;
        }

        public override string Name
        {
            get { return "Help"; }
        }

        public override IList<string> Aliases
        {
            get { return new List<string> { "help" }; }
        }

        public override string Help
        {
            get { return "Lists available commands or displays a help message for the command given as parameter. Parameters: [<command>]"; }
        }

        public override string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(DefaultOption)); // TODO: Move to Command?
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        public override void Execute(IrcEventArgs e)
        {
            string message = "";

            //DefaultOption options = OptionParser.Parse<DefaultOption>(e.Data.Message);

            // If parameter is given, show help message of command <parameter>
            if (e.Data.MessageArray.Count() > 1 && commands.ContainsKey(commandIdentifier + e.Data.MessageArray[1]))
                message = commands[commandIdentifier + e.Data.MessageArray[1]].Name + ": " + commands[commandIdentifier + e.Data.MessageArray[1]].Help;
            else
            {
                // Else list available commands
                message = "Available commands: ";
                foreach (ICommand command in commands.Values.Distinct())
                {
                    message += command.Name;

                    if (command.Aliases != null)
                        message += " (" + string.Join(", ", command.Aliases) + ")";

                    if (command != commands.Values.Last())
                        message += ", ";
                }
            }

            // TODO: Move this responsibility to Bot (through event?)
            if (e.Data.Type == ReceiveType.QueryNotice)
                e.Data.Irc.SendMessage(SendType.Notice, e.Data.Nick, message);
            else
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
        }
    }
}
