using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core
{
    public static class StringExtensions
    {
        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static string ByteArrayToString(this byte[] bytes)
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                output.Append(bytes[i].ToString("x2"));
            }

            return output.ToString();
        }
    }
}
