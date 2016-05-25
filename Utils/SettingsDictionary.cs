using CarbonFxModules.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{
    public class SettingsDictionary : IStrategySettings
    {

        private Dictionary<string, object> _entries = new Dictionary<string, object>();
        
        public T Get<T>(string key)
        {
            if (!_entries.ContainsKey(key))
            {
                throw new SettingNotFound(key);
            }
            object o = null;
            if (_entries.TryGetValue(key, out o))
            {
                return (T)o;
            }
            return (T)o;
        }


        /// <summary>
        /// Gets all available setting names.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllKeys()
        {
            foreach (var d in _entries)
            {
                yield return d.Key;
            }
        }


        /// <summary>
        /// Gets a setting if it exists, or returns the given default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultVal)
        {
            return GetOrDefault(key, defaultVal);
        }

        /// <summary>
        /// Gets a setting if it exists, or returns the given default
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public T GetOrDefault<T>(string key, T defaultVal)
        {
            if (!_entries.ContainsKey(key))
            {
                return defaultVal;
            }
            object o = null;
            if (_entries.TryGetValue(key, out o))
            {
                return (T)o;
            }
            else
            {
                return defaultVal;
            }
        }

        public string GetKeyName(string key)
        {
            foreach (var k in this.GetAllKeys())
            {
                if (k.ToLower() == key.ToLower())
                {
                    return k;
                }
            }
            return null;
        }

        public Type GetSettingType(string key)
        {
            if (_entries.ContainsKey(key))
            {
                return _entries[key].GetType();
            }
            return null;
        }

        /// <summary>
        /// Creates/Overrites key with value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="o"></param>
        public void Set(string key, object o)
        {
            if (_entries.ContainsKey(key))
            {
                _entries[key] = o;
            }
            else
            {
                _entries.Add(key, o);
            }
        }

        /// <summary>
        /// Deletes a setting
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (_entries.ContainsKey(key))
            {
                _entries.Remove(key);
            }
        }

        public bool Contains(string key)
        {
            return _entries.ContainsKey(key);
        }
    }
}
