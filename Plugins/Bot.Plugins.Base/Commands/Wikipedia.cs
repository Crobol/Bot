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

    class WikipediaResponse
    {
        public readonly string Title;
        public readonly string Article;
        public readonly bool Redirect;
        public readonly string FullUrl;

        /// <summary>
        /// Construct WikipediaResponse from parameters.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="article"></param>
        /// <param name="redirect"></param>
        /// <param name="fullUrl"></param>
        public WikipediaResponse(string title, string article, bool redirect, string fullUrl)
        {
            Title = title;
            Article = article;
            FullUrl = fullUrl;
        }

        /// <summary>
        /// Construct WikipediaResponse by parsing attributes from XML.
        /// </summary>
        /// <param name="responseXml">XML to parse</param>
        public WikipediaResponse(string responseXml)
        {
            System.IO.StringReader reader = new System.IO.StringReader(responseXml);
            XElement root = XElement.Load(reader);

            FullUrl = root.Descendants("page").First().Attribute("fullurl").Value;

            string article = root.Descendants("page").Descendants("extract").FirstOrDefault().Value;
            Article = article.Replace('\n', ' ');

            Title = root.Descendants("page").First().Attribute("title").Value;
        }
    }

    [Export(typeof(ICommand))]
    class Wikipedia : AsyncCommand
    {
        private readonly ILog log = LogManager.GetLogger(typeof(Wikipedia));

        // 0 = lang, 1 = title
        private readonly string pageDataUrl = "https://{0}.wikipedia.org/w/api.php?action=query&prop=info|extracts&inprop=url&format=xml&exchars=400&explaintext&redirects&titles={1}";

        // TODO: Move command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public Wikipedia([Import("CommandCompletedEventHandler")] Core.Commands.EventHandler onCommandCompleted)
        {
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Wikipedia"; }
        }

        public override IList<string> Aliases
        {
            get { return new List<string> { "wikipedia" }; }
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
                lines = GetResponse(options);
            }

            if (!lines.Any())
                lines.Add("No article found");

            return new CommandCompletedEventArgs(e.Data.Irc.Address, e.Data.Channel, lines);
        }

        protected IList<string> GetResponse(WikipediaOptions options)
        {
            WikipediaResponse response = null;
            string query = options.Query;
            /*do
            {
                string url = string.Format(pageDataUrl, options.Language, query);
                string xml = HttpHelper.GetFromUrl(url);
                try
                {
                    response = new WikipediaResponse(xml);
                    if (response.Redirect)
                        query = string.Join("", response.Article.SkipWhile(x => x != ' ')).Trim();
                }
                catch (Exception ex)
                {
                    log.Debug(ex);
                    return new List<string>();
                }                
            } while (response.Redirect);*/

            string url = string.Format(pageDataUrl, options.Language, query);
            
            try
            {
                string xml = HttpHelper.GetFromUrl(url);
                response = new WikipediaResponse(xml);
            }
            catch (Exception ex)
            {
                log.Debug(ex);
            }   

            if (!string.IsNullOrEmpty(response.Article))
                return new List<string>() { "Wiki: " + response.Article, response.FullUrl };
            else
                return new List<string>();
        }

        protected string GetArticle(string url)
        {
            string article = null;

            try // TODO: Remove try/catch?
            {
                string xml = HttpHelper.GetFromUrl(url);
                System.IO.StringReader reader = new System.IO.StringReader(xml);
                XElement root = XElement.Load(reader);

                IEnumerable<XAttribute> attributes = root.Descendants("page").SelectMany(x => x.Attributes());
                string pageUrl = (string)attributes.Where(x => x.Name == "fullurl").FirstOrDefault();

                article = root.Descendants("page").Descendants("extract").FirstOrDefault().Value;
                article = article.Replace('\n', ' ');
            }
            catch (Exception e)
            {
                log.Error("Exception trying to fetch Wikipedia article", e);
            }

            return article;
        }

        protected WikipediaResponse GetWikipediaResponse(string url)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Fetches first 500 characters from specified Wikipedia article URL.
        /// </summary>
        protected string ScrapeWikipediaUrl(string url)
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
