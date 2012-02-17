﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Text;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using Bot.Core;
using Bot.Core.Commands;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    class UrbanDictionary : AsyncCommand
    {
        // TODO: Move command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public UrbanDictionary([Import("CommandCompletedEventHandler")] CommandCompletedEventHandler onCommandCompleted)
        {
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Urban Dictionary"; }
        }

        public override string[]  Aliases
        {
	        get { return new string[] { "ud", "urban-dictionary" }; }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string url = "http://www.urbandictionary.com/define.php?term=" + e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault(); // TODO: URL encode
            string html = HtmlHelper.GetFromUrl(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//table [@id = 'entries']/descendant::td [starts-with(@id, 'entry_')]/div [@class = 'definition']");

            IList<string> lines = new List<string>();

            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
            {
                lines.Add(("UrbanDictionary: " + WebUtility.HtmlDecode(node.InnerText)).FormatToIrc());
                lines.Add(url);
            }
            else
                lines.Add("No results found");

            return new CommandCompletedEventArgs(e.Data.Channel, lines);
        }
    }
}
