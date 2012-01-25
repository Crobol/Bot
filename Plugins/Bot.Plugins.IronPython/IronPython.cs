using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using Meebey.SmartIrc4net;
using Nini.Config;
using IronPython.Runtime.Types;
using IronPython.Hosting;
using Bot.Core;
using Bot.Core.Plugins;
using Bot.Core.Commands;
using log4net;

namespace Bot.Plugins.IronPython
{
    [Export(typeof(IPlugin))]
    public class IronPython : IPlugin
    {
        private ILog log = LogManager.GetLogger(typeof(IronPython));

        private ScriptEngine ipy = null;
        private Dictionary<string, ICommand> commands;
        private IConfig config = null;
        private UserService userService = null;
        string commandIdentifier = "!"; // TODO: Don't hard code this

        [ImportingConstructor]
        public IronPython([Import("Commands")] Dictionary<string, ICommand> commands, [Import("UserService")] UserService userService)
        {
            this.commands = commands;
            this.userService = userService;
        }

        public void Initialize(IConfig config)
        {
            log.Info("Initializing \"IronPython\" plugin...");

            this.config = config;
            commandIdentifier = config.GetString("command-identifier", "!");

            log.Info("Creating Python runtime...");
            ipy = Python.CreateEngine();

            LoadScripts(config.GetString("script-folder", "Scripts"));

            log.Info("Plugin \"IronPython\" loaded");
        }

        public void OnQueryMessage(object sender, IrcEventArgs e) { }
        
        public void OnChannelMessage(object sender, IrcEventArgs e)
        {
            if (e.Data.Message.StartsWith(commandIdentifier + "reload-scripts"))
                Initialize(config);
        }

        public void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e) { }
        public void OnRawMessage(object sender, IrcEventArgs e) { }

        void LoadScripts(string directory)
        {
            log.Info("Loading Python scripts...");

            string[] files = Directory.GetFiles(directory, "*.py");
            foreach (string file in files)
            {
                ScriptScope scope = null;
                try
                {
                    scope = ipy.CreateScope();
                    scope.SetVariable("userService", userService);

                    ScriptSource scriptSource = ipy.CreateScriptSourceFromFile(file);
                    CompiledCode compiledCode = scriptSource.Compile();
                    compiledCode.Execute(scope);
                }
                catch (Exception e)
                {
                    log.Error("Failed to load Python-script \"" + file + "\"", e);
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
                        //Console.WriteLine("Error | Failed to load Python class/function/module | " + e.Message);
                    }
                }
            }
        }
    }
}
