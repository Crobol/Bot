using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    class SyncTest : Command
    {
        public SyncTest()
        {

        }

        public override string Name
        {
            get { return "SyncTest"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "synctest" }; }
        }

        public override void Execute(IrcEventArgs e)
        {
            Thread.Sleep(3000);
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "Sync wait done");
        }
    }

    [Export(typeof(ICommand))]
    class AsyncTest : AsyncCommand
    {
        public AsyncTest()
        {

        }

        public override string Name
        {
            get { return "SyncTest"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "asynctest" }; }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            Thread.Sleep(3000);
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "Async wait done");

            return null;
        }
    }
}
