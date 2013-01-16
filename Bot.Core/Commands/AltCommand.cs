using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AltCommandAttributes : Attribute
    {
        private readonly string name;
        private readonly string[] aliases;
        private readonly bool async;

        public string Name { get { return name; } }
        public string[] Aliases { get { return aliases; } }
        public bool Async { get { return async; } }

        public AltCommandAttributes(string name)
        {
            this.name = name;
            this.async = false;
            this.aliases = new[] { name.ToLower() };
        }

        public AltCommandAttributes(string name, params string[] aliases)
        {
            this.name = name;
            this.async = false;
            this.aliases = aliases;
        }

        public AltCommandAttributes(string name, bool async, params string[] aliases)
        {
            this.name = name;
            this.async = async;
            this.aliases = aliases;
        }
    }

    public abstract class AltCommand : IAltCommand
    {
        //public virtual string Help { get { return "No help message available for this command"; } }
        //public virtual string Signature { get { return Aliases.Aggregate((o, x) => o += "|" + x) + " <params...>"; } }

        private string name;
        private bool async;
        private string[] aliases;

        public string Name { get { return name; } }
        public bool Async { get { return async; } set { this.async = value; } }
        public string[] Aliases { get { return aliases; } }

        protected AltCommand()
        {
            AltCommandAttributes attributes = this.GetType().GetAttributeValue((AltCommandAttributes x) => x);
            if (attributes == null)
            {
                throw new Exception("AltCommandAttributes missing.");
            }

            name = attributes.Name;
            async = attributes.Async;
            aliases = attributes.Aliases;
        }

        protected AltCommand(string name, bool async, string[] aliases)
        {
            this.name = name;
            this.async = async;
            this.aliases = aliases;
        }

        public abstract IEnumerable<string> Execute(IrcEventArgs e);
    }
}
