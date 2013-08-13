using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Bot.Core.Component;
using Bot.Core.Messages;
using log4net;
using Python.Runtime;
using TinyMessenger;

namespace Bot.Components
{
    public class PythonComponent : Component, IDisposable
    {
        private ILog log = LogManager.GetLogger(typeof(PythonComponent));

        private PyObject module;

        private bool disposed = false;

        public PythonComponent(ITinyMessengerHub hub)
            : base(hub)
        {
            PythonEngine.Initialize();

            LoadScripts("Scripts");

            hub.Subscribe<InvokeCommandMessage>(OnBotCommandMessage);
        }

        private void OnBotCommandMessage(InvokeCommandMessage message)
        {
            string commandName = message.Command.Substring(1);

            try
            {
                string args = "";
                if (message.IrcEventArgs.Data.MessageArray.Length > 1)
                    args =  string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(1));

                PyString param = new PyString(args);
                IntPtr ptr = PythonEngine.AcquireLock();
                module.SetAttr(new PyString("sys.argv"), new PyString("param"));
                PyObject result = module.InvokeMethod(commandName, param);
                PythonEngine.ReleaseLock(ptr);

                hub.Publish<IrcSendMessage>(new IrcSendMessage(this, Meebey.SmartIrc4net.SendType.Message, message.IrcEventArgs.Data.Irc.Address, message.IrcEventArgs.Data.Channel, result.ToString())); 
            }
            catch (Exception ex)
            {
                log.Debug("Could not invoke Python function", ex);
            }
        }

        private void LoadScripts(string directory)
        {
            log.Info("Loading Python scripts...");

            string[] files = Directory.GetFiles(directory, "*.py");
            var sb = new StringBuilder();
            foreach (string file in files)
            {
                try
                {
                    string script = System.IO.File.ReadAllText(file);
                    sb.Append(script);                    
                }
                catch (Exception e)
                {
                    log.Error("Failed to load script file \"" + file + "\"", e);
                }
            }

            IntPtr ptr = PythonEngine.AcquireLock();
            module = PythonEngine.ModuleFromString("bot", sb.ToString());
            PythonEngine.ReleaseLock(ptr);
        }

        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && PythonEngine.IsInitialized)
                {
                    PythonEngine.Shutdown();   
                }
                disposed = true;
            }
        }

        #endregion
    }
}
