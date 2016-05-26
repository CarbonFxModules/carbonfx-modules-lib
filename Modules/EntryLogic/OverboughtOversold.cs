using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;
using CarbonFxModules.Utils;

namespace CarbonFxModules.Modules.EntryLogic
{
    public class OverboughtOversold : ModuleBase, IMarketEntryLogic, IModuleInit
    {

        MarketSeries _m5;
        public void InitModule()
        {
            _m5 = Strategy.Robot.MarketData.GetSeries(TimeFrame.Minute5);
        }

        public TradeType? OpenOrder()
        {
            var candles = new CandlePatterns(_m5);

            if (candles.IsHigherHigh())
            {
                return TradeType.Sell;
            }
            else if (candles.IsLowerLow())
            {
                return TradeType.Buy;
            }
            return null;
        }
    }
}
