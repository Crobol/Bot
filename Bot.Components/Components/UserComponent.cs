using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bot.Core;

namespace Bot.Components
{
    class UserComponent : Component
    {
        private UserSystem userSystem;

        public UserComponent(TinyMessenger.ITinyMessengerHub hub, UserSystem userSystem) : base(hub)
        {
            this.userSystem = userSystem;
        }


        public override void ProcessMessage(object sender, Message message)
        {
            AuthenticateUserMessage authenticateUserMessage = message as AuthenticateUserMessage;
            if (authenticateUserMessage != null)
            {
                userSystem.AuthenticateUser(authenticateUserMessage.username, authenticateUserMessage.password, authenticateUserMessage.nick, authenticateUserMessage.ident);
            }

            AuthenticatedInvokeMessage authenticatedInvokeMessage = message as AuthenticatedInvokeMessage;
            if (authenticatedInvokeMessage != null)
            {
                if (userSystem.IsAuthenticated(authenticatedInvokeMessage.ident))
                    authenticatedInvokeMessage.function();
            }
        }
    }
}
