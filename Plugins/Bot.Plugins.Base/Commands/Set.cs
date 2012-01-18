﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using Nini.Config;

namespace Bot.Commands
{
    [Export(typeof(Command))]
    class Set : Command
    {
        UserService userService;
        IDictionary<string, Command> commands;

        [ImportingConstructor]
        public Set([Import("UserService")] UserService userService, [Import("Commands")] Dictionary<string, Command> commands)
        {
            this.userService = userService;
            this.commands = commands;
        }

        public override string Name()
        {
            return "set";
        }

        public override string Help()
        {
            return "Sets config value to <value>. Parameters: <valuename> <value>";
        }

        public override void Execute(IrcEventArgs e)
        {
            User user = userService.GetAuthenticatedUser(e.Data.From);

            string[] args = e.Data.Message.Split(new char[] { ' ' }, 3);
            if (args.Length == 3)
            {
                if (user != null)
                    userService.SetUserSetting((int)user.ID, args[1], args[2]);
                else
                    userService.SetUserSetting(null, args[1], args[2]);
            }
        } 
    }
}