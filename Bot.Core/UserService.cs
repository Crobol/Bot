using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bot.Core
{
    public class UserService
    {
        private BotEntities db = null;
        private IDictionary<string, User> authedUsers = new Dictionary<string, User>();
        SHA512CryptoServiceProvider sha512hasher = new SHA512CryptoServiceProvider();

        public UserService(BotEntities db)
        {
            this.db = db;
        }

        public bool AuthenticateUser(string username, string password, string nick, string ident)
        {
            SHA512CryptoServiceProvider sha512hasher = new SHA512CryptoServiceProvider();
            string hash = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();

            var users = from x in db.Users where x.Username == username && x.Password == hash select x;

            if (users.Any())
            {
                var user = users.First();
                authedUsers[ident] = user;

                user.LastNick = nick;
                user.LastIdent = ident;

                db.SaveChanges();

                return true;
            }
            else
                return false;
        }

        public bool DeauthenticateUser(string ident)
        {
            return authedUsers.Remove(ident);
        }

        public bool IsAuthenticated(string ident)
        {
            return authedUsers.ContainsKey(ident);
        }

        public User GetAuthenticatedUser(string ident)
        {
            if (authedUsers.ContainsKey(ident))
                return authedUsers[ident];
            else
                return null;
        }

        public int CreateUser(string username, string password, int userLevel = 1)
        {
            User user = new User();
            user.Username = username;
            user.Password = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();
            user.UserLevel = (long)(userLevel > 10 ? 10 : userLevel);

            if (GetUser(username, password) == null)
            {
                db.Users.AddObject(user);
                db.SaveChanges();
                return (int)user.Id;
            }
            else
            {
                return -1;
            }
        }

        public User GetUser(int id)
        {
            return db.Users.Where(x => x.Id == id).FirstOrDefault();
        }

        public User GetUser(string username)
        {
            return db.Users.Where(x => x.Username == username).FirstOrDefault();
        }

        public User GetUser(string username, string password)
        {
            string hash = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();
            return (from user 
                    in db.Users 
                    where user.Username == username && user.Password == hash 
                    select user
                    ).FirstOrDefault();
        }

        public IList<User> GetAuthenticatedUsers()
        {
            return authedUsers.Values.ToList();
        }

        public System.Data.Objects.ObjectSet<User> GetUsers()
        {
            return db.Users;
        }
    }
}
