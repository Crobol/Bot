using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Bot.Core;
using Bot.Core.Commands;
using LoreSoft.MathExpressions;
using log4net;
using Meebey.SmartIrc4net;
using Newtonsoft.Json.Linq;

namespace Bot.Commands
{
    class ExchangeOptions
    {
        [OptionFullName("amount")]
        [DefaultOption]
        public string Amount { get; set; }
        
        [OptionFullName("from")]
        [DefaultValue("USD")]
        public string From { get; set; }

        [OptionFullName("to")]
        [DefaultValue("SEK")]
        public string To { get; set; }
    }

    [Export(typeof(ICommand))]
    [CommandAttributes("Exchange", true, "exchange")]
    class Exchange : Command, IDisposable
    {
        private MathEvaluator eval = new MathEvaluator();
        private bool disposed = false;
        private readonly ILog log = LogManager.GetLogger(typeof(Exchange));
        private readonly Regex letterFilter = new Regex(@"[A-Za-z]+");

        // 0 = amount, 1 = from, 2 = to
        private const string url = "http://www.google.com/ig/calculator?hl=en&q={0}{1}=?{2}";

        public string Help
        {
            get { return "Currency conversion."; }
        }

        public string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(ExchangeOptions));
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            IList<string> lines = new List<string>();
            
            try
            {
                ExchangeOptions options = OptionParser.ParseByOrder<ExchangeOptions>(e.Data.Message);

                double amount;
                
                var expression = string.Join("", e.Data.MessageArray.Where(x => !letterFilter.IsMatch(x)));

                try
                {
                     amount = eval.Evaluate(expression);
                }
                catch
                {
                     amount = double.Parse(options.Amount);
                }

                string query = string.Format(url, amount, options.From, options.To);
                string json = HttpHelper.GetFromUrl(query);

                JObject o = JObject.Parse(json);
                string result = (string)o.SelectToken("rhs");

                string resultAmount = result.Split('.').FirstOrDefault();

                if (!string.IsNullOrEmpty(resultAmount))
                    lines.Add(amount.ToString() + " " + options.From.ToUpper() + " = " + resultAmount + " " + options.To.ToUpper());
            }
            catch (Exception ex)
            {
                log.Error("Error calculating exchange", ex);
            }

            return  lines;
        }

        #region IDispose implementation

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
