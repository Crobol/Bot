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
            get { return new string[] { "t", "translate" }; }
        }

        public override string Help
        {
            get { return "Makes a Tyda.se search and returns the first result. Parameters: <expression>"; }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string url = "http://tyda.se/search?form=1&w=" + e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault(); // TODO: URL encode
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
			
            return new CommandCompletedEventArgs(e.Data.Channel, new List<string> { message });
        }

        private string TryFetchHtml(string url)
        {
            string html = null;

            try
            {
                html = HtmlHelper.GetFromUrl(url);
            }
            catch (Exception ex)
            {
                log.Error("Exception when fetching HTML. Trying again...", ex);
                try
                {
                    html = HtmlHelper.GetFromUrl(url);
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
