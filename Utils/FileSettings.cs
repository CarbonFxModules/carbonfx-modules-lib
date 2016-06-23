using cAlgo.API;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{
    public class FileSettings : IDisposable
    {
        public static FileSettings GetSettings(string filePath, string key)
        {
            if (!Path.GetExtension(filePath).Equals(".json", StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception(string.Format("Invalid json configuration file: {0}", filePath));
            }
            if (!File.Exists(filePath))
            {
                throw new Exception(string.Format("Can't find settings file: '{0}'", filePath));
            }

            return new FileSettings(filePath, key, string.Empty);
        }

        public static FileSettings GetSettings(string robot, string symbol, string version)
        {
            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "cAlgo", "Settings", robot + ".json");
            if (File.Exists(fileName))
            {
                Console.WriteLine("Opening configuration '{0}' ", fileName);
                return new FileSettings(fileName, symbol, version);
            }
            else
            {
                throw new Exception(string.Format("Settings file missing for '{0}', {1}", robot, fileName));
            }
        }

        private string _fileName;
        private string _key;
        private string _version;
        private Timer _t;

        private JObject _settings;

        private FileSettings(string fileName, string symbol, string version)
        {
            _fileName = fileName;
            _key = symbol;
            _version = version;
            UpdateSettings();

            //_t = new Timer(1000 * 15);
            //_t.Elapsed += t_Elapsed;
            //_t.AutoReset = true;
            //_t.Enabled = true;
            //_t.Start();
        }

        //private void t_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    UpdateSettings();
        //}

        private void UpdateSettings()
        {
            var json = File.ReadAllText(_fileName);
            var obj = JObject.Parse(json);

            var temp = (JObject)obj[_key];

            if (!string.IsNullOrEmpty(_version))
            {
                if (temp[_version] == null)
                {
                    throw new Exception(string.Format("Invalid version: {0} {1}", _key, _version));
                }
            }
            if (temp == null)
            {
                throw new Exception(string.Format("Invalid config: {0} {1}", _key, _version));
            }
            _settings = temp;
        }

        public T Get<T>(string key)
        {
            try
            {
                if (!string.IsNullOrEmpty(_version) && _settings[_version][key] != null)
                {
                    return _settings[_version].Value<T>(key);
                }
                else
                {
                    return _settings.Value<T>(key);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Invalid Key: {0} {1} {2}", _version, key, ex.Message));
            }
        }

        public JToken GetObject(string key)
        {
            throw new NotImplementedException("JToken GetObject");
            //return JToken.FromObject(new object());
        }

        public void Dispose()
        {
            _t.Stop();
            _t = null;
        }
    }
}
