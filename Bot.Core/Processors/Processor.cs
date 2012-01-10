using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Processors
{
    public abstract class Processor
    {
        public abstract void Execute(IrcEventArgs e);
    }
}
