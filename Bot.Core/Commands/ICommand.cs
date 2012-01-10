using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    interface ICommand
    {
        string Name();
        string Help();
        void Execute(IrcEventArgs e);
    }
}
