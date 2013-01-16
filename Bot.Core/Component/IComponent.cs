using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core.Component
{
    public interface IComponent
    {
        void Initialize();
        void Shutdown();
    }
}
