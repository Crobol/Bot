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
        protected long counter = 0;

        protected delegate CommandCompletedEventArgs WorkerFunc(IrcEventArgs e);
        protected abstract CommandCompletedEventArgs Worker(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            Interlocked.Increment(ref counter);

            WorkerFunc worker = new WorkerFunc(Worker);
            AsyncCallback completedCallback = new AsyncCallback(CommandCompletedCallback);

            AsyncOperation async = AsyncOperationManager.CreateOperation(null);
            worker.BeginInvoke(e, completedCallback, async);
        }

        private void CommandCompletedCallback(IAsyncResult ar)
        {
            WorkerFunc worker = (WorkerFunc)((AsyncResult)ar).AsyncDelegate;
            AsyncOperation async = (AsyncOperation)ar.AsyncState;

            CommandCompletedEventArgs completedArgs = worker.EndInvoke(ar);

            Interlocked.Decrement(ref counter);
            
            async.PostOperationCompleted(delegate(object e) { OnCommandCompleted((CommandCompletedEventArgs)e); }, completedArgs);
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
