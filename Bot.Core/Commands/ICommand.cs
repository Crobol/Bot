using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public interface ICommand
    {
        string Name { get; }
        IList<string> Aliases { get; }

        string Help { get; }
        string Signature { get; }

        void Execute(IrcEventArgs e);
    }
}
