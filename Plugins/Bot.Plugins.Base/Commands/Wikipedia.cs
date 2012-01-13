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

namespace Bot.Commands
{
    [Export(typeof(Command))]
    class Wikipedia : AsyncCommand
    {
        // TODO: Make command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public Wikipedia([Import("AsyncCommandCompletedEventHandler")] AsyncCommand.AsyncCommandCompletedEventHandler onAsyncCommandCompleted)
        {
            this.CommandCompleted += onAsyncCommandCompleted;
        }

        public override string Name()
        {
            return "w";
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string subject = string.Join(" ", e.Data.MessageArray.Skip(1));
            subject = subject.Replace(" ", "_");
            subject = subject.UppercaseFirst();
            if (!string.IsNullOrWhiteSpace(subject))
            {
                string url = "https://en.wikipedia.org/wiki/" + subject;
                string message = FetchWikipedia(url);

                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, url);
            }

            return null;
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
                    if (message.Length > 500)
                    {
                        message = message.Substring(0, 500);
                    }
                    return message;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error | " + e.Message);
            }

            return "";
        }
    }
}
