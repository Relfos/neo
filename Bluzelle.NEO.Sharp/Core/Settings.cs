using System;
using System.Collections.Generic;
using System.IO;

namespace Bluzelle.NEO.Sharp.Core
{
    public class Settings
    {
        private Dictionary<string, string> entries = new Dictionary<string, string>();

        public Settings(string[] args)
        {
            foreach (var arg in args)
            {
                if (!arg.StartsWith("--"))
                {
                    continue;
                }

                var temp = arg.Substring(2).Split(new char[] { '=' }, 2);
                var key = temp[0];
                var val = temp.Length > 1 ? temp[1] : "";

                entries[key] = val;
            }
        }

        public string GetValue(string key, string defaultVal = null)
        {
            if (entries.ContainsKey(key))
            {
                return entries[key];
            }

            if (defaultVal != null)
            {
                return defaultVal;
            }

            Console.WriteLine("Missing argument: --" + key);
            Environment.Exit(-1);
            return null;
        }
    }
}

