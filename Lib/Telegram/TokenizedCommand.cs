using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib.Telegram
{
    public class TokenizedCommands
    {
        List<string> _commands = new List<string>();
        string _originalCmd;
        public TokenizedCommands(string command)
        {
            _originalCmd = command;
            foreach (var c in command.Split(' '))
            {
                if (!string.IsNullOrEmpty(c))
                {
                    _commands.Add(c);
                }
            }
        }

        public bool Has(string command)
        {
            return _commands.Contains(command);
        }

        public bool StartsWith(string command)
        {
            return _originalCmd.StartsWith(command);
        }

        public string Get(string key, string defaultVal)
        {
            if (Has(key))
            {
                var val = GetValue(key, false);
                if (string.IsNullOrEmpty(val))
                {
                    return defaultVal;
                }
                else
                {
                    return val;
                }
            }
            else
            {
                return defaultVal;
            }
        }

        /// <summary>
        /// Gets from this index to the end of the command
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public string GetToEnd(int idx)
        {
            return string.Join(" ", _commands.Slice(idx));
        }

        public string GetValue(string key, bool throwExp = true)
        {

            int idx = _commands.IndexOf(key);
            if (idx > -1 && idx + 1 < _commands.Count)
            {
                return _commands[idx + 1];
            }
            else
            {
                if (throwExp)
                {
                    throw new ArgumentNullException(key);
                }
                return string.Empty;
            }
        }

        public string GetByIdx(int v)
        {
            return _commands[v];
        }

        public KeyValuePair<string, string> GetKeyVal(int idx = 0)
        {
            int desired = (idx * 2) + 2;
            if (_commands.Count >= desired)
            {
                int start = idx * 2;
                return new KeyValuePair<string, string>(_commands[start], _commands[start + 1]);
            }
            else
            {
                throw new ArgumentException("Command does't have key val");
            }
        }

        public int Count()
        {
            return _commands.Count;
        }
    }
}
