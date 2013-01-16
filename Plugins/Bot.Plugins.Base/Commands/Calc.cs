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
    class Calc : Command, IDisposable
    {
        private MathEvaluator eval = new MathEvaluator();
        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && eval != null)
                {
                    eval.Dispose();
                }

                eval = null;
                disposed = true;
            }
       }

        public override string Name
        {
            get { return "Calc"; }
        }

        public override IList<string> Aliases
        {
            get { return new List<string> { "calc" }; }
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
