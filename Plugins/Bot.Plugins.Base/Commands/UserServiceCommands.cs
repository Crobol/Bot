using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    [Export(typeof(ICommand))]
    class AddUser : Command
    {
        UserService userService;

        [ImportingConstructor]
        public AddUser([Import("UserService")] UserService userService)
        {
            this.userService = userService;
        }

        public override string Name
        {
            get { return "Add User"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "au", "add-user" }; }
        }

        public override string Help
        {
            get { return "Adds user to database. Parameters: <username> <password> [<userlevel> = 1]"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            if (e.Data.MessageArray.Count() < 3)
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "Missing parameters");
                return;
            }

            User user = userService.GetAuthenticatedUser(e.Data.From);

            if (user != null && user.UserLevel == 10)
            {
                int userLevel = 1;
                if (e.Data.MessageArray.Count() > 3)
                    int.TryParse(e.Data.MessageArray[3], out userLevel);

                if (userService.CreateUser(e.Data.MessageArray[1], e.Data.MessageArray[2], userLevel) > 0)
                {
                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "User created");
                }
                else
                {
                    e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "User already exists");
                }
            }
            else
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "You do not have authorization to use this command");
                return;
            }
        }
    }

    [Export(typeof(ICommand))]
    class AuthenticateUser : Command
    {
        UserService userService;

        [ImportingConstructor]
        public AuthenticateUser([Import("UserService")] UserService userService)
        {
            this.userService = userService;
        }

        public override string Name
        {
            get { return "Authenticate"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "auth", "authenticate", "login" }; }
        }

        public override string Help
        {
            get { return "Authenticates user until user leaves all channels where the bot is present. Parameters: <username> <password>"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            if (e.Data.MessageArray.Count() < 3)
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "Invalid syntax");
                return;
            }

            if (userService.AuthenticateUser(e.Data.MessageArray[1], e.Data.MessageArray[2], e.Data.Nick, e.Data.From))
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "Authenticated");
            }
            else
            {
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "Error");
            }
        }
    }

    [Export(typeof(ICommand))]
    class ListAuthenticatedUsers : Command
    {
        UserService userService;

        [ImportingConstructor]
        public ListAuthenticatedUsers([Import("UserService")] UserService userService)
        {
            this.userService = userService;
        }

        public override string Name
        {
            get { return "List Authenticated Users"; }
        }

        public override string[] Aliases
        {
            get { return new string[] { "lau", "list-authed-users", "list-authenticated-users" }; }
        }

        public override string Help
        {
            get { return "Lists all authenticated users"; }
        }

        public override void Execute(IrcEventArgs e)
        {
            IList<User> users = userService.GetAuthenticatedUsers();
            string message = "Users: ";
            foreach (var user in users)
            {
                message += user.Username;
                if (user != users.Last())
                    message += ", ";
            }
            e.Data.Irc.SendMessage(SendType.Message, e.Data.Channel, message);
        }
    }
}
