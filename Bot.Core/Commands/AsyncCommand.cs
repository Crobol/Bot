using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public abstract class AsyncCommand : Command
    {
        protected Object _lock = new Object();
        protected int counter = 0;
        protected long lastCompleted = 0;

        protected delegate CommandCompletedEventArgs WorkerDelegate(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            Interlocked.Increment(ref counter);

            WorkerDelegate worker = new WorkerDelegate(DoWork);
            AsyncCallback completedCallback = new AsyncCallback(CommandCompletedCallback);

            AsyncOperation async = AsyncOperationManager.CreateOperation(null);
            worker.BeginInvoke(e, completedCallback, async);
        }

        protected void CommandCompletedCallback(IAsyncResult ar)
        {
            WorkerDelegate worker = (WorkerDelegate)((AsyncResult)ar).AsyncDelegate;
            AsyncOperation async = (AsyncOperation)ar.AsyncState;

            CommandCompletedEventArgs completedArgs = worker.EndInvoke(ar);

            lock (_lock)
            {
                counter--;
                lastCompleted = System.Diagnostics.Stopwatch.GetTimestamp();
            }
            
            async.PostOperationCompleted(delegate(object e) { OnCommandCompleted((CommandCompletedEventArgs)e); }, completedArgs);
        }

        /// <summary>
        /// Use to determine the proximity of calls to the same command
        /// </summary>
        /// <param name="threshold">Last command completed threshold value</param>
        /// <returns>True if there are multiple calls running in parallel or if the last call completed under "threshold" ms ago</returns>
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
