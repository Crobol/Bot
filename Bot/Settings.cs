using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot
{
    class Settings
    {
        protected Dictionary<string, object> settings = new Dictionary<string, object>();

        public Settings()
        {

        }

        public Settings(string filepath)
        {

        }

        string GetString(string setting)
        {
            return "";
        }

        int GetInt(string setting)
        {
            return 0;
        }

        bool GetBool(string setting)
        {
            return false;
        }
    }
}
