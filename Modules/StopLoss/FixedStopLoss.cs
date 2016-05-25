using cAlgo.API;
using CarbonFxModules.Bots;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.StopLoss
{
    public class FixedStopLoss : ModuleBase, IStopLoss
    {
        const string modulePrefix = "FixedSL_";
        const string @StopLoss = modulePrefix + "StopLoss";

        public double? GetStopLoss(TradeType direction)
        {
            return Settings.Get<double>(@StopLoss, 15);
        }
    }
}
