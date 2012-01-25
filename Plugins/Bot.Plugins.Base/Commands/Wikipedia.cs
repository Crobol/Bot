﻿using System;
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

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string subject = string.Join(" ", e.Data.MessageArray.Skip(1));
            subject = subject.Replace(" ", "_");
            subject = subject.UppercaseFirst();

            IList<string> lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(subject))
            {
                string url = "https://en.wikipedia.org/wiki/" + subject;
                string message = FetchWikipedia(url);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    lines.Add(message.FormatToIrc());
                    lines.Add(url);
                }
            }

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
                string html = HtmlHelper.GetFromUrl(url);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode node = doc.DocumentNode.SelectSingleNode("//div [@id = 'bodyContent']/div/p");

                if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                {
                    string message = "Wiki: " + node.InnerText;
                    message = WebUtility.HtmlDecode(message).Trim();

                    return message;
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
