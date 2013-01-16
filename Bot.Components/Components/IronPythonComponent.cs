using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bot.Core.Component;
using Bot.Core.Messages;
//using IronPython.Runtime.Types;
//using IronPython.Hosting;
//using Microsoft.Scripting.Hosting;
using Meebey.SmartIrc4net;
using log4net;
using TinyMessenger;

namespace Bot.Components
{
    /*class IronPythonComponent : Component
    {
        private ILog log = LogManager.GetLogger(typeof(IronPythonComponent));

        private ScriptEngine ipy = null;
        private ScriptScope scope = null;

        public IronPythonComponent(ITinyMessengerHub hub) 
            : base(hub)
        {
            ipy = IronPython.Hosting.Python.CreateEngine();
            LoadScripts("Scripts");

            hub.Subscribe<InvokeCommandMessage>(OnBotCommandMessage);
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            ProcessCommand(message.Command, message.IrcEventArgs);
        }

        private void ProcessCommand(string command, IrcEventArgs e)
        {
            dynamic func = null; 

            string commandName = command.Substring(1);

            try
            {
                if (scope.TryGetVariable(commandName, out func))
                {
                    try
                    {
                        func();
                        dynamic returnValue = null;
                        if (e.Data.MessageArray.Length > 1)
                            returnValue = func(string.Join(" ", e.Data.MessageArray.Skip(1)));
                        else
                            returnValue = func();

                        Console.WriteLine(returnValue);
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Could not invoke IronPython function", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Debug("Could not get IronPython variable", ex);
            }
        }

        void LoadScripts(string directory)
        {
            log.Info("Loading Python scripts...");

            scope = ipy.CreateScope();

            string[] files = Directory.GetFiles(directory, "*.py");
            foreach (string file in files)
            {
                try
                {
                    ScriptSource scriptSource = ipy.CreateScriptSourceFromFile(file);
                    CompiledCode compiledCode = scriptSource.Compile();
                    compiledCode.Execute(scope);
                }
                catch (Exception e)
                {
                    log.Error("Failed to load Python-script \"" + file + "\"", e);
                }
            }
        }
    }*/
}
