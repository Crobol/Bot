using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    abstract class AsyncCommand : Command
    {
        protected delegate AsyncCommandCompletedEventArgs WorkerDelegate(IrcEventArgs e);
        public delegate void AsyncCommandCompletedEventHandler(object sender, AsyncCommandCompletedEventArgs e);
        public event AsyncCommandCompletedEventHandler CommandCompleted;

        protected abstract AsyncCommandCompletedEventArgs Worker(IrcEventArgs e);

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

            AsyncCommandCompletedEventArgs completedArgs = worker.EndInvoke(ar);

            async.PostOperationCompleted(delegate(object e) { OnCommandCompleted((AsyncCommandCompletedEventArgs)e); }, completedArgs);
        }

        protected void OnCommandCompleted(AsyncCommandCompletedEventArgs e)
        {
            if (CommandCompleted != null)
                CommandCompleted(this, e);
        }
    }
}
