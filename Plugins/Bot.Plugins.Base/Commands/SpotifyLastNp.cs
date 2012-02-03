using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using log4net;
using Newtonsoft.Json;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    class SpotifyLastNp : AsyncCommand
    {
        private ILog log = LogManager.GetLogger(typeof(SpotifyLastNp));

        ConcurrentQueue<string> responseBuffer;

        private readonly static Regex artistPattern = new Regex(@"np: ([\d\w\s]+) –", RegexOptions.IgnoreCase);
        private readonly static Regex trackPattern = new Regex(@"– ([\d\w\s]+) \[", RegexOptions.IgnoreCase);

        [ImportingConstructor]
        public SpotifyLastNp([Import("ResponseBuffer")] ConcurrentQueue<string> responseBuffer)
        {
            this.responseBuffer = responseBuffer;
        }

        public override string Name
        {
            get { return "Spotify Last NP"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "snp" }; }
        }

        protected override CommandCompletedEventArgs DoWork(IrcEventArgs e)
        {
            string np = responseBuffer.LastOrDefault(x => x.StartsWith("np: "));
            if (np == null)
                return null;

            Match artist = artistPattern.Match(np);
            Match track = trackPattern.Match(np);

            string result = HtmlHelper.GetFromUrl("http://ws.spotify.com/search/1/track.json?q=" + artist.Groups[1] + "+" + track.Groups[1]);

            SpotifyMetaInfo meta = JsonConvert.DeserializeObject<SpotifyMetaInfo>(result);
            Track trackInfo = meta.Tracks.FirstOrDefault();
            if (trackInfo != null)
            {
                string uid = trackInfo.Href.Split(':').LastOrDefault();
                if (!string.IsNullOrWhiteSpace(uid))
                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, "http://open.spotify.com/track/" + uid);
            }

            return null;
        }
    }
}
