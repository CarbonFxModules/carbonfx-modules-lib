using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using CarbonFxModules.Lib;

namespace CarbonFxModules.Utils
{
    public static class StrategyExtensions
    {
        public static bool PlaceOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction, double price, double pipSize, double? tp, double? sl, double? offset, long size, string comment = "")
        {
            // Market order
            if (offset.HasValue == false)
            {
                return MarketOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            // Limit Sell
            else if (offset > 0 && direction == TradeType.Sell)
            {
                price += offset.Value * pipSize;
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            // Stop Limit Sell
            else if (offset < 0 && direction == TradeType.Sell)
            {
                price += offset.Value * pipSize;
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            // Limit Buy
            else if (offset > 0 && direction == TradeType.Buy)
            {
                price -= offset.Value * pipSize;
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            // Stop Limit Buy
            else if (offset < 0 && direction == TradeType.Buy)
            {
                price -= offset.Value * pipSize;
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            return false;
        }

        public static bool MarketOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction, double price, long size, double? sl, double? tp, string comment = "")
        {
            TradeResult result = Strategy.Robot.ExecuteMarketOrder(direction.Value, Strategy.Symbol, size, Strategy.GetLabel(), sl, tp, Strategy.Symbol.PipSize * 2, comment);
            if (result.IsSuccessful)
            {
                sb.AppendLine("Created: {0} {1} {2} {3}", direction.Value, Strategy.Symbol.Code, size, price);                
            }
            else
            {
                sb.AppendLine("Market order failed {0}", result.Error);
            }
            return result.IsSuccessful;
        }

        public static bool CreateLimitOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction, double price, long size, double? sl, double? tp, string comment = "")
        {
            TradeResult result = Strategy.Robot.PlaceLimitOrder(direction.Value, Strategy.Symbol, size, price, Strategy.GetLabel(), sl, tp, null, comment);
            if (result.IsSuccessful)
            {
                sb.AppendLine("Created Limit: {0} {1} {2} {3}", direction.Value, Strategy.Symbol.Code, size, price);
            }
            else
            {
                sb.AppendLine("Limit order failed {0}", result.Error);
            }
            return result.IsSuccessful;
        }

        public static bool CreateStopOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction, double price, long size, double? sl, double? tp, string comment = "")
        {
            TradeResult result = Strategy.Robot.PlaceStopOrder(direction.Value, Strategy.Symbol, size, price, Strategy.GetLabel(), sl, tp, null, comment);
            if (result.IsSuccessful)
            {
                sb.AppendLine("Created Stop: {0} {1} {2} {3}", direction.Value, Strategy.Symbol.Code, size, price);
            }
            else
            {
                sb.AppendLine("Stop Limit order failed {0}", result.Error);
            }
            return result.IsSuccessful;
        }
    }
}
