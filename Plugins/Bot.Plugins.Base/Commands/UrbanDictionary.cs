﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using Bot.Core;
using Bot.Core.Commands;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    [CommandAttributes("Urban Dictionary", true, "ud", "urbandictionary")]
    class UrbanDictionary : Command
    {
        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            string url = "http://www.urbandictionary.com/define.php?term=" + e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault(); // TODO: URL encode
            string html = "";

            try
            {
                html = HttpHelper.GetFromUrl(url);
            }
            catch
            {
                return new [] { "Error fetching result" };
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNode node = doc.DocumentNode.SelectSingleNode("//table [@id = 'entries']/descendant::td [starts-with(@id, 'entry_')]/div [@class = 'definition']");
            
            IList<string> lines = new List<string>();
            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                lines.Add(("UrbanDictionary: " + WebUtility.HtmlDecode(node.InnerText)).FormatToIrc());
            else
                lines.Add("No results found");

            return lines;
        }
    }
}
