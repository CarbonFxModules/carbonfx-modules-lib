using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib.Telegram
{
    public class NoopTelegram : ITelegram
    {
        public void AskYesNo(string question, BoolResponse callback, TimeSpan expiration)
        {
            
        }

        public void GetChoice(string question, List<string> options, StringResponse callback, TimeSpan expiration)
        {
            
        }

        public void PassCommand(string command)
        {
            
        }

        public void RespondToCommand(string command, Func<string, string> callback)
        {
            
        }

        public void SendMessage(string message)
        {
            
        }

        public void SendMessage(string message, params object[] args)
        {
            
        }
    }
}
