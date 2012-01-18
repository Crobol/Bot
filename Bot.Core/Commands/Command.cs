using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public delegate void CommandCompletedEventHandler(object sender, CommandCompletedEventArgs e);

    public abstract class Command : ICommand
    {
        public event CommandCompletedEventHandler CommandCompleted;

        public abstract string Name();

        public virtual string Help()
        {
            return "No help message available for this command";
        }

        public abstract void Execute(IrcEventArgs e);

        protected void OnCommandCompleted(CommandCompletedEventArgs e)
        {
            if (CommandCompleted != null)
                CommandCompleted(this, e);
        }
    }
}
