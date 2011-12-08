using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public abstract class Command
    {
        public abstract string Name();

        public abstract void Execute(IrcEventArgs e);
    }
}
