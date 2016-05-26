using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarbonFxModules.Bots;
using CarbonFxModules.Modules;
using CarbonFxModules.Lib;
using cAlgo.API;

namespace CarbonFxModules.Modules.TakeProfit
{
    public class FixedTakeProfit : ModuleBase, ITakeProfit
    {
        const string modulePrefix = "FixedTP_";
        const string TakeProfit = modulePrefix + "TakeProfit";

        public double? GetTakeProfit(TradeType direction)
        {
            return Settings.Get<double>(TakeProfit, 15);
        }
    }
}
