﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Bot.Core.Plugins;
using Meebey.SmartIrc4net;
using Nini.Config;

namespace Bot.Plugins.Logger
{
    [Export(typeof(IPlugin))]
    public class Logger : IPlugin, IDisposable
    {
        private FileStream rawLog;
        private bool disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && rawLog != null)
                {
                    rawLog.Flush();
                    rawLog.Close();
                    rawLog.Dispose();
                }

                rawLog = null;
                disposed = true;
            }
        }

        public void Initialize(IConfig config)
        {
            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            rawLog = new FileStream("Logs/raw.log", FileMode.Append, FileAccess.Write, FileShare.None, 512, true);

            Console.WriteLine("Plugin Loaded | Type: Logger");
        }

        public void OnJoin(object sender, JoinEventArgs e) { }
        public void OnQueryMessage(object sender, IrcEventArgs e) { }
        public void OnChannelMessage(object sender, IrcEventArgs e) { }
        public void OnError(object sender, Meebey.SmartIrc4net.ErrorEventArgs e) { }
        public void OnRawMessage(object sender, IrcEventArgs e) 
        {
            UTF8Encoding encoding =  (UTF8Encoding)Encoding.UTF8;
            IAsyncResult asyncResult = rawLog.BeginWrite(encoding.GetBytes(e.Data.RawMessage + "\n"), 0, encoding.GetByteCount(e.Data.RawMessage + "\n"), new AsyncCallback(EndWriteCallback), new State(rawLog));
        }

        private void EndWriteCallback(IAsyncResult asyncResult)
        {
            State state = (State)asyncResult.AsyncState;
            state.FileStream.EndWrite(asyncResult);
        }

        class State
        {
            FileStream stream;

            public State(FileStream fileStream)
            {
                this.stream = fileStream;
            }

            public FileStream FileStream
            {
                get
                {
                    return stream;
                }
            }
        }
    }
}
