using cAlgo.API;
using CarbonFxModules.Modules;
using CarbonFxModules.Lib;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib.Telegram
{

    public class StrategyTelegram : TelegramClient
    {
        const string TelegramChannel = "TelegramChannel";
        const string TelegramBotAPI = "TelegramBotAPI";

        StrategyBase _strategy;
        public StrategyTelegram(StrategyBase bot) : base(
                    bot.Settings.Get<string>(TelegramChannel),
                    bot.Settings.Get<string>(TelegramBotAPI),
                    bot.Name, true
            )
        {
            _strategy = bot;
            this.RespondToCommand("status", telegramGetAccountInfo);
            this.RespondToCommand("positions", telegramGetPositions);
            this.RespondToCommand("orders", telegramGetOrderInfo);
            this.RespondToCommand("set", telegramSetSetting);
            this.RespondToCommand("settings", telelgramGetSettings);
            this.RespondToCommand("shutdown", telegramShutdownBot);
            this.RespondToCommand("modulestate", telegramGetMarketState);
            this.RespondToCommand("modules", telegramGetModules);
            this.RespondToCommand("report", telegramGetTradingReport);
            this.RespondToCommand("exposure", telegramGetExposure);
            this.RespondToCommand("commands", telegramListCommands);
            this.OnError += StrategyTelegram_OnError;
        }


        /// <summary>
        /// Lists all registered commands
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string telegramListCommands(string arg)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Commands {0}*\n", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            foreach (var c in this.GetCommands())
            {
                sb.AppendLine(string.Format("/{0}", c));
            }
            return sb.ToString();
        }

        private void StrategyTelegram_OnError(object sender, Exception e)
        {   
            _strategy.Robot.Print("Telegram Error: {0} {1}", e.TargetSite.Name, e.Message);
            foreach(var key in e.Data.Keys)
            {
                _strategy.Robot.Print("{0} : {1}", key, e.Data[key]);
            }
        }

        /// <summary>
        /// Gets Positions
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string telegramGetPositions(string arg)
        {
            StringBuilder sb = new StringBuilder();
            var command = new TokenizedCommands(arg);
            sb.AppendFormat("*Positions {0}*\n", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);

            IEnumerable<Position> positions;
            if (command.StartsWith("all"))
            {
                positions = _strategy.Robot.Positions.OrderBy(p => p.SymbolCode);
            }
            else
            {
                positions = _strategy.GetPositions().OrderBy(p => p.SymbolCode);
            }

            if (positions.Count() == 0)
            {
                sb.Append("No positions");
            }
            else
            {
                foreach (var p in positions)
                {
                    sb.Append("\n");
                    sb.AppendFormat("-{0} ", p.SymbolCode);
                    sb.AppendFormat("{0} ", p.TradeType);
                    sb.AppendFormat("{0:0.00} ", _strategy.Symbol.VolumeToQuantity(p.Volume));
                    sb.AppendFormat("Pips: {0:0.0}\n", p.Pips);
                    sb.AppendFormat("--Net: {0:c} ", p.NetProfit);
                    if (p.Swap > 0)
                    {
                        sb.AppendFormat("Swp: {0:c} ", p.Swap);
                    }
                    sb.AppendFormat("{0:#.#}hrs", _strategy.Robot.Server.Time.Subtract(p.EntryTime).TotalHours);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Returns a report of recent trades
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string telegramGetTradingReport(string command)
        {
            var cmdArgs = command.Split(' ');
            TimeSpan span = cmdArgs.Length == 0
                ? new TimeSpan(240, 0, 0)
                : Utilities.ParseTimeSpan(cmdArgs[0]);


            // Only get latest records
            var records = _strategy.Robot.History.FindAll(_strategy.GetLabel()).Where(h => h.ClosingTime > DateTime.Now.Subtract(span));

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Report {0}*", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);

            double pipsSum = 0.0;
            double netSum = 0.0;
            int cnt = 0;
            foreach (var r in records)
            {
                sb.AppendLine();
                sb.AppendFormat("{0} {1} {2} {3} {4:c} | {5}", r.SymbolCode, r.TradeType, r.Quantity, r.Pips, r.NetProfit, r.ClosingTime.ToUniversalTime().ToString("MMM d, hh:mm"));
                pipsSum += r.Pips;
                netSum += r.NetProfit;
                cnt++;
            }
            sb.AppendLine();
            sb.AppendFormat("{2} Trades, Pips: {0}, Net: {1:c}", pipsSum, netSum, records.Count());
            return sb.ToString();
        }


        /// <summary>
        /// Gets the name of all the modules
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string telegramGetModules(string command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Modules {0}*", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            foreach (var mod in _strategy.Modules)
            {
                sb.Append("\n-");
                sb.Append(mod.Name);
            }
            foreach (var pipeline in _strategy.OrderPipelines)
            {
                sb.Append("\n");
                sb.Append("*OrderPipeline: ");
                sb.Append(pipeline.Name);
                sb.Append("*");
                foreach (var mod in pipeline.Modules)
                {
                    sb.Append("\n-");
                    sb.Append(mod.Name);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Runs the modules and checks to see what state they think
        /// the market is in.  IMarketStatem, IMarketFilter, and all the order interfaces
        /// </summary>
        /// <param name="command"></param>
        /// <returns>ModuleName:ModuleReturnValue</returns>
        private string telegramGetMarketState(string command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Module State: {0}*", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            foreach (var mod in _strategy.Modules)
            {
                GetModuleOutput(sb, mod);
            }

            foreach (var pipeline in _strategy.OrderPipelines)
            {
                sb.Append("\n");
                sb.Append("*OrderPipeline: ");
                sb.Append(pipeline.Name);
                sb.Append("*");
                foreach (var module in pipeline.Modules)
                {
                    GetModuleOutput(sb, module);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Runs each module and sends the output to telegram
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="mod"></param>
        private void GetModuleOutput(StringBuilder sb, ModuleBase mod)
        {
            string moduleName = mod.Name;

            sb.Append("\n*");
            sb.Append(moduleName);
            sb.Append("*");

            if (mod is IReportStatus)
            {
                sb.Append("\n-IReportStatus ");
                sb.Append("\n");
                sb.Append(((IReportStatus)mod).GetStatus());
            }
            if (mod is IMarketEntryLogic)
            {
                sb.Append("\n-IMarketEntryLogic ");
                var result = ((IMarketEntryLogic)mod).OpenOrder();
                if (result.HasValue) sb.Append(result.Value);
                else sb.Append("null");
            }
            if (mod is IMarketFilter)
            {
                sb.Append("\n-IMarketFilter ");
                sb.Append(((IMarketFilter)mod).IsValidMarketCondition());
            }
            if (mod is IMarketState)
            {
                sb.Append("\n-IMarketState ");
                sb.Append(((IMarketState)mod).GetMarketState());
            }
            if (mod is IOrderFilter)
            {
                sb.Append("\n-IOrderFilter ");
                sb.Append("Buy");
                sb.Append(": ");
                sb.Append(((IOrderFilter)mod).ConfirmOrder(TradeType.Buy));
                sb.Append("\n-IOrderFilter ");
                sb.Append("Sell");
                sb.Append(": ");
                sb.Append(((IOrderFilter)mod).ConfirmOrder(TradeType.Sell));
            }
            if (mod is ITakeProfit)
            {
                sb.Append("\n-ITakeProfit ");
                sb.Append("Buy:");
                sb.Append(((ITakeProfit)mod).GetTakeProfit(TradeType.Buy));
                sb.Append("\n-ITakeProfit ");
                sb.Append("Sell:");
                sb.Append(((ITakeProfit)mod).GetTakeProfit(TradeType.Sell));
            }
            if (mod is IStopLoss)
            {
                sb.Append("\n-IStopLoss ");
                sb.Append("Buy:");
                sb.Append(((IStopLoss)mod).GetStopLoss(TradeType.Buy));
                sb.Append("\n-IStopLoss ");
                sb.Append("Sell:");
                sb.Append(((IStopLoss)mod).GetStopLoss(TradeType.Sell));
            }
            if (mod is ILotSize)
            {
                sb.Append("\n-ILotSize ");
                sb.Append("Buy:");
                sb.Append(((ILotSize)mod).GetLotSize(TradeType.Buy));
                sb.Append(", Sell:");
                sb.Append(((ILotSize)mod).GetLotSize(TradeType.Buy));
            }
        }


        /// <summary>
        /// Shutdown
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private string telegramShutdownBot(string command)
        {
            _strategy.Robot.Print("Shutdown command received");
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                _strategy.Robot.Stop();
            });
            return "Stopping";
        }


        /// <summary>
        /// Gets Settings
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        private string telelgramGetSettings(string setting)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Settings {0}*", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            var settings = _strategy.Settings;
            // TODO:  Get all keys from modules IValidateSettings
            foreach (var k in settings.GetAllKeys())
            {
                sb.Append("\n");
                sb.Append(k);
                sb.Append("  ");
                // Censor Telegram Settings
                if (k == TelegramChannel || k == TelegramBotAPI)
                {
                    var val = settings.Get<string>(k);
                    if (val.Length > 4)
                    {
                        sb.Append(val.Substring(0, 2) + "--" + val.Substring(val.Length - 3, 2));
                    }
                    else {
                        sb.Append("~");
                    }
                    sb.Append(" String");
                }
                else
                {
                    object settingValue = settings.Get<object>(k);
                    sb.Append(settingValue);
                    sb.Append(" ");
                    sb.Append(settingValue.GetType().Name);
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Sets settings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string telegramSetSetting(string arg)
        {
            var command = new TokenizedCommands(arg);
            KeyValuePair<string, string> kv = command.GetKeyVal();


            var settings = _strategy.Settings;

            object outVal;

            string validKey = settings.GetKeyName(kv.Key);
            if (validKey != null) // Key already exists, override it
            {
                Type oldType = settings.GetSettingType(validKey);
                if (kv.Value == "null")
                {
                    settings.Remove(kv.Key);
                    return string.Format("Setting removed: {0}", kv.Key);
                }
                else if (TryParseSetting(kv.Value, oldType, out outVal))
                {
                    settings.Set(validKey, outVal);
                    return string.Format("Set! {0} {1} {2}", validKey, outVal, oldType);
                }
                else
                {
                    return string.Format("Uncompatible type: {0} {1} shoud be :{2}", kv.Key, kv.Value, oldType.Name);
                }
            }
            else // new key of unknown type
            {
                if (TryParseSetting(kv.Value, out outVal))
                {
                    settings.Set(kv.Key, outVal);
                    return string.Format("Adding new key {0} {1} as {2}", kv.Key, outVal, outVal.GetType().Name);
                }
                else
                {
                    return string.Format("Unable to recognize value type: {0} {1} ", kv.Key, kv.Value);
                }
            }

        }

        private bool TryParseSetting(string settingVal, Type oldType, out object obj)
        {
            int iout;
            if (oldType.Name == "Int32" && int.TryParse(settingVal, out iout))
            {
                obj = iout;
                return true;
            }

            double dout;
            if (oldType.Name == "Double" && double.TryParse(settingVal, out dout))
            {
                obj = dout;
                return true;
            }
            obj = settingVal;

            TimeFrame tfout;
            if (oldType.Name == "TimeFrame" && Extensions.TryParseTimeFrame(settingVal, out tfout))
            {
                obj = tfout;
                return true;
            }
            return false;
        }

        private bool TryParseSetting(string settingVal, out object obj)
        {
            int iout;
            if (int.TryParse(settingVal, out iout))
            {
                obj = iout;
                return true;
            }

            double dout;
            if (double.TryParse(settingVal, out dout))
            {
                obj = dout;
                return true;
            }
            obj = settingVal;

            TimeFrame tfout;
            if (Extensions.TryParseTimeFrame(settingVal, out tfout))
            {
                obj = tfout;
                return true;
            }
            return false;
        }


        /// <summary>
        /// Gets account info
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private string telegramGetAccountInfo(string f)
        {
            return _strategy.GetAccountInfo();
        }


        /// <summary>
        /// Gets Order Info
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private string telegramGetOrderInfo(string f)
        {
            IEnumerable<PendingOrder> pos;
            if (f == "all")
            {
                pos = _strategy.GetPendingOrders();
            }
            else
            {
                pos = _strategy.Robot.PendingOrders;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Orders {0}*\n", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            if (pos.Count() == 0)
            {
                sb.Append("No orders");
            }
            else
            {
                var buys = pos.Where(p => p.TradeType == TradeType.Buy).GroupBy(p => p.SymbolCode, p => p);
                var sells = pos.Where(p => p.TradeType == TradeType.Sell).GroupBy(p => p.SymbolCode, p => p);
                if (buys.Count() > 0)
                {
                    foreach (var group in buys)
                    {
                        sb.AppendFormat("{0} Buy: {1} Lot: {2}\n",
                        group.Key,
                        group.Count(),
                        _strategy.Symbol.VolumeToQuantity(group.Sum(p => p.Volume)));
                    }
                }
                if (sells.Count() > 0)
                {
                    foreach (var group in sells)
                    {
                        sb.AppendFormat("{0} Short: {1} Lot: {2}\n",
                        group.Key,
                        group.Count(),
                        _strategy.Symbol.VolumeToQuantity(group.Sum(p => p.Volume)));
                    }
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// Gets current exposure
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private string telegramGetExposure(string f)
        {
            IEnumerable<Position> pos;
            if (f == "all")
            {
                pos = _strategy.Robot.Positions;
            }
            else
            {
                pos = _strategy.GetPositions();
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*Exposure {0}*", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            if (pos.Count() == 0)
            {
                sb.AppendLine("No positions");
            }
            else
            {
                var groupBySymbol = pos.GroupBy(p => p.SymbolCode, p => p);

                //var sells = pos.Where(p => p.TradeType == TradeType.Sell).GroupBy(p => p.SymbolCode, p => p);
                foreach (var group in groupBySymbol)
                {
                    var buys = group.Where(p => p.TradeType == TradeType.Buy);
                    var sells = group.Where(p => p.TradeType == TradeType.Sell);
                    var sym = _strategy.Robot.MarketData.GetSymbol(group.Key);
                    var pipSize = sym.PipSize;
                    var avgPrice = sym.AveragePrice();

                    //var symbol = _strategy.Robot.MarketData.GetSymbol(group.Key);

                    var priceLevel = group.AsEnumerable().WeightedPriceLevel();

                    sb.AppendFormat("\n{0} Buy: {1} Lot: {2}\n",
                        group.Key,
                        buys.Count(),
                        _strategy.Symbol.VolumeToQuantity(buys.Sum(p => p.Volume)));

                    sb.AppendFormat("{0} Sell: {1} Lot: {2}\n",
                        group.Key,
                        sells.Count(),
                        _strategy.Symbol.VolumeToQuantity(sells.Sum(p => p.Volume)));

                    sb.AppendFormat("*{0}: {2} Positions,  Net: {1:0.00}, BE: {3:0.0000} {6:0.0} pips\nDist: {4}, Swap: {5}, *",
                        group.Key,
                        _strategy.Symbol.VolumeToQuantity(group.Sum(p => p.TradeType == TradeType.Buy ? p.Volume : p.Volume * -1)),
                        group.Count(),
                        priceLevel, "-",
                        group.Sum(g => g.Swap),
                        (priceLevel - avgPrice) / pipSize
                        //NAN Math.Round((priceLevel - symbol.Ask ) / symbol.PipSize, 1)
                        );
                }
            }
            return sb.ToString();
        }
    }
}
