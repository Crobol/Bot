using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TinyMessenger;

namespace Bot.Core.Component
{
    public abstract class Component : IComponent
    {
        protected ITinyMessengerHub hub;

        protected Component(ITinyMessengerHub hub)
        {
            this.hub = hub;
        }

        public virtual void Initialize() { }
        public virtual void Shutdown() { }
    }
}
