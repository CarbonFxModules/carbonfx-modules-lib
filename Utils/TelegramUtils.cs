using cAlgo.API;
using CarbonFxModules.Lib;
using CarbonFxModules.Lib.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{
    public static class TelegramUtils
    {
        public static void GetPips(this ITelegram ctx, string msg, DoubleResponse callback, TimeSpan expiration)
        {
            var commands = new List<string>() { "5", "10", "15", "20", "30", "40", "50" };

            ctx.GetChoice(msg,
                commands, (string s) =>
                {
                    double outVal = 0;
                    if (double.TryParse(s, out outVal))
                    {
                        callback(outVal);
                    }
                    else
                    {
                        callback(double.NaN);
                    }
                }, expiration);
        }

        public static void SetTakeProfit(this ITelegram ctx, Position position, IStrategyBase strategy)
        {
            strategy.Telegram.GetPips("Select TP (pips)", (double pips) =>
            {
                if (double.IsNaN(pips))
                {
                    strategy.Robot.Print("setTP output no good");
                    return;
                }
                double tp = 0;
                if (position.TradeType == TradeType.Buy)
                {
                    tp = position.EntryPrice + (pips * strategy.Symbol.TickSize);
                }
                else
                {
                    tp = position.EntryPrice - (pips * strategy.Symbol.TickSize);
                }
                var result = strategy.Robot.ModifyPosition(position, position.StopLoss, tp);
                if (result.IsSuccessful)
                {
                    ctx.SendMessage("TP Set {0}", tp);
                }
                else
                {
                    ctx.SendMessage("TP failed");
                }
            }, new TimeSpan(1, 0, 0));
        }

        public static void SetStopLoss(this ITelegram ctx, Position position, IStrategyBase strategy)
        {
            ctx.GetPips("Select SL (pips)", (double pips) =>
            {
                if (double.IsNaN(pips))
                {
                    strategy.Robot.Print("setSL output no good");
                    return;
                }
                double sl = 0;
                if (position.TradeType == TradeType.Buy)
                {
                    sl = position.EntryPrice - (pips * strategy.Symbol.TickSize);
                }
                else
                {
                    sl = position.EntryPrice + (pips * strategy.Symbol.TickSize);
                }
                var result = strategy.Robot.ModifyPosition(position, sl, position.TakeProfit);
                if (result.IsSuccessful)
                {
                    ctx.SendMessage("SL Set {0}", sl);
                }
                else
                {
                    ctx.SendMessage("SL failed");
                }
            }, new TimeSpan(1,0,0));
        }
    }
}
