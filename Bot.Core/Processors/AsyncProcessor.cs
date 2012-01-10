using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Bot.Core.Processors
{
    public abstract class AsyncProcessor : Processor
    {
        protected abstract void Worker(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            ThreadStart threadStart = delegate { Worker(e); };
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
    }
}
