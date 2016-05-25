using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules
{
    public interface IMarketFilter
    {
        /// <summary>
        /// Return True is the market is ok to trade
        /// </summary>
        /// <returns></returns>
        bool IsValidMarketCondition();
    }
}
