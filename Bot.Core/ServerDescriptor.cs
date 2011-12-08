using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core
{
    class ServerDescriptor
    {
        public ServerDescriptor(string host, int port = 6667, bool useSsl = false)
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
        }

        public ServerDescriptor(string host, int port, bool useSsl, string[] channels)
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
            this.Channels = channels;
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string[] Channels { get; set; }
    }
}
