using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public abstract class Command : ICommand
    {
        public abstract string Name();

        public virtual string Help()
        {
            return "No help message available for this command";
        }

        public abstract void Execute(IrcEventArgs e);
    }
}
