using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Bot.Core
{
    /// <summary>
    /// Used to map options to properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class OptionName : Attribute
    {
        private string name;
        private string longName;

        public string Name { get { return name; } }
        public string LongName { get { return longName; } }

        public OptionName(string name)
        {
            this.name = name;
            this.longName = null;
        }

        public OptionName(string name, string longName)
        {
            this.name = name;
            this.longName = longName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionDescription : Attribute
    {
        private string description;

        public string Description { get { return description; } }

        public OptionDescription(string description)
        {
            this.description = description;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionFullName : Attribute
    {
        private string fullName;

        public string FullName { get { return fullName; } }

        public OptionFullName(string fullName)
        {
            this.fullName = fullName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultValue : Attribute
    {
        public string _default;

        public string Default { get { return _default; } }

        public DefaultValue(string _default)
        {
            this._default = _default;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultOption : Attribute
    {
        public bool Default { get { return true; } }
    }

    /// <summary>
    /// Used to parse command options
    /// </summary>
    public class OptionParser
    {
        private static Regex defaultRegex = new Regex(@"^![A-Za-z0-9-]+( -\w{1} [A-Za-z0-9]*| --[A-Za-z-]+ [A-Za-z0-9]*)* ([\w\d\s]+)$");

        /// <summary>
        /// Parses a string and via reflection returns a filled instance of T
        /// </summary>
        /// <typeparam name="T">Type of the class that holds the options</typeparam>
        /// <param name="s">String to parse</param>
        /// <returns>Instance of T filled with matching values</returns>
        public static T Parse<T>(string s) where T : new()
        {
            T t = new T();
            
            var properties = t.GetType().GetProperties();
            foreach (var property in properties)
            {
                bool defaultOption = property.GetCustomAttributes(typeof(DefaultOption), false).Any(x => (x as DefaultOption).Default);

                if (defaultOption)
                {
                    var match = defaultRegex.Match(s);
                    string value = match.Groups[match.Groups.Count - 1].Value;

                    if (property.PropertyType == typeof(string))
                        property.SetValue(t, value, null);
                    else if (property.PropertyType == typeof(int))
                        property.SetValue(t, int.Parse(value), null);
                    else if (property.PropertyType == typeof(decimal))
                        property.SetValue(t, decimal.Parse(value), null);
                }
                else
                {
                    string shortName = "", longName = "";
                    foreach (var attribute in property.GetCustomAttributes(typeof(OptionName), false))
                    {
                        shortName = (attribute as OptionName).Name;
                        longName = (attribute as OptionName).LongName;
                    }

                    string defaultValue = null;
                    foreach (var attribute in property.GetCustomAttributes(typeof(DefaultValue), false))
                    {
                        defaultValue = (attribute as DefaultValue).Default;
                    }

                    Regex optionPattern = null;
                    if (property.PropertyType == typeof(bool))
                    {
                        optionPattern = new Regex("(-" + shortName + "|--" + longName + ")"); // TODO: What happens if longName is null or empty?
                        var match = optionPattern.Match(s);
                        if (match.Success)
                            property.SetValue(t, true, null);
                        else if (!string.IsNullOrEmpty(defaultValue))
                            property.SetValue(t, bool.Parse(defaultValue), null);
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        optionPattern = new Regex("(-" + shortName + "|--" + longName + ") ([A-Za-z0-9-]+)");
                        var match = optionPattern.Match(s);
                        if (match.Success && match.Groups.Count > 2 && match.Groups[2].Success)
                            property.SetValue(t, match.Groups[2].Value, null);
                        else if (!string.IsNullOrEmpty(defaultValue))
                            property.SetValue(t, defaultValue, null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        optionPattern = new Regex("(-" + shortName + "|--" + longName + ") (\\d+)");
                        var match = optionPattern.Match(s);
                        if (match.Success && match.Groups.Count > 2 && match.Groups[2].Success)
                            property.SetValue(t, int.Parse(match.Groups[2].Value), null);
                        else if (!string.IsNullOrEmpty(defaultValue))
                            property.SetValue(t, int.Parse(defaultValue), null);
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        optionPattern = new Regex("(-" + shortName + "|--" + longName + ") (\\d+)");
                        var match = optionPattern.Match(s);
                        if (match.Success && match.Groups.Count > 2 && match.Groups[2].Success)
                            property.SetValue(t, decimal.Parse(match.Groups[2].Value), null);
                        else if (!string.IsNullOrEmpty(defaultValue))
                            property.SetValue(t, decimal.Parse(defaultValue), null);
                    }
                }
            }

            return t;
        }

        public static T ParseByOrder<T>(string s, string delimiter = " ") where T : new()
        {
            T t = new T();

            string[] args = s.Split(new [] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
            var properties = t.GetType().GetProperties();

            if (args.Length - 1 == properties.Length)
            {
                int i = 1;

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(t, args[i], null);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(t, int.Parse(args[i]), null);
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        property.SetValue(t, decimal.Parse(args[i]), null);
                    }
                    i++;
                }
            }
            else if (args.Length == 2)
            {
                foreach (var property in properties)
                {
                    string value = "";
                    bool defaultOption = property.GetCustomAttributes(typeof(DefaultOption), false).Any(x => (x as DefaultOption).Default);

                    if (defaultOption)
                    {
                        value = args[1];
                    }
                    else
                    {
                        var attribute = property.GetCustomAttributes(typeof(DefaultValue), false).FirstOrDefault();
                        if (attribute != null)
                            value = (attribute as DefaultValue).Default;
                    }

                    if (property.PropertyType == typeof(string))
                        property.SetValue(t, value, null);
                    else if (property.PropertyType == typeof(int))
                        property.SetValue(t, int.Parse(value), null);
                    else if (property.PropertyType == typeof(decimal))
                        property.SetValue(t, decimal.Parse(value), null);
                }
            }

            return t;
        }

        /// <summary>
        /// Creates a signature string of the command type given
        /// </summary>
        /// <param name="t">Type of the command</param>
        /// <returns>Signature string</returns>
        public static string CreateCommandSignature(Type t)
        {
            string signature = "";
            string defaultOptionName = "";

            var properties = t.GetProperties();
            foreach (var property in properties)
            {
                bool defaultOption = false;
                foreach (var attribute in property.GetCustomAttributes(typeof(DefaultOption), false))
                {
                    defaultOption = (attribute as DefaultOption).Default;
                }

                string fullName = "";
                foreach (var attribute in property.GetCustomAttributes(typeof(OptionFullName), false))
                {
                    fullName = (attribute as OptionFullName).FullName;
                }

                if (defaultOption)
                {
                    if (!string.IsNullOrEmpty(fullName))
                        defaultOptionName = fullName;
                    else
                        defaultOptionName = "input";
                }
                else
                {
                    string shortName = "", longName = "";
                    foreach (var attribute in property.GetCustomAttributes(typeof(OptionName), false))
                    {
                        shortName = (attribute as OptionName).Name;
                        longName = (attribute as OptionName).LongName;
                    }

                    signature += "[-" + shortName + ", --" + longName;
                    if (!string.IsNullOrEmpty(fullName))
                        signature += " <" + fullName + ">";
                    signature += "] ";
                }
            }

            if (!string.IsNullOrEmpty(defaultOptionName))
                signature += "<" + defaultOptionName + ">";

            return signature;
        }

        /// <summary>
        /// Creates a help message based on the name and options of the type of the command given
        /// </summary>
        /// <param name="t">Type of the command</param>
        /// <returns>Help message string</returns>
        public static string CreateHelpMessage(Type t)
        {
            string helpMessage = "";

            var properties = t.GetProperties();
            foreach (var property in properties)
            {
                string shortName = "", longName = "";
                foreach (var attribute in property.GetCustomAttributes(typeof(OptionName), false))
                {
                    shortName = (attribute as OptionName).Name;
                    longName = (attribute as OptionName).LongName;
                }

                string fullName = "";
                foreach (var attribute in property.GetCustomAttributes(typeof(OptionFullName), false))
                {
                    fullName = (attribute as OptionFullName).FullName;
                }

                string description = "";
                foreach (var attribute in property.GetCustomAttributes(typeof(OptionDescription), false))
                {
                    description = (attribute as OptionDescription).Description;
                }

                helpMessage += "-" + shortName + ", --" + longName + ": " + description;
            }

            return helpMessage;
        }
    }
}
