﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public class CommandCompletedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string Destination { get; set; }
        public SendType SendType { get; set; }

        public CommandCompletedEventArgs(string destination, string message, SendType sendType = SendType.Message)
        {
            Message = message;
            Destination = destination;
            SendType = sendType;
        }
    }
}