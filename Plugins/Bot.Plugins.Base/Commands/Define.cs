using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;
using Newtonsoft.Json.Linq;

namespace Bot.Plugins.Base.Commands
{
    class DefineOptions
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }

        [OptionName("l", "lang")]
        [OptionFullName("language")]
        [DefaultValue("en")]
        public string Language { get; set; }
    }

    [Export(typeof(ICommand))]
    [CommandAttributes("Define", true, "define")]
    class Define : Command
    {
        // 0 = lang, 1 = query
        private const string url = "https://www.google.com/dictionary/json?callback=a&sl={0}&tl={0}&q={1}";

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            DefineOptions options = OptionParser.Parse<DefineOptions>(e.Data.Message);

            string query = string.Format(url, options.Language, options.Query);
            
            try
            {
                string result = HttpHelper.GetFromUrl(query);
                string json = result.Substring(2, result.LastIndexOf('}') - 1).Replace("\\x", "\\u00");

                JObject o = JObject.Parse(json);
                var meaning = (string) o.SelectToken("primaries[0].entries[1].terms[0].text");

                if (!string.IsNullOrEmpty(meaning))
                    return new string[] {"Definition: " + meaning};
                else
                    return new string[] {"No definition found"};
            }
            catch (Exception ex)
            {
                return new string[0];
            }
        }
    }
}
