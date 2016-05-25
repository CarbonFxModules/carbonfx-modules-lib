using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Internals;

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
            

            return null;

        }
    }
}
