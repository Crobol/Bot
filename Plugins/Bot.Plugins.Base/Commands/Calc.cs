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
    [CommandAttributes("Calculator", "calculate")]
    class Calc : Command, IDisposable
    {
        private MathEvaluator eval = new MathEvaluator();
        
        public string Help
        {
            get { return "Evaluates a mathematical expression. Parameters <expression>"; }
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            return new [] { eval.Evaluate(string.Join("", e.Data.MessageArray.Skip(1))).ToString() };
        }

        #region IDisposable implementation

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

        #endregion
    }
}
