using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Bot.Core;
using Bot.Core.Commands;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;
using log4net;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    [CommandAttributes("Now Playing", true, "n", "np", "nowplaying")]
    public class NowPlaying : Command
    {
        private ILog log = LogManager.GetLogger(typeof(NowPlaying));
        private IPersistentStore store;

        [ImportingConstructor]
        public NowPlaying([Import("Store")] IPersistentStore store)
        {
            this.store = store;
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            string nick = "";
            string message = "";

            if (e.Data.MessageArray.Count() > 1)
            {
                nick = e.Data.MessageArray[1];
            }
            else
            {
                nick = store.GetUserSetting<string>(e.Data.Nick, "LastfmUsername");

                if (string.IsNullOrWhiteSpace(nick))
                    nick = e.Data.Nick;
            }

            log.Info("Fetching now playing information for user \"" + nick + "\"");

            try
            {
                message = FetchNowPlayingInfo(nick);
            }
            catch (Exception)
            {
                return new string[0];
            }

            return new[] { message };
        }

        /// <summary>
        /// Fetches and parses the Now Playing information from http://last.fm/user/<lastfmUsername>
        /// </summary>
        /// <param name="lastfmUsername">Last.fm username to fetch from</param>
        protected string FetchNowPlayingInfo(string lastfmUsername)
        {
            string html;

            try
            {
                html = HttpHelper.GetFromUrl("http://last.fm/user/" + lastfmUsername);
            }
            catch (Exception e)
            {
                log.Warn("Could not get last.fm user page HTML.", e);
                throw;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode subjectNode = doc.DocumentNode.SelectSingleNode("//table [@id = 'recentTracks']/descendant::td [contains(@class, 'subjectCell') and contains(@class, 'highlight')]");

            if (subjectNode == null || string.IsNullOrWhiteSpace(subjectNode.InnerText))
            {
                log.Warn("Could not find Now Playing information in last.fm user page HTML.");
                throw new NullReferenceException();
            }

            string message = "np: " + subjectNode.InnerText.Trim();
            message = WebUtility.HtmlDecode(message);

            string[] trackInfo = subjectNode.InnerText.Trim().Split('–');
            html = HttpHelper.GetFromUrl("http://last.fm/music/" + trackInfo[0].Trim().Replace(' ', '+'));

            doc.LoadHtml(html);

            HtmlNode tagsNode = doc.DocumentNode.SelectSingleNode("//section [@class = 'global-tags']/ul/li/a");

            if (tagsNode != null && !string.IsNullOrWhiteSpace(tagsNode.InnerText))
            {
                message += " [" + tagsNode.InnerText.Trim() + "]";
            }

            return message;
        }
    }
}
