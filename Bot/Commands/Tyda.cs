using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class Tyda : AsyncCommand
    {
        public override string Name()
        {
            return "t";
        }

        protected override AsyncCommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string url = "http://tyda.se/search?form=1&w=" + e.Data.MessageArray.LastOrDefault();
            string html = HtmlHelper.GetFromUrl(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div [@class = 'tyda_content']/descendant::a [@id = 'tyda_transR6']");
            
            return new AsyncCommandCompletedEventArgs(e.Data.Channel, "Tyda.se: " + node.InnerText);
        }
    }
}
