using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bot.Core;
using Bot.Core.Commands;
using Meebey.SmartIrc4net;

namespace Bot.Commands
{
    class AddUser : Command
    {
        UserService userService;

        public AddUser(UserService userService)
        {
            this.userService = userService;
        }

        public override string Name()
        {
            return "add-user";
        }

        public override string Help()
        {
            return "Adds user to database. Parameters: <username> <password> [<userlevel> = 1]";
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
                e.Data.Irc.SendMessage(SendType.Message, e.Data.Nick, "Not authorized");
                return;
            }
        }
    }

    class AuthenticateUser : Command
    {
        UserService userService;

        public AuthenticateUser(UserService userService)
        {
            this.userService = userService;
        }

        public override string Name()
        {
            return "auth";
        }

        public override string Help()
        {
            return "Authenticates user until user leaves all channels where the bot is present. Parameters: <username> <password>";
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

    class ListAuthenticatedUsers : Command
    {
        UserService userService;

        public ListAuthenticatedUsers(UserService userService)
        {
            this.userService = userService;
        }

        public override string Name()
        {
            return "list-authed-users";
        }

        public override string Help()
        {
            return "Lists all authenticated users";
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
