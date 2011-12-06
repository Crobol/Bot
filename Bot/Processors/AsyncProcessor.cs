using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Bot.Processors
{
    abstract class AsyncProcessor
    {
        protected abstract void Worker(IrcEventArgs e);

        public void Execute(IrcEventArgs e)
        {
            ThreadStart threadStart = delegate { Worker(e); };
            Thread thread = new Thread(threadStart);
            thread.Start();
        }
    }
}
