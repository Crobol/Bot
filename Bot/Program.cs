using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using Bot.Core.Plugins;
using Nini.Config;

namespace Bot
{
    class Program
    {
        static void Main(string[] args)
        {
            Bot.Run("config");
        }
    }
}
