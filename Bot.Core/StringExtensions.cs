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

        public static string ByteArrayToString(this byte[] value)
        {
            var output = new StringBuilder();

            for (int i = 0; i < value.Length; i++)
            {
                output.Append(value[i].ToString("x2"));
            }

            return output.ToString();
        }

        public static string FormatToIrc(this string s)
        {
            if (s.Length > 400)
            {
                int lastIndex = s.Substring(0, 400).LastIndexOf(' ');
                return s.Substring(0, lastIndex) + " [...]";
            }
            else
                return s;
        }

        public static string[] SplitArguments(string commandLine)
        {
            var parmChars = commandLine.ToCharArray();
            var inSingleQuote = false;
            var inDoubleQuote = false;
            for (var index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"' && !inSingleQuote)
                {
                    inDoubleQuote = !inDoubleQuote;
                    parmChars[index] = '\n';
                }
                if (parmChars[index] == '\'' && !inDoubleQuote)
                {
                    inSingleQuote = !inSingleQuote;
                    parmChars[index] = '\n';
                }
                if (!inSingleQuote && !inDoubleQuote && parmChars[index] == ' ')
                    parmChars[index] = '\n';
            }
            return (new string(parmChars)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
