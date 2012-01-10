using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using Nini.Config;

namespace Bot.Commands
{
    class Set : Command
    {
        IConfig config = null;

        public Set(IConfig config)
        {
            this.config = config;
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
            string[] args = e.Data.Message.Split(new char[] { ' ' }, 3);
            if (args.Length == 3)
                config.Set(args[1], args[2]);
        } 
    }
}
