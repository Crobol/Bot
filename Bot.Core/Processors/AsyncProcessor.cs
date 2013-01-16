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
        private long counter = 0;

        protected abstract void Worker(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            WaitCallback callback = delegate {
                Interlocked.Increment(ref counter);
                Worker(e);
                Interlocked.Decrement(ref counter);
            };
            ThreadPool.QueueUserWorkItem(callback);
        }

        /// <summary>
        /// Used to determine if there are parallell calls of this command running. TODO: Reconsider
        /// </summary>
        /// <returns>True if there are multiple calls running in parallel.</returns>
        protected bool ParallelCalls()
        {
            long c = Interlocked.Read(ref counter);
            return c > 1;
        }
    }
}
