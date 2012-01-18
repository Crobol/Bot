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
        protected delegate CommandCompletedEventArgs WorkerDelegate(IrcEventArgs e);

        protected abstract CommandCompletedEventArgs Worker(IrcEventArgs e);

        public override void Execute(IrcEventArgs e)
        {
            WorkerDelegate worker = new WorkerDelegate(Worker);
            AsyncCallback completedCallback = new AsyncCallback(CommandCompletedCallback);

            AsyncOperation async = AsyncOperationManager.CreateOperation(null);
            worker.BeginInvoke(e, completedCallback, async);
        }

        protected void CommandCompletedCallback(IAsyncResult ar)
        {
            WorkerDelegate worker = (WorkerDelegate)((AsyncResult)ar).AsyncDelegate;
            AsyncOperation async = (AsyncOperation)ar.AsyncState;

            CommandCompletedEventArgs completedArgs = worker.EndInvoke(ar);

            async.PostOperationCompleted(delegate(object e) { OnCommandCompleted((CommandCompletedEventArgs)e); }, completedArgs);
        }
    }
}
