using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Exceptions
{
    public class SettingNotFound : ArgumentException
    {
        public SettingNotFound(string key): base(key) { }
    }
}
