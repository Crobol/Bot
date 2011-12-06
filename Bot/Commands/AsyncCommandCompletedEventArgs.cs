using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Bot.Commands
{
    class AsyncCommandCompletedEventArgs : EventArgs
    {
        private string message;
        private string destination;

        public AsyncCommandCompletedEventArgs(string destination, string message)
        {
            this.destination = destination;
            this.message = message;
        }

        public string Destination
        {
            get
            {
                return destination;
            }
        }

        public string Message
        {
            get
            {
                return message;
            }
        }
    }
}
