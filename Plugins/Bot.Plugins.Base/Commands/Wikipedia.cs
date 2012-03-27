using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using Bot.Core;
using Bot.Core.Commands;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using log4net;

namespace Bot.Commands
{
    class WikipediaOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }

        [OptionName("l", "lang")]
        [OptionFullName("language")]
        [OptionDefault("en")]
        public string Language { get; set; }
    }

    [Export(typeof(ICommand))]
    class Wikipedia : AsyncCommand
    {
        private ILog log = LogManager.GetLogger(typeof(Wikipedia));

        // TODO: Make command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public Wikipedia([Import("CommandCompletedEventHandler")] CommandCompletedEventHandler onCommandCompleted)
        {
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Wikipedia"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "wikipedia" }; }
        }

        public override string Help
        {
            get
            {
                return "Gets first paragraph of matching article.";
            }
        }

        public override string Signature
        {
            get
            {
                string signature = OptionParser.CreateCommandSignature(typeof(WikipediaOptions));
                return Aliases.Aggregate((o, x) => o += "|" + x) + " " + signature;
            }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            WikipediaOptions options = OptionParser.Parse<WikipediaOptions>(e.Data.Message);

            string query = options.Query;
            query = query.Replace(" ", "_");
            query = query.UppercaseFirst();

            IList<string> lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                string url = "https://" + options.Language + ".wikipedia.org/wiki/" + query;
                string message = FetchWikipedia(url);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    lines.Add(message.FormatToIrc());
                    lines.Add(url);
                }
            }

            if (!lines.Any())
                lines.Add("No article found");

            return new CommandCompletedEventArgs(e.Data.Channel, lines);
        }

        /// <summary>
        /// Fetches first 500 characters in wikipedia-article about "subject"
        /// </summary>
        /// <param name="irc"></param>
        /// <param name="destinationChannel"></param>
        /// <param name="subject"></param>
        protected string FetchWikipedia(string url)
        {
            try
            {
                string html = HttpHelper.GetFromUrl(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode node = doc.DocumentNode.SelectSingleNode("//div [@id = 'bodyContent']/div/p");

                if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                {
                    return "Wiki: " + WebUtility.HtmlDecode(node.InnerText).Trim();
                }
                else
                {
                    throw new Exception("No content node");
                }
            }
            catch (Exception e)
            {
                log.Error("Exception trying to fetch Wikipedia article", e);
            }
            return "";
        }
    }
}
