using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using LoreSoft.MathExpressions;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    class Calc : Command
    {
        private MathEvaluator eval = new MathEvaluator();

        public Calc()
        {
            
        }

        public override string Name
        {
            get { return "Calc"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "calc" }; }
        }

        public override string Help
        {
            get { return "Evaluates a mathematical expression. Parameters <expression>"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, eval.Evaluate(string.Join("", e.Data.MessageArray.Skip(1))).ToString());
        }
    }
}
