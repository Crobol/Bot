using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public interface IAltCommand
    {
        string Name { get; }
        bool Async { get; }
        string[] Aliases { get; }

        IEnumerable<string> Execute(IrcEventArgs e);
    }
}
