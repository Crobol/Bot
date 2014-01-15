using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using WolframAPI;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    [CommandAttributes("WolframAlpha", true, "wa", "wolfram", "alpha")]
    public class Wolfram : Command
    {
        private const string WolframAppId = "";
        private readonly WAClient client = new WAClient(WolframAppId);

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            if (e.Data.MessageArray.Count() < 2)
                return null;

            var operation = string.Join(" ", e.Data.MessageArray.Skip(1));

            if (!string.IsNullOrEmpty(operation))
            {
                var result = client.Solve(operation);
                return new [] { "Result: " + result };
            }
            else
            {
                return new string[0];
            }
        }
    }
}
