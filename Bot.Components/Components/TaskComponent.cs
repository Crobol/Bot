using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Bot.Core;
using Bot.Core.Messages;
using Chronic;
using log4net;
using Meebey.SmartIrc4net;
using TinyMessenger;

namespace Bot.Components
{
    internal class Reminder : IDisposable
    {
        public string TargetServer { get; set; }
        public string Target { get; set; }
        public string Creator { get; set; }
        public DateTime When { get; set; }
        public Timer Timer { get; set; }
        public string Message { get; set; }

        private bool disposed = false;

        #region Implement IDisposable

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && Timer != null)
                {
                    Timer.Close();
                    Timer.Dispose();
                }

                Timer = null;
                disposed = true;
            }
        }

        #endregion
    }

    internal class TaskTimeComparer : IComparer<Reminder>
    {
        public int Compare(Reminder a, Reminder b)
        {
            if (a.When > b.When)
            {
                return 1;
            }
            else if (a.When < b.When)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class TaskComponent : Core.Component.Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(TaskComponent));
        private readonly C5.IPriorityQueue<Reminder> tasks = new C5.IntervalHeap<Reminder>(10, new TaskTimeComparer());
        private readonly Timer timer = new Timer();

        public TaskComponent(ITinyMessengerHub hub, IPersistentStore store)
            : base(hub)
        {
            hub.Subscribe<InvokeCommandMessage>(this.HandleCommandMessage);

            timer.AutoReset = false;
            timer.Elapsed += OnTime;
        }

        private void HandleCommandMessage(InvokeCommandMessage message)
        {
            if (message.Command.ToLower() == "r" && message.IrcEventArgs.Data.MessageArray.Length > 2)
            {
                string taskMessage = GetDescription(message.IrcEventArgs.Data.Message); //string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(2));

                DateTime when;
                try
                {
                    var line = message.IrcEventArgs.Data.Message;
                    when = ParseDateTime(line.Substring(line.LastIndexOf('@') + 1, line.Length - line.LastIndexOf('@') - 1));
                }
                catch (Exception e)
                {
                    log.Debug("Invalid date format", e);
                    return;
                }

                var timer = new Timer {AutoReset = false};
                timer.Elapsed += OnTime;
                if (when < DateTime.Now)
                    timer.Interval = 1;
                else
                    timer.Interval = (when - DateTime.Now).TotalMilliseconds;

                var task = new Reminder()
                    {
                        Message = taskMessage,
                        Target = message.IrcEventArgs.Data.Channel,
                        Creator = message.IrcEventArgs.Data.Nick,
                        TargetServer = message.IrcEventArgs.Data.Irc.Address,
                        When = when,
                        Timer = timer
                    };

                tasks.Add(task);
                task.Timer.Start();

                hub.Publish(new IrcSendMessage(this, SendType.Message, task.TargetServer, message.IrcEventArgs.Data.Channel, "Okidok!"));
            }
        }

        private void OnTime(object sender, ElapsedEventArgs e)
        {
            if (!tasks.Any())
            {
                log.Warn("No tasks in queue");
                return; 
            }

            var task = tasks.DeleteMin();

            hub.Publish(new IrcSendMessage(this, SendType.Message, task.TargetServer, task.Target, "Reminder: " + task.Message));

            task.Dispose();
        }

        private void SetTimer(DateTime time)
        {
            timer.Stop();

            if (time < DateTime.Now)
                timer.Interval = 1;
            else
                timer.Interval = (time - DateTime.Now).TotalMilliseconds;

            timer.Start();
        }

        private DateTime ParseDateTime(string s)
        {
            Parser parser = new Parser();
            return parser.Parse(s).ToTime();
        }

        private string GetDescription(string line)
        {
            if (string.IsNullOrEmpty(line) || !line.Contains("@"))
                throw new FormatException("Invalid reminder string format");

            return line.Substring(line.IndexOf(' '), line.LastIndexOf('@') - line.IndexOf(' ')).Trim();
        }
    }
}
