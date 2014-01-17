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
    // TODO: Register scripts as ScriptCommand (or maybe PythonCommand) : ICommand which points to the script and invokes it through command line? And instead use specialized CommandLoaders (PythonScriptLoader, PluginLoader etc)

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
            foreach (string file in files)
            {
                try
                {
                    string name = string.Join("", Path.GetFileNameWithoutExtension(file));
                    scripts.Add(name.ToLower(), file);
                    log.Info("Found script \"" + name + "\"");
                }
                catch (Exception e)
                {
                    log.Error("Failed to load script file \"" + file + "\"", e);
                }
            }
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            string commandName = message.Command;
            if (!scripts.ContainsKey(commandName))
            {
                var matches = scripts.Keys.Where(x => x.StartsWith(commandName)).ToList();
                if (matches.Any())
                {
                    commandName = matches.First();
                }
                else
                {
                    log.Debug("Command not found");
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(commandName) && scripts.ContainsKey(commandName))
            {
                 string args = "";

                if (message.IrcEventArgs.Data.MessageArray.Length > 1)
                    args = string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(1));

                args = args.Replace("\'", "");
                args = args.Replace(">", "");
                args = args.Replace("<", "");
                args = args.Replace("|", "");

                try
                {
                    var proc = new System.Diagnostics.Process();
                    proc.EnableRaisingEvents = false;
                    proc.StartInfo.FileName = @"python";
                    proc.StartInfo.Arguments = scripts[commandName] + " \"" + args + "\"";
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.StandardOutputEncoding = Encoding.UTF8;
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

        private void Reload()
        {
            throw new NotImplementedException();
        }
    }
}
