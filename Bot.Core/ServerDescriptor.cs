using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core
{
    public class ServerDescriptor : ICloneable
    {
        public ServerDescriptor(string host, int port = 6667, bool useSsl = false, string nick = "")
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
            this.Nick = nick;
        }

        public ServerDescriptor(string host, int port, bool useSsl, string[] channels, string nick = "")
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
            this.Channels = channels;
            this.Nick = nick;
        }

        public Object Clone()
        {
            ServerDescriptor clone = new ServerDescriptor(Host, Port, UseSsl, Nick);
            clone.Channels = (string[])Channels.Clone();
            return clone;
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string[] Channels { get; set; }
        public string Nick { get; set; }
    }
}
