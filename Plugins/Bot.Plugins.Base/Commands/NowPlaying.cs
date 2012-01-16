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
    [Export(typeof(Command))]
    class NowPlaying : AsyncCommand
    {
        private ILog log = LogManager.GetLogger(typeof(NowPlaying));

        IConfig config;
        UserService userService;

        [ImportingConstructor]
        public NowPlaying([Import("Config")] IConfig config, [Import("UserService")] UserService userService, [Import("AsyncCommandCompletedEventHandler")] AsyncCommand.AsyncCommandCompletedEventHandler onAsyncCommandCompleted)
        {
            this.config = config;
            this.userService = userService;
            this.CommandCompleted += onAsyncCommandCompleted;
        }

        public override string Name()
        {
            return "np";
        }

        public override string Help()
        {
            return "Fetches now playing information from last.fm. You can save your last.fm username by setting \"" + Name() + ".<nickname>\" or when logged in \"" + Name() + ".username\". Parameters: [<username>]";
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            User user = userService.GetAuthenticatedUser(e.Data.From);

            string nick = "";
            string message = "";

            if (e.Data.MessageArray.Count() > 1)
            {
                nick = e.Data.MessageArray[1];
            }
            else
            {
                if (user != null)
                    nick = userService.GetUserSetting(user.ID, Name() + ".username");
                else
                    nick = userService.GetUserSetting(null, Name() + "." + e.Data.Nick);
            }

            if (!string.IsNullOrWhiteSpace(nick))
                message = FetchNowPlayingInfo(nick);

            CommandCompletedEventArgs completedArgs = new CommandCompletedEventArgs(e.Data.Channel, message);

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
                string html = HtmlHelper.GetFromUrl("http://last.fm/user/" + lastfmUsername);

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                HtmlNode subjectNode = doc.DocumentNode.SelectSingleNode("//table [@id = 'recentTracks']/descendant::td [contains(@class, 'subjectCell') and contains(@class, 'highlight')]");

                if (subjectNode != null && !string.IsNullOrWhiteSpace(subjectNode.InnerText))
                {
                    string message = "np: " + subjectNode.InnerText.Trim();
                    message = WebUtility.HtmlDecode(message);

                    string[] trackInfo = subjectNode.InnerText.Trim().Split('–');
                    html = HtmlHelper.GetFromUrl("http://last.fm/music/" + trackInfo[0].Trim().Replace(' ', '+'));

                    doc.LoadHtml(html);

                    HtmlNode tagsNode = doc.DocumentNode.SelectSingleNode("//div [@class = 'tags']/p/a");

                    if (tagsNode != null && !string.IsNullOrWhiteSpace(tagsNode.InnerText))
                    {
                        message += " [" + tagsNode.InnerText.Trim() + "]";
                    }

                    return message;
                }
                else
                    log.Warn("Could not find Now Playing information in last.fm page");
            }
            catch (Exception e)
            {
                log.Warn("Could not get Now Playing information", e);
            }

            return "";
        }
    }
}
