using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;

namespace Bot.Plugins.Base.Commands
{
    [Export(typeof(ICommand))]
    [CommandAttributes("Generate Archive Link", true, "links")]
    class GenerateArchiveLink : Command
    {
        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            if (e.Data.Channel != "<dummy string>") // Hardcode allowed channel, really really badly done
                return new string[0];

            var key = CalculateMD5Hash("<dummy string>").Substring(0, 12);
            var link = string.Format("https://user:{0}@<dummy string>/", key);

            return new [] { link };
        }

        protected string CalculateMD5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
