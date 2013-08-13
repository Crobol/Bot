using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Bot.Core;
using Bot.Core.Commands;
using log4net;
using Meebey.SmartIrc4net;
using Newtonsoft.Json.Linq;

namespace Bot.Commands
{
    class ExchangeOptions
    {
        [OptionFullName("amount")]
        [DefaultOption]
        public decimal Amount { get; set; }
        
        [OptionFullName("from")]
        [DefaultValue("USD")]
        public string From { get; set; }

        [OptionFullName("to")]
        [DefaultValue("SEK")]
        public string To { get; set; }
    }

    [Export(typeof(ICommand))]
    [CommandAttributes("Exchange", true, "exchange")]
    class Exchange : Command
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Exchange));

        // 0 = amount, 1 = from, 2 = to
        private readonly string url = "http://www.google.com/ig/calculator?hl=en&q={0}{1}=?{2}";

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

                string query = string.Format(url, options.Amount, options.From, options.To);
                string json = HttpHelper.GetFromUrl(query);

                JObject o = JObject.Parse(json);
                string result = (string)o.SelectToken("rhs");

                string resultAmount = result.Split('.').FirstOrDefault();

                if (!string.IsNullOrEmpty(resultAmount))
                    lines.Add(options.Amount.ToString() + " " + options.From.ToUpper() + " = " + resultAmount + " " + options.To.ToUpper());
            }
            catch (Exception ex)
            {
                log.Error("Error calculating exchange", ex);
            }

            return  lines;
        }
    }
}
