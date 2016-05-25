using CarbonFxModules.Lib.Telegram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib.Telegram
{
    public interface ITelegram
    {
        void SendMessage(string message);
        void SendMessage(string message, params object[] args);
        void AskYesNo(string question, BoolResponse callback, TimeSpan expiration);
        void GetChoice(string question, List<string> options, StringResponse callback, TimeSpan expiration);
        void RespondToCommand(string command, Func<string, string> callback);
        void PassCommand(string command);
    }

    public class TelegramClient : ITelegram, IDisposable
    {
        string[] _chatId;
        string _appId;
        string _label;
        bool _pollForUpdates;

        public event EventHandler<Exception> OnError;

        public TelegramClient(string channelId, string apiId, string label, bool polling = false)
        {
            _chatId = channelId.Split(' ');
            _appId = apiId;
            _label = label;
            _pollForUpdates = polling;
            if (_pollForUpdates)
            {
                WatchForUpdates();
            }
        }

        // Track custom keyboard so we can match questions with a response.
        private Dictionary<string, CustomKeyboard> _keyboardResponses = new Dictionary<string, CustomKeyboard>();
        private CustomKeyboard GetKeyboard(Func<string, string> callback, TimeSpan expiration)
        {
            var kb = new CustomKeyboard(true, true, expiration);
            kb.Callback = callback;
            _keyboardResponses.Add(kb.ID, kb);
            return kb;
        }

        private Dictionary<string, Func<string, string>> _commandHandlers = new Dictionary<string, Func<string, string>>();
        public void RespondToCommand(string command, Func<string, string> callback)
        {
            _commandHandlers.Add(command, callback);
        }

        public IEnumerable<string> GetCommands()
        {
            return _commandHandlers.Keys;
        }

        /// <summary>
        /// Presents user with selection button for each item
        /// </summary>
        /// <param name="message">limited markdown.  https://core.telegram.org/bots/api/#markdown-style</param>
        /// <param name="items">Buttons</param>
        /// <param name="_callback">Handle the users response (if they respond)</param>
        /// <returns></returns>
        public void SendCustomKeyboardList(string message, IEnumerable<string> items, Func<string, string> _callback, TimeSpan expiration)
        {
            var kb = GetKeyboard(_callback, expiration);
            const int columnLength = 3;
            int cCount = 0;
            foreach (var item in items)
            {
                if (cCount == 0)
                {
                    kb.AddRow();
                }
                if (cCount == columnLength)
                {
                    cCount = 0;
                }
                cCount++;
                kb.AddKey(item);
            }
            SendCustomKeyboard(message, kb);
        }

        private void SendCustomKeyboard(string message, CustomKeyboard kb)
        {
            var values = new Dictionary<string, string>
            {
                { "reply_markup", kb.ToString() }
            };
            message = string.Format("{0} - {1}", kb.ID, message);
            _sendMessage(message, values);
        }

        /// <summary>
        /// Sends a message to the telegram channel
        /// </summary>
        /// <param name="message">limited markdown.  https://core.telegram.org/bots/api/#markdown-style</param>
        /// <param name="options"></param>
        /// <returns></returns>
        private void _sendMessage(string message, Dictionary<string, string> options = null)
        {
            foreach (var chat in _chatId)
            {
                var values = new Dictionary<string, string>
                {
                    { "chat_id", chat },
                    { "text", message },
                    { "parse_mode", "Markdown" }
                };
                if (options != null)
                {
                    foreach (var option in options)
                    {
                        if (values.ContainsKey(option.Key))
                        {
                            values[option.Key] = option.Value;
                        }
                        else {
                            values.Add(option.Key, option.Value);
                        }
                    }
                }
                Task.Run(() =>
                {
                    MakeTelegramRequest(_appId, "sendMessage", values);
                });
            }
        }

        protected int updateOffset = -1;
        public string GetBotUpdates()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("offset", (updateOffset).ToString());
            values.Add("timeout", (15).ToString()); // long poll                

            var jsonData = MakeTelegramRequest(_appId, "getUpdates", values);

            var matches = new Regex("\"update_id\"\\:(\\d+)").Matches(jsonData);
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    int msg_id = -1;
                    int.TryParse(m.Groups[1].Value, out msg_id);
                    if (msg_id >= updateOffset)
                    {
                        updateOffset = ++msg_id;
                    }
                }
            }
            return jsonData;
        }

        private Regex findMessages = new Regex("\"text\"\\:\"([^\"]+)");
        private void parseUpdates(string jsonData)
        {
            // Find out if they responded to a previous keyboard
            // {"ok":true,"result":[{"update_id":683320991,
            //   "message":{ 
            //         "message_id":121,
            //         "from":{ "id":104405457,"first_name":"---","username":"---"},
            //         "chat":{ "id":104405457,"first_name":"---","username":"---","type":"private"},
            //         "date":1460069382,
            //         "text":"\/status"}
            //        }]}

            var matches = findMessages.Matches(jsonData);
            if (matches.Count > 0)
            {
                foreach (Match m in matches)
                {
                    string text = m.Groups[1].Value;
                    // It's a command
                    HandleCommand(text);
                }
            }

            string foo = jsonData;
        }

        /// <summary>
        /// Parses a message passes it to the appropriate handler
        /// </summary>
        /// <param name="command"></param>
        private void HandleCommand(string command)
        {
            if (command.StartsWith("\\/"))
            {
                string[] cmdAgs = command.Remove(0, 2).Split(' ');
                string cmd = cmdAgs[0];
                string parameters = string.Join(" ", cmdAgs.Skip(1));
                if (_commandHandlers.ContainsKey(cmd))
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            string result = _commandHandlers[cmd](parameters).Replace('_', '-');
                            SendMessage(result);
                        }
                        catch (Exception ex)
                        {
                            SendMessage("Error: {0} {1}", ex.TargetSite.Name, ex.Message);
                        }
                    });
                }
                else
                {
                    Task.Run(() =>
                    {
                        SendMessage("unhandled command");
                    });
                }
            }
            else // Something else, lets see if it's a response to a keyboard question
            {
                List<string> _removeThese = new List<string>();
                foreach (var kb in _keyboardResponses)
                {
                    if (kb.Value.CheckResponse(command))
                    {
                        _removeThese.Add(kb.Key);
                    }
                }

                foreach (var key in _removeThese)
                {
                    _keyboardResponses.Remove(key);
                }
            }
        }

        private bool _watching = false;
        private void WatchForUpdates()
        {
            Task.Run(() =>
            {
                _watching = true;
                while (_watching)
                {
                    Thread.Sleep(1500);
                    try
                    {
                        var updates = GetBotUpdates();
                        if (_watching)
                        {
                            parseUpdates(updates);
                            removeOldKbQuestions();
                        }
                    }
                    catch (Exception ex)
                    {
                        SendMessage("Error in updates:" + ex.Message);
                    }
                }
            });
        }


        /// <summary>
        /// Removes old keyboards questions after expiration
        /// </summary>
        private void removeOldKbQuestions()
        {
            List<string> _removeThese = new List<string>();
            foreach (var kbQuestion in _keyboardResponses)
            {
                if (kbQuestion.Value.Expiration < DateTime.Now)
                {
                    _removeThese.Add(kbQuestion.Key);
                }
            }

            foreach (var key in _removeThese)
            {
                var kb = _keyboardResponses[key];
                _keyboardResponses.Remove(key);
                kb.Callback = null;
                kb = null;
            }
        }

        List<WebRequest> _openRequests = new List<WebRequest>();

        private string MakeTelegramRequest(string api_key, string method, Dictionary<string, string> values)
        {
            string TELEGRAM_CALL_URI = string.Format("https://api.telegram.org/bot{0}/{1}", api_key, method);
            var request = WebRequest.Create(TELEGRAM_CALL_URI);
            lock (_openRequests)
            {
                _openRequests.Add(request);
            }
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            StringBuilder data = new StringBuilder();
            int cnt = 0;
            foreach (var d in values)
            {
                if (cnt == 0)
                {
                    data.Append(string.Format("{0}={1}", d.Key, d.Value));
                }
                else
                {
                    data.Append(string.Format("&{0}={1}", d.Key, d.Value));
                }
                cnt++;
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(data.ToString());
            request.ContentLength = byteArray.Length;
            string outStr = "error:";
            try
            {
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    outStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                if (OnError != null)
                {
                    ex.Data.Add("method", method);
                    foreach (var d in values)
                    {
                        ex.Data.Add(d.Key, d.Value);
                    }
                    OnError(this, ex);
                }
                outStr += ex.Message;
            }
            finally
            {
                lock (_openRequests)
                {
                    if (_openRequests.Contains(request))
                    {
                        _openRequests.Remove(request);
                    }
                }
            }
            return outStr;
        }

        public void Dispose()
        {
            _watching = false;
            foreach (var kb in _keyboardResponses)
            {
                kb.Value.Callback = null;
            }
            _keyboardResponses.Clear();
            _commandHandlers.Clear();

            foreach (var req in _openRequests.ToArray())
            {
                req.Abort();
            }
            lock (_openRequests)
            {
                _openRequests.Clear();
            }
        }

        public void SendMessage(string message)
        {
            SendMessage(message, null);
        }

        public void AskYesNo(string question, BoolResponse callback, TimeSpan expiration)
        {
            SendCustomKeyboardList(question, new string[] { "yes", "no" }, (string result) =>
            {
                if (result == "yes")
                {
                    callback(true);
                }
                else if (result == "no")
                {
                    callback(false);
                }
                return "";
            }, expiration);
        }

        public void AskYesNo(string question, BoolResponse callback)
        {
            AskYesNo(question, callback, new TimeSpan(0, 60, 0));
        }

        public void GetChoice(string question, List<string> options, StringResponse callback, TimeSpan expiration)
        {
            SendCustomKeyboardList(question, options, (string result) =>
            {
                callback(result);
                return "";
            }, expiration);
        }

        public void SendMessage(string message, params object[] args)
        {
            if (args != null)
            {
                message = string.Format(message, args);
            }
            _sendMessage(message, null);
        }

        /// <summary>
        /// Allows one command to call other command
        /// </summary>
        /// <param name="command"></param>
        public void PassCommand(string command)
        {
            HandleCommand(command);
        }
    }
}
