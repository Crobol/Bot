using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    public class PythonScriptCommand : Command
    {
        private string scriptPath { get; set; }

        public PythonScriptCommand(string scriptPath, string name, bool async, params string[] aliases) : base(name, async, aliases)
        {
            this.scriptPath = scriptPath;
        }

        public PythonScriptCommand(string scriptPath, string name, bool async, string[] aliases, string description)
            : base(name, async, aliases, description)
        {
            this.scriptPath = scriptPath;
        }

        public override IEnumerable<string> Execute(IrcEventArgs e)
        {
            string args = "";

            if (e.Data.MessageArray.Length > 1)
                args = string.Join(" ", e.Data.MessageArray.Skip(1));

            args = args.Replace("\'", "");
            args = args.Replace(">", "");
            args = args.Replace("<", "");
            args = args.Replace("|", "");

            try
            {
                var proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = @"python";
                proc.StartInfo.Arguments = scriptPath + " \"" + args + "\"";
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                proc.Start();
                string data = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                return new[] {data};
            }
            catch
            {
                
            }

            return null;
        }
    }
}
