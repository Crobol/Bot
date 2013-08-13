using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using Bot.Core;
using Bot.Core.Messages;
using log4net;
using Meebey.SmartIrc4net;
using TinyMessenger;

namespace Bot.Components
{
    internal class Task
    {
        public string TargetServer { get; set; }
        public string Target { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }

    internal class TaskTimeComparer : IComparer<Task>
    {
        public int Compare(Task a, Task b)
        {
            if (a.Time > b.Time)
            {
                return 1;
            }
            else if (a.Time < b.Time)
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
        private readonly C5.IPriorityQueue<Task> tasks = new C5.IntervalHeap<Task>(10, new TaskTimeComparer());
        private readonly Timer timer = new Timer();

        public TaskComponent(ITinyMessengerHub hub, IPersistentStore store)
            : base(hub)
        {
            hub.Subscribe<InvokeCommandMessage>(this.HandleCommandMessage);
        }

        private void HandleCommandMessage(InvokeCommandMessage message)
        {
            if (message.Command.ToLower() == "!r" && message.IrcEventArgs.Data.MessageArray.Length > 2)
            {
                string when = message.IrcEventArgs.Data.MessageArray[1];
                string taskMessage = string.Join(" ", message.IrcEventArgs.Data.MessageArray.Skip(2));

                DateTime dateTime;
                try
                {
                    dateTime = ParseDateTime(when);
                }
                catch (Exception e)
                {
                    log.Debug("Invalid date format", e);
                    return;
                }

                var task = new Task()
                    {
                        Message = taskMessage,
                        Target = message.IrcEventArgs.Data.Channel,
                        TargetServer = message.IrcEventArgs.Data.Irc.Address,
                        Time = dateTime
                    };

                tasks.Add(task);

                SetTimer(tasks.Min().Time);
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

            timer.Stop();

            if (tasks.Any())
            {
                var time = tasks.Min(x => x.Time);

                SetTimer(time);
            }
        }

        private void SetTimer(DateTime time)
        {
            if (timer.Enabled)
                timer.Stop();

            timer.Interval = (time - DateTime.Now).TotalMilliseconds;
            timer.Elapsed += OnTime;
            timer.Start();
        }

        private Regex dateTimeFormat = new Regex(@"(\d{4})");

        private DateTime ParseDateTime(string s)
        {
            var dateTime = DateTime.ParseExact(s, "yyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0);

            /*var match = dateTimeFormat.Match(s);

            if (match.Success && match.Groups.Count > 1)
            {
                DateTime ymd;
                string hourMinutes;

                if (match.Groups.Count == 3)
                {
                    var yearMonthDay = match.Groups[1].Value + "0000";
                    ymd = DateTime.ParseExact(yearMonthDay, "yyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);

                    hourMinutes = match.Groups[2].Value;
                }
                else
                {
                    ymd = DateTime.Now;
                    hourMinutes = "010101" + match.Groups[0].Value;
                }
                var hm = DateTime.ParseExact(hourMinutes, "yyMMddHHmm", System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat);

                return new DateTime(ymd.Year, ymd.Month, ymd.Day, hm.Hour, hm.Minute, 0);
            }
            else
            {
                throw new FormatException("Invalid date time format");
            }*/
        }
    }
}
