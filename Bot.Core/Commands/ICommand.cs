using System.Collections.Generic;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public interface ICommand
    {
        string Name { get; }
        bool Async { get; }
        string[] Aliases { get; }
        string Description { get; }

        IEnumerable<string> Execute(IrcEventArgs e);
    }
}
