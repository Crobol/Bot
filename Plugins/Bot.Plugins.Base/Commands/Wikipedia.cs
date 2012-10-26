using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Bot.Core;
using Bot.Core.Commands;
using HtmlAgilityPack;
using log4net;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class WikipediaOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }

        [OptionName("l", "lang")]
        [OptionFullName("language")]
        [DefaultValue("en")]
        public string Language { get; set; }
    }

    [Export(typeof(ICommand))]
    class Wikipedia : AsyncCommand
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Wikipedia));

        private readonly string searchUrl = "http://en.wikipedia.org/w/api.php?action=opensearch&limit=3&search=test";

        // 0 = lang, 1 = title
        private readonly string pageDataUrl = "https://{0}.wikipedia.org/w/api.php?action=query&prop=info|extracts&inprop=url&format=xml&exchars=400&explaintext&titles={1}";

        // TODO: Move command completed from AsyncCommand to Command to avoid this
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
                return "Get extract from matching article.";
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

            IList<string> lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(options.Query))
            {
                string url = string.Format(pageDataUrl, options.Language, options.Query);
                lines = GetResult(url);
            }

            if (!lines.Any())
                lines.Add("No article found");

            return new CommandCompletedEventArgs(e.Data.Channel, lines);
        }

        protected IList<string> GetResult(string url)
        {
            IList<string> response = new List<string>();

            try
            {
                string xml = HttpHelper.GetFromUrl(url);
                System.IO.StringReader reader = new System.IO.StringReader(xml);
                XElement root = XElement.Load(reader);

                IEnumerable<XAttribute> attributes = root.Descendants("page").SelectMany(x => x.Attributes());
                string pageUrl = (string)attributes.Where(x => x.Name == "fullurl").FirstOrDefault();

                string extract = root.Descendants("page").Descendants("extract").FirstOrDefault().Value;
                extract = extract.Replace('\n', ' ');

                response.Add("Wiki: " + extract);
                response.Add(pageUrl);
            }
            catch (Exception e)
            {
                log.Error("Exception trying to fetch Wikipedia article", e);
            }

            return response;
        }

        /// <summary>
        /// Fetches first 500 characters from specified Wikipedia article URL.
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
