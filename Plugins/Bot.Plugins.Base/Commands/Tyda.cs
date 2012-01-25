using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using HtmlAgilityPack;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    [Export(typeof(ICommand))]
    class Tyda : AsyncCommand
    {
        // TODO: Move command completed from AsyncCommand to Command to avoid this
        [ImportingConstructor]
        public Tyda([Import("CommandCompletedEventHandler")] CommandCompletedEventHandler onCommandCompleted)
        {
            this.CommandCompleted += onCommandCompleted;
        }

        public override string Name
        {
            get { return "Translate"; }
        }

        public override string[] Aliases
        {
            get
            {
                return new string[] { "t", "trans", "translate" };
            }
        }

        public override string Help
        {
            get { return "Makes a Tyda.se search and returns the first result. Parameters: <expression>"; }
        }

        protected override CommandCompletedEventArgs Worker(IrcEventArgs e)
        {
            string url = "http://tyda.se/search?form=1&w=" + e.Data.Message.Split(new char[] { ' ' }, 2).LastOrDefault(); // TODO: URL encode
            string html = HtmlHelper.GetFromUrl(url);

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode node = doc.DocumentNode.SelectSingleNode("//div [@class = 'tyda_content']/descendant::a [@id = 'tyda_transR6']");
            
            string message = "";

            if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                message = "Tyda.se: " + node.InnerText;
            else
                message = "No results found";

            return new CommandCompletedEventArgs(e.Data.Channel, message);
        } 
    }
}
