using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public class CommandCompletedEventArgs : EventArgs
    {
        public IList<string> MessageLines { get { return messageLines; } }
        public string Server { get; set; }
        public string Channel { get; set; }
        public SendType SendType { get; set; }

        private IList<string> messageLines;

        public CommandCompletedEventArgs(string server, string channel, IList<string> messageLines, SendType sendType = SendType.Message)
        {
            this.messageLines = messageLines;
            Channel = channel;
            Server = server;
            SendType = sendType;
        }
    }
}
