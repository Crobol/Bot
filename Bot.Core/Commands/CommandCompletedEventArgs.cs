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
        public IList<string> MessageLines { get; set; }
        public string Destination { get; set; }
        public SendType SendType { get; set; }

        public CommandCompletedEventArgs(string destination, IList<string> messageLines, SendType sendType = SendType.Message)
        {
            MessageLines = messageLines;
            Destination = destination;
            SendType = sendType;
        }
    }
}
