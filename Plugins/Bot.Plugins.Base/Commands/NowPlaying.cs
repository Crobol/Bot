using System;
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
using Nini.Config;
using log4net;

namespace Bot.Commands
{
    [Export(typeof(ICommand))]
    class NowPlaying : AsyncCommand
    {
        private ILog log = LogManager.GetLogger(typeof(NowPlaying));

        private UserSystem userSystem;

        private const string SystemName = "np";

        [ImportingConstructor]
        public NowPlaying([Import("UserSystem")] UserSystem userSystem, [Import("CommandCompletedEventHandler")] CommandCompletedEventHandler onCommandCompleted)
        {
            this.userSystem = userSystem;
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Now Playing"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "np", "now-playing" }; }
        }

        public override string Help
        {
            get { return "Fetches now playing information from last.fm. You can save your last.fm username by setting \"" + Name + ".<nickname>\" or when logged in \"" + Name + ".username\". Parameters: [<username>]"; }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            User user = userSystem.GetAuthenticatedUser(e.Data.From);

            string nick = "";
            string message = "";

            if (e.Data.MessageArray.Count() > 1)
            {
                nick = e.Data.MessageArray[1];
            }
            else
            {
                if (user != null)
                    nick = userSystem.GetUserSetting(user.ID, SystemName + ".username");
                else
                    nick = userSystem.GetUserSetting(null, SystemName + "." + e.Data.Nick);

                if (string.IsNullOrWhiteSpace(nick))
                    nick = e.Data.Nick;
            }

            if (!string.IsNullOrWhiteSpace(nick))
            {
                log.Info("Fetching now playing information for user \"" + nick + "\"");
                message = FetchNowPlayingInfo(nick);
                if (CloseCall())
                    message += " -- " + e.Data.Nick;
            }
            else
            {
                log.Warn("No nick found or specified");
            }

            CommandCompletedEventArgs completedArgs = new CommandCompletedEventArgs(e.Data.Channel, new List<string> { message });

            return completedArgs;
        }

        /// <summary>
        /// Fetches and parses the Now Playing information from http://last.fm/user/lastfmUsername
        /// </summary>
        /// <param name="lastfmUsername">Last.fm username to fetch from</param>
        protected string FetchNowPlayingInfo(string lastfmUsername)
        {
            try
            {
                string html = HttpHelper.GetFromUrl("http://last.fm/user/" + lastfmUsername);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode subjectNode = doc.DocumentNode.SelectSingleNode("//table [@id = 'recentTracks']/descendant::td [contains(@class, 'subjectCell') and contains(@class, 'highlight')]");

                if (subjectNode != null && !string.IsNullOrWhiteSpace(subjectNode.InnerText))
                {
                    string message = "np: " + subjectNode.InnerText.Trim();
                    message = WebUtility.HtmlDecode(message);

                    string[] trackInfo = subjectNode.InnerText.Trim().Split('–');
                    html = HttpHelper.GetFromUrl("http://last.fm/music/" + trackInfo[0].Trim().Replace(' ', '+'));

                    doc.LoadHtml(html);

                    HtmlNode tagsNode = doc.DocumentNode.SelectSingleNode("//div [@class = 'tags']/p/a");

                    if (tagsNode != null && !string.IsNullOrWhiteSpace(tagsNode.InnerText))
                    {
                        message += " [" + tagsNode.InnerText.Trim() + "]";
                    }

                    return message;
                }
                else
                    log.Warn("Could not find Now Playing information in last.fm user page");
            }
            catch (Exception e)
            {
                log.Warn("Could not get last.fm user page information", e);
            }

            return "";
        }
    }
}
