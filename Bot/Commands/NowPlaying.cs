using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class NowPlaying : AsyncCommand
    {
        public NowPlaying()
        {

        }

        public override string Name()
        {
            return "!np";
        }

        protected override AsyncCommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string nick = "";
            string message = "";

            if (e.Data.MessageArray.Count() > 1)
                nick = e.Data.MessageArray[1];
            else
                nick = e.Data.Nick;

            if (!string.IsNullOrWhiteSpace(nick))
                message = FetchNowPlayingInfo(nick);

            AsyncCommandCompletedEventArgs completedArgs = new AsyncCommandCompletedEventArgs(e.Data.Channel, message);

            return completedArgs;
        }

        /// <summary>
        /// Fetches and parses the Now Playing information from http://last.fm/user/lastfmUsername
        /// </summary>
        /// <param name="irc">The current irc connection</param>
        /// <param name="destinationChannel">Channel which to respond to</param>
        /// <param name="lastfmUsername">Last.fm username to fetch from</param>
        protected string FetchNowPlayingInfo(string lastfmUsername)
        {
            try
            {
                string html = HtmlHelper.GetFromUrl("http://last.fm/user/" + lastfmUsername);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode subjectNode = doc.DocumentNode.SelectSingleNode("//table [@id = 'recentTracks']/descendant::td [contains(@class, 'subjectCell') and contains(@class, 'highlight')]");

                if (subjectNode != null && !string.IsNullOrWhiteSpace(subjectNode.InnerText))
                {
                    string message = "np: " + subjectNode.InnerText.Trim();
                    message = WebUtility.HtmlDecode(message);
                    return message;
                }
                else
                    Console.WriteLine("Error | Could not find Now Playing information in last.fm page");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error | Could not get Now Playing information | Exception: " + e.Message);
            }

            return "";
        }
    }
}
