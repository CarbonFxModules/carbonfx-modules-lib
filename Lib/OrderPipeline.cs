using cAlgo.API;
using cAlgo.API.Internals;
using CarbonFxModules.Modules;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;

namespace CarbonFxModules.Lib
{
    public class OnTickOrderPipeline : OrderPipeline, IOnTick
    {
        public OnTickOrderPipeline(StrategyBase strategy, string name, IEnumerable<ModuleBase> modules, IDictionary<string, object> settings) : base(strategy, name, modules, settings) { }
        public void OnTick()
        {
            if (_enabled)
            {
                CheckForEntry();
            }
        }
    }

    public class OnBarOrderPipeline : OrderPipeline, IOnBar
    {
        public OnBarOrderPipeline(StrategyBase strategy, string name, IEnumerable<ModuleBase> modules, IDictionary<string, object> settings) : base(strategy, name, modules, settings) { }
        public void OnBar()
        {
            if (_enabled)
            {
                CheckForEntry();
            }
        }
    }

    public abstract class OrderPipeline : IDisposable
    {
        public IMarketEntryLogic MarketEntryModule { get; set; }
        public ITakeProfit TakeProfitModule { get; set; }
        public IStopLoss StopLossModule { get; set; }
        public ILotSize LotSizeModule { get; set; }
        List<IOrderFilter> _orderFilters = new List<IOrderFilter>();
        List<IMarketFilter> _marketFilters = new List<IMarketFilter>();
        List<ModuleBase> _allmodules = new List<ModuleBase>();
        List<IBeforeMarketOrder> _beforeMarketOrder = new List<IBeforeMarketOrder>();

        public IEnumerable<IMarketFilter> MarketFilters
        {
            get
            {
                return _marketFilters;
            }
        }

        public IEnumerable<ModuleBase> Modules
        {
            get
            {
                return _allmodules;
            }
        }

