using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Bot.Core.Processors;
using Bot.Core.Component;
using Bot.Core.Messages;
using Meebey.SmartIrc4net;
using log4net;
using Nini.Config;
using TinyMessenger;

namespace Bot.Components
{
    public class ProcessorComponent : Component
    {
        private readonly ILog log = LogManager.GetLogger(typeof(ProcessorComponent));

        [ImportMany]
        private IEnumerable<Processor> Processors { get; set; }

        public ProcessorComponent(ITinyMessengerHub hub, IConfig config) : base(hub)
        {
            log.Info("Initializing processor component...");

            hub.Subscribe<IrcMessage>(OnIrcMessage);

            using (var catalog = new DirectoryCatalog("Plugins"))
            {
                using (var container = new CompositionContainer(catalog))
                {
                    container.ComposeExportedValue<IConfig>("Config", config);
                    container.ComposeParts(this);
                }
            }
        }

        private void OnIrcMessage(IrcMessage message)
        {
            foreach (Processor processor in Processors)
            {
                processor.Execute(message.ircEventArgs);
            }
        }
    }
}
