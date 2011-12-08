using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class Say : Command
    {
        public override string Name()
        {
            return "say";
        }

        public override void Execute(IrcEventArgs e)
        {
            string message = e.Data.Message.Split(new char[] {' '}, 2).LastOrDefault();
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
        } 
    }

    class Join : Command
    {
        public override string Name()
        {
            return "join";
        }

        public override void Execute(IrcEventArgs e)
        {
            string channel = e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault();
            e.Data.Irc.RfcJoin(channel);
        }
    }

    class Part : Command
    {
        public override string Name()
        {
            return "part";
        }

        public override void Execute(IrcEventArgs e)
        {
            e.Data.Irc.RfcPart(e.Data.Channel);
        }
    }
}
