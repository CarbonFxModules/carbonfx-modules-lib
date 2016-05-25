using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CarbonFxModules.Lib.Telegram
{
    public class CustomKeyboard
    {
        private static List<string> _usedIds = new List<string>();
        /// <summary>
        /// Make keyboard big on clients
        /// </summary>
        public bool Resize { get; set; }

        /// <summary>
        /// Hide keyboard after click
        /// </summary>
        public bool OneTime { get; set; }

        private string _id;

        public string ID
        {
            get
            {
                return _id;
            }
        }

        public Func<string, string> Callback { get; internal set; }

        private DateTime _created;

        public DateTime Created
        {
            get
            {
                return _created;
            }
        }

        private DateTime _expiration;
        public DateTime Expiration
        {
            get
            {
                return _expiration;
            }
        }

        public CustomKeyboard(bool resize, bool oneTime, TimeSpan expiration)
        {
            _created = DateTime.Now;
            _expiration = _created.Add(expiration);
            Resize = resize;
            OneTime = oneTime;
            
            lock (CustomKeyboard._usedIds)
            {
                string id = string.Empty;
                do
                {
                    string base36Str = Base26.Encode(Math.Abs(DateTime.Now.ToBinary()));
                    id = (base36Str.Substring(0, 2) + base36Str.Substring(base36Str.Length - 3, 2)).ToLower();
                    Thread.Sleep(100);
                }
                while (CustomKeyboard._usedIds.Contains(id));
                _id = id;
            }

            //using (SHA256 hasher = SHA256.Create())
            //{
            //    byte[] data = hasher.ComputeHash(Encoding.ASCII.GetBytes(Math.Abs(DateTime.Now.ToBinary()).ToString()));
            //    string strData = Convert.ToBase64String(data);
            //    _id = (strData.Substring(0, 2) + strData.Substring(strData.Length - 8,2)).ToLower();
            //}
        }

        List<string> _rows = new List<string>();
        public CustomKeyboard AddRow()
        {
            _rows.Add("");
            return this;
        }

        public CustomKeyboard AddKey(string key)
        {
            var cnt = _rows.Count - 1;
            if (_rows[cnt].Length > 0)
            {
                _rows[cnt] += "," + key;
            }
            else
            {
                _rows[cnt] = key;
            }
            return this;
        }

        List<string> _registeredValues = new List<string>();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"keyboard\":[");
            for (int i = 0; i < _rows.Count; i++)
            {
                sb.Append("[");
                sb.Append(string.Join(",", _rows[i].Split(',').Select(s =>
                {
                    string key = string.Format("{0}_{1}", s, _id);
                    if (!_registeredValues.Contains(key)) _registeredValues.Add(key);
                    return string.Format("\"{0}\"", key);
                })));
                sb.Append("]");
                if (i < _rows.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.Append("],");
            sb.Append(string.Format("\"resize_keyboard\":{0},", Resize.ToString().ToLower()));
            sb.Append(string.Format("\"one_time_keyboard\":{0}", OneTime.ToString().ToLower()));
            sb.Append("}");
            return HttpUtility.UrlEncode(sb.ToString());
        }

        /// <summary>
        /// Checks to see if any keyboard commands are in this message,
        /// if so fire the callback
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool CheckResponse(string response)
        {
            if (!response.Contains(this.ID)) return false;

            foreach (var val in _registeredValues)
            {
                if (response.Contains(val))
                {
                    if (Callback != null)
                    {
                        // Remove the keyboard identifier and only pass message on
                        Task.Run(() =>
                        {
                            Callback(val.Replace("_" + this.ID, ""));
                        });
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
