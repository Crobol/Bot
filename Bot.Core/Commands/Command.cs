using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meebey.SmartIrc4net;

namespace Bot.Core.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandAttributes : Attribute
    {
        private readonly string name;
        private readonly string[] aliases;
        private readonly bool async;

        public string Name { get { return name; } }
        public string[] Aliases { get { return aliases; } }
        public bool Async { get { return async; } }

        public CommandAttributes(string name)
        {
            this.name = name;
            this.async = false;
            this.aliases = new[] { name.ToLower() };
        }

        public CommandAttributes(string name, params string[] aliases)
        {
            this.name = name;
            this.async = false;
            this.aliases = aliases;
        }

        public CommandAttributes(string name, bool async, params string[] aliases)
        {
            this.name = name;
            this.async = async;
            this.aliases = aliases;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CommandDescription : Attribute
    {
        private readonly string description;

        public string Description { get { return description; } }

        public CommandDescription(string description)
        {
            this.description = description;
        }
    }

    public abstract class Command : ICommand
    {
        private readonly string name;
        private readonly bool async;
        private readonly string[] aliases;
        private readonly string description;

        public string Name { get { return name; } }
        public bool Async { get { return async; } }
        public string[] Aliases { get { return aliases; } }
        public string Description { get { return description; } }

        protected Command()
        {
            CommandAttributes attributes = this.GetType().GetAttributeValue((CommandAttributes x) => x);
            if (attributes == null)
            {
                throw new Exception("CommandAttributes missing.");
            }

            name = attributes.Name;
            async = attributes.Async;
            aliases = attributes.Aliases;

            CommandDescription description = this.GetType().GetAttributeValue((CommandDescription x) => x);
            if (description != null)
            {
                this.description = description.Description;
            }
            else
            {
                this.description = "No description for this command.";
            }
        }

        protected Command(string name, bool async, string[] aliases)
        {
            this.name = name;
            this.async = async;
            this.aliases = aliases;
            this.description = "No description for this command.";
        }

        protected Command(string name, bool async, string[] aliases, string description) : this(name, async, aliases)
        {
            this.description = description;
        }

        public abstract IEnumerable<string> Execute(IrcEventArgs e);
    }
}
