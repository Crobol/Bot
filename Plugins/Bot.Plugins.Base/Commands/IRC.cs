using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    [Export(typeof(ICommand))]
    class Say : Command
    {
        public override string Name
        {
            get { return "say"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            string message = e.Data.Message.Split(new char[] {' '}, 2).LastOrDefault();
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
        } 
    }

    [Export(typeof(ICommand))]
    class Join : Command
    {
        UserService userService;

        [ImportingConstructor]
        public Join([Import("UserService")] UserService userService)
        {
            this.userService = userService;
        }

        public override string Name
        {
            get { return "Join"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "join" }; }
        }

        public override void Execute(IrcEventArgs e)
        {
            if (userService.IsAuthenticated(e.Data.From))
            {
                string channel = e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault();
                e.Data.Irc.RfcJoin(channel);
            }
            else
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "You do not have authorization to use this command");
        }
    }

    [Export(typeof(ICommand))]
    class Part : Command
    {
        UserService userService;

        [ImportingConstructor]
        public Part([Import("UserService")] UserService userService)
        {
            this.userService = userService;
        }

        public override string Name
        {
            get { return "Part"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "part" }; }
        }

        public override void Execute(IrcEventArgs e)
        {
            if (userService.IsAuthenticated(e.Data.From))
                e.Data.Irc.RfcPart(e.Data.Channel);
            else
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "You do not have authorization to use this command");
        }
    }
}
