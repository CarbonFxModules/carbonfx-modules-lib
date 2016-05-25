using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.MarketCondition
{
    /// <summary>
    /// Traders way has shitty liquidity between  20 - 23 hours everyday
    /// </summary>
    public class TradersWayBlackoutHours : ModuleBase, IMarketFilter
    {
        public bool IsValidMarketCondition()
        {
            var serverTime = Strategy.Robot.Server.Time;
            return !(serverTime.Hour >= 20 && serverTime.Hour <= 22);
        }
    }
}
