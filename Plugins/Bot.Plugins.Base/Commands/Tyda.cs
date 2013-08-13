using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using log4net;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class TydaOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { set; get; }

        [OptionName("l", "lang")]
        [OptionFullName("language")]
        public string Language { set; get; }
    }


    [Export(typeof(ICommand))]
    [CommandAttributes("Translate", true, "translate")]
    class Tyda : Command
    {
        private ILog log = LogManager.GetLogger(typeof(Tyda));

        public string Help
        {
            get 
            {
                return "Tyda.se word lookup."; 
            }
        }

        public string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(TydaOptions));
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            string message;
            TydaOptions options = OptionParser.Parse<TydaOptions>(e.Data.Message);

            string url = "http://tyda.se/search?form=1&w=" + options.Query; // TODO: URL encode
            if (!string.IsNullOrEmpty(options.Language))
                url += "&w_lang=" + options.Language;

            string html;

            try
            {
                html = HttpHelper.GetFromUrl(url);
            }
            catch (Exception ex)
            {
                log.Error("Could not download HTML from URL: " + url, ex);
                return null;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection foundNodes = doc.DocumentNode.SelectNodes("//div [@class = 'tyda_content']/descendant::table [@class = 'tyda_res_body']/descendant::table [starts-with(@class, 'tyda_res_body_trans')]/descendant::a [starts-with(@id, 'tyda_trans')]");

            if (foundNodes != null && foundNodes.Count > 0)
            {
				StringBuilder sb = new StringBuilder();
                sb.Append("Translate: ");
                IEnumerable<HtmlNode> nodes = foundNodes.Take(4);
                foreach (HtmlNode node in nodes)
                {
                    if (!string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        sb.Append(node.InnerText);
                        if (node != nodes.Last())
                            sb.Append(", ");
                    }
                }
				message = sb.ToString();
            }
			else
				message = "No results found";

            return new [] {message};
        }
    }
}
