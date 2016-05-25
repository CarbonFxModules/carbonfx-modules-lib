using System;
using System.Collections.Generic;

namespace CarbonFxModules.Utils
{
    public interface IStrategySettings
    {
        T Get<T>(string key);
        T Get<T>(string key, T defaultVal);
        T GetOrDefault<T>(string key, T defaultVal);
        void Set(string key, object o);
        bool Contains(string key);
        IEnumerable<string> GetAllKeys();
        string GetKeyName(string key);
        Type GetSettingType(string key);
        void Remove(string key);
    }
}