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
    class Signature : Command
    {
        Dictionary<string, ICommand> commands;
        string commandIdentifier = "!"; // TODO: Don't hard code this

        [ImportingConstructor]
        public Signature([Import("Commands")] Dictionary<string, ICommand> commands)
        {
            this.commands = commands;
        }

        public override string Name
        {
            get { return "Signature"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "signature" }; }
        }

        public override string Help
        {
            get { return "Displays the signature of a command. Parameters: <command>"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            if (e.Data.MessageArray.Length > 1 && commands.ContainsKey(commandIdentifier + e.Data.MessageArray[1]))
            {
                string signature = commands[commandIdentifier + e.Data.MessageArray[1]].Signature;
                string message = commandIdentifier + signature;

                if (e.Data.Type == ReceiveType.QueryNotice)
                    e.Data.Irc.SendMessage(SendType.Notice, e.Data.Nick, message);
                else
                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
            }
        }
    }
}
