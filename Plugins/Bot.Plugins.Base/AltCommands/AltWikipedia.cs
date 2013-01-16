﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using log4net;

namespace Bot.Plugins.Base.AltCommands
{
    class WikipediaOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }

        [OptionName("l", "lang")]
        [OptionFullName("language")]
        [Core.DefaultValue("en")]
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

    [Export(typeof(IAltCommand))]
    [AltCommandAttributes("Wikipedia", true, new[] { "wikipedia" })]
    public class AltWikiCommand : AltCommand
    {
        private readonly ILog log = LogManager.GetLogger(typeof(AltWikiCommand));

        // 0 = lang, 1 = title
        private readonly string pageDataUrl = "https://{0}.wikipedia.org/w/api.php?action=query&prop=info|extracts&inprop=url&format=xml&exchars=400&explaintext&redirects&titles={1}";

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            WikipediaOptions options = OptionParser.Parse<WikipediaOptions>(e.Data.Message);

            ICollection<string> lines = GetResponse(options);

            if (!lines.Any())
                lines.Add("No article found");

            return lines;
        }

        private ICollection<string> GetResponse(WikipediaOptions options)
        {
            if (string.IsNullOrEmpty(options.Query))
                return new List<string>();

            string url = string.Format(pageDataUrl, options.Language, options.Query);
            WikipediaResponse response = null;

            try
            {
                string xml = HttpHelper.GetFromUrl(url);
                response = new WikipediaResponse(xml);
            }
            catch (Exception ex)
            {
                log.Debug(ex);
            }

            return (response != null && !string.IsNullOrEmpty(response.Article))
                       ? new List<string>() { "Wiki: " + response.Article, response.FullUrl }
                       : new List<string>();
        }
    }
}