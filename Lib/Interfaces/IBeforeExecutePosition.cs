using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules
{
    /// <summary>
    /// Last place a module can stop a market order from being executed.  returns false
    /// </summary>
    public interface IBeforeMarketOrder
    {
        /// <summary>
        /// Inspect parameters before sending market order. if anything is wrong, throw an exception
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="symbol"></param>
        /// <param name="vol"></param>
        /// <param name="label"></param>
        /// <param name="stopLoss"></param>
        /// <param name="takeProfit"></param>
        bool OnBeforeMarketOrder(TradeType direction, Symbol symbol, long vol, string label, double? stopLoss, double? takeProfit);
    }
}
