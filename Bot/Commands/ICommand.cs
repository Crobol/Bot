﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    interface ICommand
    {
        void Execute(IrcEventArgs e);
    }
}
