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
        protected Dictionary<string, Command> commands;
        protected IConfig config = null;
        string commandIdentifier = "!";

        [ImportingConstructor]
        public IronPython([Import("commands")] Dictionary<string, Command> commands)
        {
            this.commands = commands;
        }

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
        public void OnChannelMessage(object sender, IrcEventArgs e) { }
        public void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e) { }
        public void OnRawMessage(object sender, IrcEventArgs e) { }

        void LoadScripts(string directory)
        {
            Console.WriteLine("Loading Python scripts...");

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
                            if (commands.ContainsKey(commandIdentifier + pythonCommand.Name()))
                                commands.Remove(commandIdentifier + pythonCommand.Name());
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
