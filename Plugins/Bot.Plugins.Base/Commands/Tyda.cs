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
    class Tyda : AsyncCommand
    {
        private ILog log = LogManager.GetLogger(typeof(Tyda));

        // TODO: Move command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public Tyda([Import("CommandCompletedEventHandler")] CommandCompletedEventHandler onCommandCompleted)
        {
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Translate"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "translate" }; }
        }

        public override string Help
        {
            get 
            {
                return "Tyda.se word lookup."; 
            }
        }

        public override string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(TydaOptions));
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            TydaOptions options = OptionParser.Parse<TydaOptions>(e.Data.Message);

            string url = "http://tyda.se/search?form=1&w=" + options.Query; // TODO: URL encode
            if (!string.IsNullOrEmpty(options.Language))
                url += "&w_lang=" + options.Language;

            string html = TryFetchHtml(url);

            if (html == null)
                return null;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNodeCollection foundNodes = doc.DocumentNode.SelectNodes("//div [@class = 'tyda_content']/descendant::table [@class = 'tyda_res_body']/descendant::table [starts-with(@class, 'tyda_res_body_trans')]/descendant::a [starts-with(@id, 'tyda_trans')]");

            string message;
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

            if (e.Data.Type == ReceiveType.QueryNotice)
                return new CommandCompletedEventArgs(e.Data.Nick, new List<string> { message }, SendType.Notice);
            else
                return new CommandCompletedEventArgs(e.Data.Channel, new List<string> { message });
        }

        private string TryFetchHtml(string url)
        {
            string html = null;

            try
            {
                html = HttpHelper.GetFromUrl(url);
            }
            catch (Exception ex)
            {
                log.Error("Exception when fetching HTML. Trying again...", ex);
                try
                {
                    html = HttpHelper.GetFromUrl(url);
                }
                catch (Exception ex2)
                {
                    log.Error("Exception when fetching HTML. Aborting...", ex2);
                    return null;
                }
            }

            return html;
        }
    }
}
