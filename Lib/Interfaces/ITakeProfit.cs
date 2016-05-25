using cAlgo.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules
{
    public interface ITakeProfit
    {
        double? GetTakeProfit(TradeType direction);     
    }
}
