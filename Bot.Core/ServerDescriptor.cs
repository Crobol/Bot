using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core
{
    public class ServerDescriptor
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public IList<string> Channels { get { return channels; } }
        public string Nick { get; set; }

        private IList<string> channels;

        public ServerDescriptor(string host, int port = 6667, bool useSsl = false, string nick = "")
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
            this.Nick = nick;
        }

        public ServerDescriptor(string host, int port, bool useSsl, IList<string> channels, string nick = "")
        {
            this.Host = host;
            this.Port = port;
            this.UseSsl = useSsl;
            this.channels = channels;
            this.Nick = nick;
        }
    }
}
