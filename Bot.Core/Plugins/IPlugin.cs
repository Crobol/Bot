using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;
using Nini.Config;

namespace Bot.Core.Plugins
{
    public interface IPlugin
    {
        void Initialize(IConfig config);

        void OnQueryMessage(object sender, IrcEventArgs e);
        void OnChannelMessage(object sender, IrcEventArgs e);
        void OnError(object sender, ErrorEventArgs e);
        void OnRawMessage(object sender, IrcEventArgs e);
    }
}
