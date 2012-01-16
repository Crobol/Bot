using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Bot.Core
{
    public class UserService
    {
        private BotDataContext db = null;
        private IDictionary<string, User> authedUsers = new Dictionary<string, User>();
        SHA512CryptoServiceProvider sha512hasher = new SHA512CryptoServiceProvider();

        public UserService(BotDataContext db)
        {
            this.db = db;
        }

        public bool AuthenticateUser(string username, string password, string nick, string ident)
        {
            SHA512CryptoServiceProvider sha512hasher = new SHA512CryptoServiceProvider();
            string hash = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();

            var users = from x in db.User where x.Username == username && x.Password == hash select x;

            if (users.Any())
            {
                var user = users.First();
                authedUsers[ident] = user;

                user.LastNick = nick;
                user.LastIdent = ident;

                db.SubmitChanges();

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

        public IList<User> GetAuthenticatedUsers()
        {
            return authedUsers.Values.ToList();
        }

        public int CreateUser(string username, string password, int userLevel = 1)
        {
            User user = new User();
            user.Username = username;
            user.Password = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();
            user.UserLevel = (userLevel > 10 ? 10 : userLevel);

            if (GetUser(username, password) == null)
            {
                db.User.InsertOnSubmit(user);
                db.SubmitChanges();
                return (int)user.ID;
            }
            else
            {
                return -1;
            }
        }

        public User GetUser(int id)
        {
            return db.User.Where(x => x.ID == id).FirstOrDefault();
        }

        public User GetUser(string username)
        {
            return db.User.Where(x => x.Username == username).FirstOrDefault();
        }

        public User GetUser(string username, string password)
        {
            string hash = sha512hasher.ComputeHash(Encoding.Default.GetBytes(password)).ByteArrayToString();
            return (from user 
                    in db.User 
                    where user.Username == username && user.Password == hash 
                    select user
                    ).FirstOrDefault();
        }

        public DbLinq.Data.Linq.Table<User> GetUsers()
        {
            return db.User;
        }

        /// <summary>
        /// Get user setting
        /// </summary>
        /// <param name="userId">User who owns setting</param>
        /// <param name="name">Name of the setting</param>
        /// <returns></returns>
        public string GetUserSetting(int? userId, string name)
        {
            if (userId == null)
                userId = -1;

            return (from userSetting
                    in db.UserSetting
                    where userSetting.UserID == userId && userSetting.Name == name
                    select userSetting.Value).FirstOrDefault();
        }

        public void SetUserSetting(int? userId, string name, string value)
        {
            if (userId == null)
                userId = -1;

            UserSetting setting = (from userSetting
                                   in db.UserSetting
                                   where userSetting.UserID == userId && userSetting.Name == name
                                   select userSetting).FirstOrDefault();

            if (setting != null)
            {
                setting.Value = value;
            }
            else
            {
                setting = new UserSetting();
                setting.UserID = userId;
                setting.Name = name;
                setting.Value = value;
                db.UserSetting.InsertOnSubmit(setting);
            }

            db.SubmitChanges();
        }
    }
}
