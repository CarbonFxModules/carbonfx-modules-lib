using cAlgo.API;
using CarbonFxModules.Lib;
using CarbonFxModules.Utils;
using System.Linq;
using System;

namespace CarbonFxModules.Modules.OrderProtection
{
    public class TrailingStops : ModuleBase, IOnBar, IModuleInit, IValidateSettings
    {
        const string modulePrefix = "Trailing_";
        const string @MoveTrailingEvery = modulePrefix + "MoveEvery";
        const string @TrailingAmount = modulePrefix + "Amount";

        double _moveTrailingEvery;
        double _trailingAmount;

        public string[] CheckRequiredSettings()
        {
            return new string[] { };
        }

        public string[] GetAvailableSettings()
        {
            return new string[] { @MoveTrailingEvery, @TrailingAmount };
        }

        public void InitModule()
        {
            _moveTrailingEvery = Settings.Get<double>(@MoveTrailingEvery, 10.0);
            _trailingAmount = Settings.Get<double>(@TrailingAmount, 10.0);
        }

        void IOnBar.OnBar()
        {
            var allPositions = Strategy.GetPositions();
            double netShort = allPositions.Where(p => p.TradeType == TradeType.Sell).Sum(p => p.Pips * p.Volume);
            double netLong = allPositions.Where(p => p.TradeType == TradeType.Buy).Sum(p => p.Pips * p.Volume);

            double pipSize = Strategy.Symbol.PipSize, bid = Strategy.Symbol.Bid, ask = Strategy.Symbol.Ask;

            foreach (var position in allPositions)
            {
                if (position.TradeType == TradeType.Buy)
                {
                    double newStop = double.NaN;
                    if (position.StopLoss == null && position.Pips > _moveTrailingEvery)
                    {
                        newStop = position.EntryPrice + pipSize * 2;
                    }
                    else if (position.StopLoss != null && position.TakeProfit == null)
                    {
                        double? gain = (bid - position.StopLoss) / pipSize;
                        if (gain > _moveTrailingEvery)
                        {
                            newStop = bid - (_trailingAmount * pipSize);
                        }
                    }
                    if (!double.IsNaN(newStop) && (!position.StopLoss.HasValue || !Utilities.FuzzyCompare(position.StopLoss.Value, newStop, pipSize)))
                    {
                        Strategy.Robot.ModifyPosition(position, newStop, position.TakeProfit);
                    }
                }
                else if (position.TradeType == TradeType.Sell)
                {
                    double newStop = double.NaN;
                    if (position.StopLoss == null && position.Pips > _moveTrailingEvery)
                    {
                        newStop = position.EntryPrice - pipSize * 2;
                    }
                    else if (position.StopLoss != null && position.TakeProfit == null)
                    {
                        double? gain = (position.StopLoss - ask) / pipSize;
                        if (gain > _moveTrailingEvery)
                        {
                            newStop = bid + (_trailingAmount * pipSize);
                        }
                    }
                    if (!double.IsNaN(newStop) && (!position.StopLoss.HasValue || !Utilities.FuzzyCompare(position.StopLoss.Value, newStop, pipSize)))
                    {
                        Strategy.Robot.ModifyPosition(position, newStop, position.TakeProfit);
                    }
                }
            }
        }
    }
}
