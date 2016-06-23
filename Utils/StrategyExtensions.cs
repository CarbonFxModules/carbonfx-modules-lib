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
        private static List<string> _all_symbols = new List<string>();
        private static List<string> _major_symbols = new List<string>();

        public static IList<string> GetMajorSymbols(this IStrategyBase Strategy)
        {
            if (_major_symbols.Count == 0)
            {
                _major_symbols.AddRange(new string[]
                {
                        "EURUSD",
                        "GBPUSD",
                        "EURJPY",
                        "USDJPY",
                        "AUDUSD",
                        "USDCHF",
                        "GBPJPY",
                        "USDCAD",
                        "EURGBP",
                        "EURCHF",
                        "AUDJPY",
                        "NZDUSD"
                });
            }
            return _major_symbols;
        }

        public static IList<string> GetSymbols(this IStrategyBase Strategy)
        {
            if (_all_symbols.Count == 0)
            {
                _all_symbols.AddRange(new string[] {
                        "EURUSD",
                        "GBPUSD",
                        "EURJPY",
                        "USDJPY",
                        "AUDUSD",
                        "USDCHF",
                        "GBPJPY",
                        "USDCAD",
                        "EURGBP",
                        "EURCHF",
                        "AUDJPY",
                        "NZDUSD",
                        "CHFJPY",
                        "EURAUD",
                        "CADJPY",
                        "GBPAUD",
                        "EURCAD",
                        "AUDCAD",
                        "GBPCAD",
                        "AUDNZD",
                        "NZDJPY",
                        "USDNOK",
                        "AUDCHF",
                        "USDMXN",
                        "GBPNZD",
                        "EURNZD",
                        "CADCHF",
                        "USDSGD",
                        "USDSEK",
                        "NZDCAD",
                        "EURSEK",
                        "GBPSGD",
                        "EURNOK",
                        "EURHUF",
                        "USDPLN",
                        "USDDKK",
                        "GBPNOK",
                        "NZDCHF",
                        "GBPCHF",
                        "USDTRY",
                        "EURTRY"
                });
            }
            return _all_symbols;
        }

        public static bool PlaceOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction,
            double price, double? tp, double? sl, long size, string comment, out PendingOrder po)
        {
            double marketPrice = Strategy.Symbol.Bid;
            if (price > marketPrice && direction == TradeType.Sell)
            {
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            else if (price > marketPrice && direction == TradeType.Buy)
            {
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            else if (price < marketPrice && direction == TradeType.Sell)
            {
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            else if (price < marketPrice && direction == TradeType.Buy)
            {
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            po = null;
            return false;
        }
        public static bool PlaceOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction,
            double price, double pipSize, double? tp, double? sl, double? offset, long size, string comment = "")
        {
            PendingOrder po;
            // Market order
            if (offset.HasValue == false)
            {
                return MarketOrder(Strategy, sb, direction, price, size, sl, tp, comment);
            }
            // Limit Sell
            else if (offset > 0 && direction == TradeType.Sell)
            {
                price += offset.Value * pipSize;
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            // Stop Limit Sell
            else if (offset < 0 && direction == TradeType.Sell)
            {
                price += offset.Value * pipSize;
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            // Limit Buy
            else if (offset > 0 && direction == TradeType.Buy)
            {
                price -= offset.Value * pipSize;
                return CreateLimitOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            // Stop Limit Buy
            else if (offset < 0 && direction == TradeType.Buy)
            {
                price -= offset.Value * pipSize;
                return CreateStopOrder(Strategy, sb, direction, price, size, sl, tp, comment, out po);
            }
            return false;
        }

        public static bool MarketOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction,
            double price, long size, double? sl, double? tp, string comment = "")
        {
            TradeResult result = Strategy.Robot.ExecuteMarketOrder(direction.Value, Strategy.Symbol,
                size, Strategy.GetLabel(), sl, tp, Strategy.Symbol.PipSize * 2, comment);
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

        public static bool CreateLimitOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction,
            double price, long size, double? sl, double? tp, string comment, out PendingOrder po)
        {
            TradeResult result = Strategy.Robot.PlaceLimitOrder(direction.Value, Strategy.Symbol,
                size, price, Strategy.GetLabel(), sl, tp, null, comment);
            if (result.IsSuccessful)
            {
                po = result.PendingOrder;
                sb.AppendLine("Created Limit: {0} {1} {2} {3}", direction.Value, Strategy.Symbol.Code, size, price);
            }
            else
            {
                po = null;
                sb.AppendLine("Limit order failed {0}", result.Error);
            }
            return result.IsSuccessful;
        }

        public static bool CreateStopOrder(this IStrategyBase Strategy, StringBuilder sb, TradeType? direction,
            double price, long size, double? sl, double? tp, string comment, out PendingOrder po)
        {
            TradeResult result = Strategy.Robot.PlaceStopOrder(direction.Value, Strategy.Symbol,
                size, price, Strategy.GetLabel(), sl, tp, null, comment);
            if (result.IsSuccessful)
            {
                po = result.PendingOrder;
                sb.AppendLine("Created Stop: {0} {1} {2} {3}", direction.Value, Strategy.Symbol.Code, size, price);
            }
            else
            {
                po = null;
                sb.AppendLine("Stop Limit order failed {0}", result.Error);
            }
            return result.IsSuccessful;
        }
    }
}
