using System;
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
    [Export(typeof(ICommand))]
    class Set : Command
    {
        UserSystem userSystem;
        
        [ImportingConstructor]
        public Set([Import("UserSystem")] UserSystem userSystem)
        {
            this.userSystem = userSystem;
        }

        public override string Name
        {
            get { return "Set"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "set" }; }
        }

        public override string Help
        {
            get { return "Sets config value to <value>. Parameters: <valuename> <value>"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            User user = userSystem.GetAuthenticatedUser(e.Data.From);

            string[] args = e.Data.Message.Split(new char[] { ' ' }, 3);
            if (args.Length == 3)
            {
                if (user != null)
                    userSystem.SetUserSetting((int)user.ID, args[1], args[2]);
                else
                    userSystem.SetUserSetting(null, args[1], args[2]);
            }
        } 
    }
}
