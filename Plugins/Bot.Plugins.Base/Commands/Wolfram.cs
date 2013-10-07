using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using WolframAPI;

namespace Bot.Plugins.Base.Commands
{
    class WolframOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }

        [OptionName("a", "all")]
        [OptionFullName("all")]
        public bool All { get; set; }
    }

    [Export(typeof(ICommand))]
    [CommandAttributes("WolframAlpha", true, "wa", "wolfram", "alpha")]
    public class Wolfram : Command
    {
        private const string WolframAppId = "";
        private readonly WAClient client = new WAClient(WolframAppId);

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            var options = OptionParser.Parse<WolframOptions>(e.Data.Message);
            var operation = string.Join(" ", e.Data.MessageArray.Skip(1));
            var result = client.Solve(operation);
            return new string[] { "Result: " + result };
        }
    }
}
