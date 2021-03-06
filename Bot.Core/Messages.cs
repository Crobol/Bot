﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Messages
{
    public class InvokeCommandMessage : TinyMessenger.TinyMessageBase
    {
        public string Command { get; set; }
        public IrcEventArgs IrcEventArgs { get; set; }

        public InvokeCommandMessage(object sender, string command, IrcEventArgs ircEventArgs)
            : base(sender)
        {
            Command = command;
            IrcEventArgs = ircEventArgs;
        }
    }

    public class IrcSendMessage : TinyMessenger.TinyMessageBase
    {
        public SendType sendType;
        public string server;
        public string channel;
        public string message;

        public IrcSendMessage(object sender, SendType sendType, string server, string channel, string message)
            : base(sender)
        {
            this.sendType = sendType;
            this.server = server;
            this.channel = channel;
            this.message = message;
        }
    }

    public class IrcMessage : TinyMessenger.TinyMessageBase
    {
        public IrcEventArgs ircEventArgs;

        public IrcMessage(object sender, IrcEventArgs ircEventArgs)
            : base(sender)
        {
            this.ircEventArgs = ircEventArgs;
        }
    }
}
