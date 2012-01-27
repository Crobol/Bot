using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bot.Core.Processors;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using Nini.Config;
using log4net;

namespace Bot.Processors
{
    [Export(typeof(Processor))]
    class UrlTitles : AsyncProcessor
    {
        private ILog log = LogManager.GetLogger(typeof(UrlTitles));

        WebClient webClient = new WebClient();
        private List<Regex> urlPatterns = null;
        protected static Regex genericUrlPattern = new Regex(@"https?://\S+", RegexOptions.IgnoreCase);

        private UrlTitles()
        {
            webClient.CachePolicy = new System.Net.Cache.HttpRequestCachePolicy(System.Net.Cache.HttpRequestCacheLevel.CacheIfAvailable);
            webClient.Proxy = null;
        }

        public UrlTitles(List<Regex> urlPatterns) : this()
        {
            this.urlPatterns = urlPatterns;
        }

        [ImportingConstructor]
        public UrlTitles([Import("Config")] IConfig config) : this()
        {
            string[] patternStrings = config.GetString("title-whitelist").Split(',');

            urlPatterns = new List<Regex>();
            foreach (string patternString in patternStrings)
                urlPatterns.Add(new Regex(patternString.Trim()));
        }

        protected override void Worker(IrcEventArgs e)
        {
            if (e.Data.Message.Contains("http"))
            {
                if (e.Data.MessageArray[0].StartsWith(e.Data.Irc.Nickname + ":") || e.Data.MessageArray[0].StartsWith("!link-title")) // TODO: split to separate command?
                {
                    ProcessTitles(e.Data.Irc, e.Data.Channel, e.Data.Message, genericUrlPattern);
                }
                else
                {
                    if (urlPatterns != null && urlPatterns.Count > 0)
                        ProcessTitles(e.Data.Irc, e.Data.Channel, e.Data.Message, urlPatterns);
                    else
                        ProcessTitles(e.Data.Irc, e.Data.Channel, e.Data.Message, genericUrlPattern);
                }
            }
        }

        /// <summary>
        /// Looks for URLs in ircMessage and prints out the inner text in the title element
        /// </summary>
        protected void ProcessTitles(IrcClient irc, string destinationChannel, string ircMessage, List<Regex> whitelist)
        {
            foreach (Regex re in whitelist)
            {
                ProcessTitles(irc, destinationChannel, ircMessage, re);
            }
        }

        /// <summary>
        /// Looks for URLs in ircMessage and prints out the inner text in the title element
        /// </summary>
        /// <param name="irc">The current irc connection</param>
        /// <param name="destinationChannel">Channel which to respond to</param>
        /// <param name="ircMessage">Message to parse</param>
        protected void ProcessTitles(IrcClient irc, string destinationChannel, string ircMessage, Regex urlMatcher)
        {
            MatchCollection matches = urlMatcher.Matches(ircMessage);
            foreach (Match match in matches)
            {
                string message = "";
                HtmlDocument doc = new HtmlDocument();

                try
                {
                    string html = webClient.DownloadString(match.Value);//HtmlHelper.GetFromUrl(match.Value);
                    doc.LoadHtml(html);
                }
                catch (Exception e)
                {
                    log.Error("Error downloading HTML", e);
                    return;
                }

                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");

                if (titleNode != null && !string.IsNullOrEmpty(titleNode.InnerText))
                {
                    string title = titleNode.InnerText;
                    title = WebUtility.HtmlDecode(title);

                    StringBuilder sb = new StringBuilder();
                    string[] parts = title.Split(new char[] { ' ', '\n', '\t', '\r', '\f', '\v' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < parts.Length; i++)
                        sb.AppendFormat("{0} ", parts[i]);

                    title = sb.ToString().Trim();

                    message = "Title: " + title;
                }
                else
                    log.Warn("Could not find title in HTML-document");

                if (message.Length > 0)
                    irc.SendMessage(SendType.Message, destinationChannel, message);
            }
        }
    }
}
