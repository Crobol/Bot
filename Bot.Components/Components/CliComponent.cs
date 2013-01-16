using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bot.Core.Component;
using Bot.Core.Messages;
using log4net;
using TinyMessenger;

namespace Bot.Components
{
    public class CliComponent : Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(CliComponent));

        private readonly Dictionary<string, string> scripts = new Dictionary<string, string>();

        public CliComponent(ITinyMessengerHub hub)
            : base(hub)
        {
            hub.Subscribe<InvokeCommandMessage>(OnBotCommandMessage); 
            
            log.Info("Registering Python scripts...");

            string[] files = Directory.GetFiles("Scripts", "*.py");
            var sb = new StringBuilder();
            foreach (string file in files)
            {
                try
                {
                    string name = string.Join("", Path.GetFileNameWithoutExtension(file));
                    scripts.Add("!" + name, file);
                }
                catch (Exception e)
                {
                    log.Error("Failed to load script file \"" + file + "\"", e);
                }
            }
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            if (message.IrcEventArgs.Data.MessageArray.Length == 1) // TODO: Fix this
                return;

            string args = string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(1));

            if (scripts.ContainsKey(message.Command))
            {
                try
                {
                    var proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.FileName = @"python";
                    proc.StartInfo.Arguments = scripts[message.Command] + " " + args;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    proc.StartInfo.UseShellExecute = false;
                    proc.Start();
                    string data = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();

                    message.IrcEventArgs.Data.Irc.SendMessage(Meebey.SmartIrc4net.SendType.Message, message.IrcEventArgs.Data.Channel, data);
                }
                catch (Exception ex)
                {
                    log.Debug(ex);
                }
            }
        }
    }
}
