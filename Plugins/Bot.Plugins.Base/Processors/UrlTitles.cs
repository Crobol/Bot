using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bot.Core;
using Bot.Core.Processors;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using Nini.Config;
using log4net;

namespace Bot.Processors
{
    [Export(typeof (Processor))]
    internal class UrlTitles : AsyncProcessor
    {
        private ILog log = LogManager.GetLogger(typeof (UrlTitles));

        private readonly List<Regex> urlPatterns = null;
        private static readonly Regex genericUrlPattern = new Regex(@"https?://\S+", RegexOptions.IgnoreCase);

        public UrlTitles(List<Regex> urlPatterns)
        {
            this.urlPatterns = urlPatterns;
        }

        [ImportingConstructor]
        public UrlTitles([Import("Config")] IConfig config)
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
                IList<string> urls = ExtractUrls(e.Data.Message, genericUrlPattern);

                IList<string> titles;
                if (e.Data.MessageArray[0].StartsWith(e.Data.Irc.Nickname + ":") ||
                    e.Data.MessageArray[0].StartsWith("!title")) // TODO: split to separate command?
                {
                    titles = GetTitles(urls, new List<Regex> { genericUrlPattern });
                }
                else
                {
                    if (urlPatterns != null && urlPatterns.Count > 0)
                        titles = GetTitles(urls, urlPatterns);
                    else
                        titles = GetTitles(urls, new List<Regex> { genericUrlPattern });
                }

                foreach (string title in titles)
                {
                    string message = "Title: " + title;
                    if (ParallelCalls())
                        message += " -- " + e.Data.Nick;
                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
                    
                }

                if (e.Data.Channel == "<dummy string>")
                {
                    var webClient = new WebClient();
                    webClient.UploadValuesCompleted += (sender, args) =>
                    {
                        if (args.Result != null)
                            log.Info(Encoding.UTF8.GetString(args.Result));
                        else if (args.Error != null)
                            log.Error(args.Error);
                    };

                    foreach (string url in urls)
                    {
                        var values = new NameValueCollection();
                        values.Add("nick", "");
                        values.Add("key", "<dummy string>");
                        values.Add("url", url);
                        webClient.UploadValuesAsync(new Uri("https://<dummy string>"), values);
                    }
                }
            }
        }

        protected IList<string> ExtractUrls(string message, Regex urlMatcher)
        {
            var urls = new List<string>();
            MatchCollection matches = urlMatcher.Matches(message);
            foreach (Match match in matches)
            {
                urls.Add(match.Value);
            }
            return urls;
        }

        protected IList<string> GetTitles(IList<string> urls, IList<Regex> whitelist)
        {
            var titles = new List<string>();
            foreach (string url in urls)
            {
                if (whitelist.Any(x => x.IsMatch(url)))
                {
                    string title = GetTitle(url);
                    if (!string.IsNullOrEmpty(title))
                        titles.Add(title);
                }
            }
            return titles;
        }

        /// <summary>
        /// Looks for URLs in message and prints out the inner text in the title element
        /// </summary>
        protected IList<string> GetTitles(string message, List<Regex> whitelist)
        {
            var titles = new List<string>();
            foreach (Regex re in whitelist)
            {
                IList<string> matches = GetTitles(message, re);
                if (matches != null)
                    titles.AddRange(matches);
            }
            return titles;
        }

        /// <summary>
        /// Looks for URLs in ircMessage and prints out the inner text in the title element
        /// </summary>
        /// <param name="ircMessage">Message to parse</param>
        protected IList<string> GetTitles(string message, Regex urlMatcher)
        {
            var titles = new List<string>();
            MatchCollection matches = urlMatcher.Matches(message);
            foreach (Match match in matches)
            {
                HtmlDocument doc = new HtmlDocument();

                try
                {
                    string html = HttpHelper.GetFromUrl(match.Value);
                    doc.LoadHtml(html);
                }
                catch (Exception e)
                {
                    log.Error("Error downloading HTML for " + match.Value, e);
                    continue;
                }

                HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");

                if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
                {
                    string title = titleNode.InnerText;
                    title = WebUtility.HtmlDecode(title);

                    StringBuilder sb = new StringBuilder();
                    string[] parts = title.Split(new char[] { ' ', '\n', '\t', '\r', '\f', '\v' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < parts.Length; i++)
                        sb.AppendFormat("{0} ", parts[i]);

                    title = sb.ToString().Trim();
                    titles.Add(title);
                }
                else
                    log.Warn("Could not find title in HTML-document");
            }
            return titles;
        }

        protected string GetTitle(string url)
        {
            var doc = new HtmlDocument();

            try
            {
                string html = HttpHelper.GetFromUrl(url);
                doc.LoadHtml(html);
            }
            catch (Exception e)
            {
                log.Error("Error downloading HTML for " + url, e);
                return "";
            }

            HtmlNode titleNode = doc.DocumentNode.SelectSingleNode("//title");

            if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
            {
                string title = titleNode.InnerText;
                title = WebUtility.HtmlDecode(title);

                StringBuilder sb = new StringBuilder();
                string[] parts = title.Split(new char[] {' ', '\n', '\t', '\r', '\f', '\v'},
                                             StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < parts.Length; i++)
                    sb.AppendFormat("{0} ", parts[i]);

                return sb.ToString().Trim();
            }

            return "";
        }
    }
}
