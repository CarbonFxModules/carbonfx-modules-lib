using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{

    /// <summary>
    /// OrderPipeLines should be able to have to their own settings, so this wrapper allows that
    /// without stomping the underlying dictionary value;
    /// </summary>
    public class OrderPipelineSettings : IStrategySettings
    {
        SettingsDictionary _baseDictionary;
        SettingsDictionary _overridesDictionary = new SettingsDictionary();

        /// <summary>
        /// Copies the values from the base dictionary and overrides them creating a new 
        /// </summary>
        /// <param name="baseDictionary"></param>
        /// <param name="overrides"></param>
        public OrderPipelineSettings(SettingsDictionary baseDictionary, IDictionary<string, object> overrides)
        {
            _baseDictionary = baseDictionary;
            foreach (var entry in overrides)
            {
                _overridesDictionary.Set(entry.Key, entry.Value);
            }
        }

        public bool Contains(string key)
        {
            return _overridesDictionary.Contains(key) || _baseDictionary.Contains(key);
        }

        public T Get<T>(string key)
        {
            object result = _overridesDictionary.Get<T>(key);
            if (result == null)
            {
                result = _baseDictionary.Get<T>(key);
            }
            return (T)result;
        }

        public T Get<T>(string key, T defaultVal)
        {
            object result = null;
            if (_overridesDictionary.Contains(key))
            {
                result = _overridesDictionary.Get<T>(key);
            }
            if (result == null)
            {
                result = _baseDictionary.Get<T>(key, defaultVal);
            }
            return (T)result;
        }

        public IEnumerable<string> GetAllKeys()
        {
            return _baseDictionary.GetAllKeys().Concat(_overridesDictionary.GetAllKeys()).Distinct();
        }

        public string GetKeyName(string key)
        {
            string outVal;
            outVal = _overridesDictionary.GetKeyName(key);
            if (outVal == null)
            {
                outVal = _baseDictionary.GetKeyName(key);
            }
            return outVal;
        }

        public T GetOrDefault<T>(string key, T defaultVal)
        {
            return this.Get<T>(key, defaultVal);
        }

        public Type GetSettingType(string key)
        {
            Type outVal;
            outVal = _overridesDictionary.GetSettingType(key);
            if (outVal == null)
            {
                outVal = _baseDictionary.GetSettingType(key);
            }
            return outVal;
        }

        public void Remove(string key)
        {
            if (_overridesDictionary.Contains(key))
            {
                _overridesDictionary.Remove(key);
                return;
            }

            if (_baseDictionary.Contains(key))
            {
                _baseDictionary.Remove(key);
                return;
            }
        }

        public void Set(string key, object o)
        {
            _overridesDictionary.Set(key, o);
        }
    }
}
