using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonFxModules.Lib;

namespace CarbonFxModules.Modules.MarketCondition
{
    public class DontTradeOnFridays : ModuleBase, IMarketFilter
    {
        public bool IsValidMarketCondition()
        {
            var serverTime = Strategy.Robot.Server.Time;
            if (serverTime.DayOfWeek == DayOfWeek.Friday
                && serverTime.Hour >= 20)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
