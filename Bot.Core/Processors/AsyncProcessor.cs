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
        protected Object _lock = new Object();
        protected int counter = 0;
        protected long lastCompleted = 0;

        protected abstract void Worker(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            WaitCallback callback = delegate {
                Interlocked.Increment(ref counter);
                Worker(e);
                lock (_lock)
                {
                    counter--;
                    lastCompleted = System.Diagnostics.Stopwatch.GetTimestamp();
                }
            };
            ThreadPool.QueueUserWorkItem(callback);
        }

        /// <summary>
        /// Use to determine the proximity of invocations to the same processor
        /// </summary>
        /// <param name="threshold">Last invocation completed threshold value</param>
        /// <returns>True if there are multiple workers running in parallel or if the last worker completed under "threshold" ms ago</returns>
        protected bool CloseCall(long threshold = 1000)
        {
            bool result = false;
            lock (_lock)
            {
                if (counter > 1 || (System.Diagnostics.Stopwatch.GetTimestamp() - lastCompleted) < (System.Diagnostics.Stopwatch.Frequency / 1000 * threshold))
                    result = true;
            }
            return result;
        }
    }
}