        protected bool _enabled = true;
        /// <summary>
        /// Is this pipeline enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
            }
        }

        public string Name { get; private set; }
        private StrategyBase _strategy;

        protected StrategyBase Strategy
        {
            get
            {
                return _strategy;
            }
        }

        public OrderPipeline(StrategyBase strategy, string name, IEnumerable<ModuleBase> modules, IDictionary<string, object> settingsOverrides)
        {
            _strategy = strategy;
            IStrategySettings _settings = _strategy.Settings;
            if (settingsOverrides != null)
            {
                _settings = new OrderPipelineSettings(strategy._settings, settingsOverrides);
            }
            Name = name;
            foreach (var module in modules)
            {
                module.Strategy = strategy;
                module.Settings = _settings;
                AddModule(module);
            }
        }

        /// <summary>
        /// Runs IMarketEntryLogic for OrderPipeline 
        /// </summary>
        protected void CheckForEntry()
        {
            if (Strategy.IsInitialized() == false) return;

            // sanity check. Make sure Entry Logic is defined for this pipeline
            if (MarketEntryModule == null)
            {
                Strategy.Robot.Print("Warning: No EntryLogic defined in OrderPipeline '" + this.Name + "'");
                return;
            }

            // Check market filters  time of day, volume, news, etc.
            if (!Strategy.CheckMarketFilters() || !this.CheckMarketFilters()) return;

            // on a micro level, does this entry look good.
            TradeType? marketOrderDirection = MarketEntryModule.OpenOrder();

            // no entry, we exit
            if (!marketOrderDirection.HasValue) return;

            TradeType direction = marketOrderDirection.Value;

            // Check Order Filters like spacing (time & price)  
            if (!Strategy.CheckOrderFilters(direction) || !this.CheckOrderFilters(direction)) return;

            // Pipeline Risk assesment should execute first!
            // Before Market Order
            if (!this.BeforeSendMarketOrder(direction,
                  Strategy.Symbol,
                  GetVolume(direction),
                  Strategy.GetLabel(),
                  GetStopLoss(direction),
                  GetTakeProfit(direction))) return;

            
            var vol = GetVolume(direction);
            var label = Strategy.GetLabel();
            var sl = GetStopLoss(direction);
            var tp = GetTakeProfit(direction);

            // Strategy Risk assesment should go here
            // Check strategy modules
            if (!Strategy.BeforeSendMarketOrder(direction,
                Strategy.Symbol,
                vol,
                label,
                sl,
                tp )) return;

            // Execute trade
            TradeResult result = Strategy.Robot.ExecuteMarketOrder(
                direction,
                Strategy.Symbol,
                vol,
                label,
                sl,
                tp,
                0.5,   // slippage
                this.Name  // OrderPipeline name
                );

            if (result.Error.HasValue)
            {
                Strategy.Robot.Print("Market Order Failed: " + result.Error.Value + ", " + result.ToString());
            }
        }

        private bool BeforeSendMarketOrder(TradeType direction, Symbol symbol, long vol, string label, double? stoploss, double? takeprofit)
        {
            foreach (var module in _beforeMarketOrder)
            {
                if (!((IBeforeMarketOrder)module).OnBeforeMarketOrder(
                    direction, symbol, vol, label, stoploss, takeprofit))
                {
                    return false;
                }
            }
            return true;
        }

        private long GetVolume(TradeType direction)
        {
            double? lotSize = 0.0;
            long volume = 1000;
            if (LotSizeModule != null)
            {
                lotSize = LotSizeModule.GetLotSize(direction);
            }
            else
            {
                lotSize = Strategy.GetLotSize(direction);
            }
            if (lotSize.HasValue)
            {
                volume = Strategy.Symbol.QuantityToVolume(lotSize.Value);
            }
            return volume;
        }

        private double? GetStopLoss(TradeType direction)
        {
            if (StopLossModule != null)
            {
                return StopLossModule.GetStopLoss(direction);
            }
            else
            {
                return Strategy.GetStopLoss(direction);
            }
        }

        private double? GetTakeProfit(TradeType direction)
        {
            if (TakeProfitModule != null)
            {
                return TakeProfitModule.GetTakeProfit(direction);
            }
            else
            {
                return Strategy.GetTakeProfit(direction);
            }
        }


        /// <summary>
        /// Checks all market condition filters for this order pipeline
        /// </summary>
        /// <returns></returns>
        public bool CheckMarketFilters()
        {
            foreach (var module in MarketFilters)
            {
                if (module is IMarketFilter)
                {
                    if (!((IMarketFilter)module).IsValidMarketCondition())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks all order filters on this pipeline
        /// </summary>
        /// <returns></returns>
        public bool CheckOrderFilters(TradeType direction)
        {
            foreach (var module in _orderFilters)
            {
                if (module is IOrderFilter)
                {
                    if (((IOrderFilter)module).ConfirmOrder(direction) == false)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///  Determines what kind of module this is and loads in into the appropriate slot
        /// </summary>
        /// <param name="module"></param>
        private void AddModule(ModuleBase module)
        {
            _allmodules.Add(module);

            // Validate and show available settings for module
            if (module is IValidateSettings)
            {
                Strategy.ValidateModuleSettings((IValidateSettings)module);
            }

            if (module is IMarketFilter)
            {
                _marketFilters.Add((IMarketFilter)module);
            }

            if (module is IMarketEntryLogic)
            {
                MarketEntryModule = (IMarketEntryLogic)module;
            }

            if (module is ITakeProfit)
            {
                TakeProfitModule = (ITakeProfit)module;
            }

            if (module is IStopLoss)
            {
                StopLossModule = (IStopLoss)module;
            }

            if (module is ILotSize)
            {
                LotSizeModule = (ILotSize)module;
            }

            if (module is IOrderFilter)
            {
                _orderFilters.Add((IOrderFilter)module);
            }

            if (module is IPositionClosed)
            {
                Strategy.AddPositionClosed((IPositionClosed)module);
            }

            if (module is IPositionOpened)
            {
                Strategy.AddPositionOpened((IPositionOpened)module);
            }
        }

        public void Dispose()
        {
            _allmodules.Clear();
            _marketFilters.Clear();
            _marketFilters = null;
            _orderFilters.Clear();
            _orderFilters = null;
            TakeProfitModule = null;
            StopLossModule = null;
            LotSizeModule = null;
            _strategy = null;
        }
    }
}
