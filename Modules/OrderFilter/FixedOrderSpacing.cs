using cAlgo.API;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.OrderFilter
{
    public class FixedOrderSpacing : ModuleBase, IOrderFilter, IValidateSettings, IReportStatus
    {
        const string modulePrefix = "FixedOS_";
        const string MinimumOrderSpacingPips = modulePrefix + "MinimumSpacingPips";
        string _lastStatus = string.Empty;
        
        public bool ConfirmOrder(TradeType direction)
        {
            double spacing = Settings.Get<double>(MinimumOrderSpacingPips, 15);
            foreach (var p in Strategy.GetPositions().Where(p=>p.TradeType == direction))
            {
                // make sure position is at least 'spacing' away from current price
                if (Math.Abs(p.Pips) < spacing)
                {
                    _lastStatus = string.Format("Too close to {0} position at: {1}. {2} pips", p.TradeType, p.EntryPrice, p.Pips);
                    return false;
                }
            }

            double currentPrice = direction == TradeType.Buy ? Strategy.Symbol.Ask : Strategy.Symbol.Bid;
            foreach (var o in Strategy.GetPendingOrders())
            {
                // Estimate by subtracting targetPrice from current ask                
                double diff = Math.Abs(currentPrice - o.TargetPrice);
                if (diff < spacing)
                {
                    _lastStatus = string.Format("Too close to {0} order at: {1}. {2} pips", o.TradeType, o.TargetPrice, diff);
                    return false;
                }
            }
            return true;
        }

        public string[] CheckRequiredSettings()
        {
            return new string[] { };
        }

        public string[] GetAvailableSettings()
        {
            return new string[] { MinimumOrderSpacingPips };
        }

        public string GetStatus()
        {
            return string.Format("{0} ... Setting:{1}", _lastStatus, Settings.Get<double>(MinimumOrderSpacingPips, 15));
        }
    }
}
