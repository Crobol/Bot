using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Bot.Core.Plugins;
using Bot.Core.Commands;
using IronPython.Runtime.Types;
using IronPython.Hosting;
using Meebey.SmartIrc4net;
using Nini.Config;

namespace Bot.Plugins.IronPython
{
    [Export(typeof(IPlugin))]
    public class IronPython : IPlugin
    {
        protected ScriptRuntime ipy = null;
        protected Dictionary<string, Command> commands = new Dictionary<string, Command>();
        protected IConfig config = null;
        string commandIdentifier = "!";

        public void Initialize(IConfig config)
        {
            this.config = config;
            commandIdentifier = config.GetString("command-identifier", "!");

            Console.WriteLine("Creating Python runtime...");
            ipy = Python.CreateRuntime();

            LoadScripts(config.GetString("script-folder", "Scripts"));

            Console.WriteLine("Plugin Loaded | Type: IronPython");
        }

        public void OnQueryMessage(object sender, IrcEventArgs e) { }

        public void OnChannelMessage(object sender, IrcEventArgs e) 
        {
            if (e.Data.Message.StartsWith(commandIdentifier) && commands.ContainsKey(e.Data.MessageArray[0]))
            {
                try
                {
                    commands[e.Data.MessageArray[0]].Execute(e);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error | Message: " + ex.Message);
                }
            }
            else if (e.Data.Message.StartsWith(commandIdentifier + "reload-scripts"))
            {
                LoadScripts(config.GetString("script-folder", "Scripts"));
            }
        }

        public void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e) { }
        public void OnRawMessage(object sender, IrcEventArgs e) { }

        void LoadScripts(string directory)
        {
            Console.WriteLine("Loading Python scripts...");

            if (commands != null)
                commands.Clear();

            string[] files = Directory.GetFiles(directory, "*.py");
            foreach (string file in files)
            {
                ScriptScope scope = null;
                try
                {
                    scope = ipy.ExecuteFile(file);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error | Failed to load Python-script \"" + file + "\" | " + e.Message);
                }

                if (scope == null)
                    continue;

                IEnumerable<string> variableNames = scope.GetVariableNames();
                foreach (string variable in variableNames.Where(x => !x.StartsWith("_")))
                {
                    // TODO: Cleaner way to load classes that inherits "Command"? Does it even need to be classes? Would simple callback functions suffice?
                    try
                    {
                        var pythonCommandType = scope.GetVariable(variable);
                        if (PythonType.Get__name__(pythonCommandType.__bases__[0]) == "Command")
                        {
                            var pythonCommand = ipy.Operations.CreateInstance(pythonCommandType);
                            commands.Add(commandIdentifier + pythonCommand.Name(), pythonCommand);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error | Failed to load Python class/function/module | " + e.Message);
                    }
                }
            }
        }
    }
}
