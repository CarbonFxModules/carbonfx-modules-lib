using cAlgo.API;
using cAlgo.API.Internals;
using CarbonFxModules.Exceptions;
using CarbonFxModules.Lib.Telegram;
using CarbonFxModules.Modules;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib
{
    /// <summary>
    /// Wraps the Robot and provides scoped methods for this Strategy
    /// Potentially running multiple strategies under 1 Robot
    /// </summary>
    public class StrategyBase : IStrategyBase, IDisposable
    {
        #region Public Properties 

        private Robot _robot;
        private Symbol _symbol;
        private string _name;
        public Symbol Symbol
        {
            get
            {
                return _symbol;
            }
        }

        public Robot Robot
        {
            get
            {
                return _robot;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        private StrategyTelegram _telegramClient;
        private ITelegram _nooptelegramClient;
        public ITelegram Telegram
        {
            get
            {
                if(_telegramClient == null)
                {
                    if(_nooptelegramClient == null)
                    {
                        _nooptelegramClient = new NoopTelegram();
                    }
                    return _nooptelegramClient;                  
                }
                return _telegramClient;
            }
        }

        #endregion

        #region Init

        public StrategyBase(Robot bot) 
        {   
            _init(bot, bot.Symbol, this.GetType().Name);
        }

        public StrategyBase(Robot bot, Symbol symbol)
        {
            _init(bot, symbol, this.GetType().Name);            
        }

        public StrategyBase(Robot bot, Symbol symbol, string name)
        {
            _init(bot, symbol, name);
        }

        private void _init(Robot bot, Symbol symbol, string name)
        {
            _robot = bot;
            _symbol = symbol;
            _name = name;
            LoadSettings(_robot);
            initializeTelegram();
            attachEventHandlers();
        }

        private void attachEventHandlers()
        {
            _robot.Positions.Closed += (PositionClosedEventArgs obj) =>
            {
                if (obj.Position.Label == this.GetLabel() && this.IsInitialized())
                {
                    foreach (var o in _positionClosed)
                    {
                        o.OnPositionClosed(obj.Position);
                    }
                }
            };

            _robot.Positions.Opened += (PositionOpenedEventArgs obj) =>
            {
                if (obj.Position.Label == this.GetLabel() && this.IsInitialized())
                {
                    foreach (var o in _positionOpened)
                    {
                        o.OnPositionOpened(obj.Position);
                    }
                }
            };
        }


        public bool IsInitialized()
        {
            return _hasInitialized;
        }

        private void initializeTelegram()
        {
            try
            {
                if (this.Robot.IsBacktesting)
                {
                    _nooptelegramClient = new NoopTelegram();
                }
                else
                {
                    _telegramClient = new StrategyTelegram(this);
                }
            }
            catch (Exception ex)
            {
                Robot.Print("StrategyBase: Error starting telegram client {0}", ex.Message);
            }
        }


        #endregion

        #region Settings

        internal SettingsDictionary _settings = new SettingsDictionary();

        public IStrategySettings Settings
        {
            get
            {
                return _settings;
            }
        }


        private void LoadSettings(Robot robot)
        {
            PropertyInfo[] props = robot.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    ParameterAttribute cAlgoParam = attr as ParameterAttribute;
                    if (cAlgoParam != null)
                    {
                        _settings.Set(prop.Name, prop.GetValue(robot));
                    }
                }
            }
        }




        /// <summary>
        /// Overrides settings in this strategy
        /// </summary>
        /// <param name="settingsOverride"></param>
        public void OverrideSettings(Dictionary<string, object> settingsOverride)
        {
            foreach (var item in settingsOverride)
            {
                _settings.Set(item.Key, item.Value);
            }
        }

        #endregion

        #region Module Initialization

        private bool _hasInitialized = false;
        /// <summary>
        /// Initalizes all modules including the ones in the orderPipelines.
        /// </summary>
        internal void Init()
        {
            try
            {
                List<IModuleInit> _initThese = new List<IModuleInit>();
                foreach (var module in _allModules)
                {
                    if (module is IModuleInit)
                    {
                        _initThese.Add((IModuleInit)module);
                    }
                }
                foreach (var pipeline in _orderPipelines)
                {
                    foreach (var module in pipeline.Modules)
                    {
                        if (module is IModuleInit)
                        {
                            _initThese.Add((IModuleInit)module);
                        }
                    }
                }
                _inflightInitializations = _initThese.Count;
                foreach (var module in _initThese)
                {
                    initModule(module, ((ModuleBase)module).Name);
                }                
            }
            catch (Exception ex)
            {
                Robot.Print("StrategyBase.Init error: " + ex.Message);
            }
        }

        object locker = new object();
        int _inflightInitializations = 0;
        private void initModule(IModuleInit module, string moduleName)
        {
            Robot.BeginInvokeOnMainThread(() =>
            {
                // This Invoke is on a different thread 
                // so it's fire and forget, BUT, we can't run other parts of the 
                // modules until everything is initialized.
                // This makes sure all modules are initialized 
                // before setting the _hasInitialized flag

                try
                {
                    module.InitModule();
                }
                catch (SettingNotFound ex)
                {
                    this.Robot.Print("Setting not found InitModule:"
                      + moduleName + ", " + ex.Message);                    
                }
                catch (Exception ex)
                {
                    this.Robot.Print("Error in InitModule:"
                      + moduleName + ", " + ex.Message);                    
                }
                finally
                {
                    lock (locker)
                    {
                        _inflightInitializations--;
                    }
                    if (_inflightInitializations == 0)
                    {
                        _hasInitialized = true;
                    }
                }
            });

        }

        #endregion

        #region Module Categorization and Triggerring 

        internal IEnumerable<ModuleBase> Modules
        {
            get
            {
                return _allModules;
            }
        }

        List<ModuleBase> _allModules = new List<ModuleBase>();
        List<IOnBar> _onBarModules = new List<IOnBar>();
        List<IOnTick> _onTickModules = new List<IOnTick>();
        List<IOrderFilter> _orderSpacingModules = new List<IOrderFilter>();
        List<IPositionClosed> _positionClosed = new List<IPositionClosed>();
        List<IPositionOpened> _positionOpened = new List<IPositionOpened>();
        List<IBeforeMarketOrder> _beforeMarketOrder = new List<IBeforeMarketOrder>();
        ITakeProfit _takeProfitModule = null;
        IStopLoss _stopLossModule = null;
        ILotSize _lotSizeModule = null;

        internal void AddModule(ModuleBase module)
        {
            module.Strategy = this;
            module.Settings = this.Settings;

            _allModules.Add(module);

            // Validate and show available settings for module
            if (module is IValidateSettings)
            {
                ValidateModuleSettings((IValidateSettings)module);
            }

            // Setup modules that respond to OnTick
            if (module is IOnTick)
            {
                _onTickModules.Add((IOnTick)module);
            }
            // Setup modules that respond to OnBar
            if (module is IOnBar)
            {
                _onBarModules.Add((IOnBar)module);
            }

            // Baseline order modules
            if (module is ITakeProfit)
            {
                _takeProfitModule = (ITakeProfit)module;
            }

            if (module is IStopLoss)
            {
                _stopLossModule = (IStopLoss)module;
            }

            if (module is ILotSize)
            {
                _lotSizeModule = (ILotSize)module;
            }

            if (module is IOrderFilter)
            {
                _orderSpacingModules.Add((IOrderFilter)module);
            }

            if (module is IPositionClosed)
            {
                _positionClosed.Add((IPositionClosed)module);
            }

            if (module is IPositionOpened)
            {
                _positionOpened.Add((IPositionOpened)module);
            }

            if (module is IBeforeMarketOrder)
            {
                _beforeMarketOrder.Add((IBeforeMarketOrder)module);
            }
        }

        /// <summary>
        /// Adds a module that wants to hear about PositionClosed events
        /// </summary>
        /// <param name="module"></param>
        internal void AddPositionClosed(IPositionClosed module)
        {
            _positionClosed.Add(module);
        }

        /// <summary>
        /// Adds a module that wants to hear about PositionOpened events
        /// </summary>
        /// <param name="module"></param>
        internal void AddPositionOpened(IPositionOpened module)
        {
            _positionOpened.Add(module);
        }

        /// <summary>
        /// Validates module settings IValidateSettings
        /// </summary>
        /// <param name="module"></param>
        internal void ValidateModuleSettings(IValidateSettings module)
        {
            string moduleName = ((ModuleBase)module).Name;
            IValidateSettings temp = (IValidateSettings)module;
            foreach (var key in temp.CheckRequiredSettings())
            {
                if (!Settings.Contains(key))
                {
                    this.Robot.Print("Warning!! Required setting not found for module:" + moduleName + ", " + key);
                    this.Robot.Print("Missing:" + key);
                    return;
                }
            }
            var available = temp.GetAvailableSettings();
            if (available.Length > 0)
            {
                this.Robot.Print("AvailableSettings: {0} - {1}", moduleName, string.Join(", ", available));
            }
        }

        #endregion

        #region Order Pipelines

        List<OrderPipeline> _orderPipelines = new List<OrderPipeline>();
        public IEnumerable<OrderPipeline> OrderPipelines
        {
            get
            {
                return _orderPipelines;
            }
        }

        internal void AddOrderPipeline(OrderPipeline pipeline)
        {
            _orderPipelines.Add(pipeline);
            if (pipeline is IOnTick)
            {
                _onTickModules.Add((IOnTick)pipeline);
            }

            if (pipeline is IOnBar)
            {
                _onBarModules.Add((IOnBar)pipeline);
            }
        }

        #endregion

        #region Module MarketState, Filters, Order Methods

        public bool ConfirmMarketState(eMarketState desiredState)
        {
            bool result = false;
            foreach (var module in _allModules)
            {
                if (module is IMarketState)
                {
                    result = (((IMarketState)module).GetMarketState() & desiredState) == desiredState;
                    if (result == false)
                    {
                        return result;
                    }
                }
            }
            return result;
        }

        public Dictionary<string, eMarketState> GetMarketState()
        {
            Dictionary<string, eMarketState> _summary = new Dictionary<string, eMarketState>();
            foreach (var module in _allModules)
            {
                if (module is IMarketState)
                {
                    _summary.Add(module.Name, ((IMarketState)module).GetMarketState());
                }
            }
            return _summary;
        }

        /// <summary>
        /// Checks all market condition filters (Session, Liquidity, Spread, etc)
        /// </summary>
        /// <returns></returns>
        public bool CheckMarketFilters()
        {
            foreach (var module in _allModules)
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
        /// Last place to stop a market order
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="symbol"></param>
        /// <param name="vol"></param>
        /// <param name="label"></param>
        /// <param name="stoploss"></param>
        /// <param name="takeprofit"></param>
        public bool BeforeSendMarketOrder(TradeType direction, Symbol symbol, long vol, string label, double? stoploss, double? takeprofit)
        {
            foreach (var module in _beforeMarketOrder)
            {
                if(!((IBeforeMarketOrder)module).OnBeforeMarketOrder(
                    direction, symbol, vol, label, stoploss, takeprofit))
                {
                    return false;
                }
            }
            return true;
        }

        public double? GetLotSize(TradeType direction)
        {
            double defaultLotSize = 0.01;
            if (_lotSizeModule != null)
            {
                return _lotSizeModule.GetLotSize(direction);
            }
            else return defaultLotSize;
        }

        public double? GetTakeProfit(TradeType direction)
        {
            if (_takeProfitModule != null)
            {
                return _takeProfitModule.GetTakeProfit(direction);
            }
            else return null;
        }

        public double? GetStopLoss(TradeType direction)
        {
            if (_stopLossModule != null)
            {
                return _stopLossModule.GetStopLoss(direction);
            }
            else return null;
        }

        /// <summary>
        /// Checks order filters at the Strategy level
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool CheckOrderFilters(TradeType direction)
        {
            foreach (var spacing in _orderSpacingModules)
            {
                if (spacing.ConfirmOrder(direction) == false)
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        #region Helper methods to get Orders/Positions for this strategy

        public IEnumerable<PendingOrder> GetPendingOrders()
        {
            return _robot.PendingOrders.Where(p =>
                p.SymbolCode == Symbol.Code &&
                p.Label.StartsWith(GetLabel())
                );
        }

        public IEnumerable<PendingOrder> GetPendingOrders(string comment)
        {
            comment = GetComment(comment);
            return _robot.PendingOrders.Where(p =>
                p.SymbolCode == Symbol.Code &&
                p.Label.StartsWith(GetLabel()) &&
                p.Comment.StartsWith(comment)
                );
        }

        /// <summary>
        /// Positions filtered to this strategy
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Position> GetPositions(string comment)
        {
            return _robot.Positions.Where(p =>
                p.SymbolCode == Symbol.Code &&
                p.Label.StartsWith(GetLabel()) &&
                p.Comment.StartsWith(comment)
                );
        }

        /// <summary>
        /// Positions filtered to this strategy
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Position> GetPositions()
        {
            return _robot.Positions.Where(p =>
                p.SymbolCode == Symbol.Code &&
                p.Label.StartsWith(GetLabel())
                );
        }

        #endregion

        #region Get Order Comment, Label, Strategy Name

        public string GetComment(string label, int cnt)
        {
            return string.Format("{0}|{1}|{2}|{3}",
              Settings.Get<string>("Magic", Name),
              GetVersion(), label, cnt);
        }

        public string GetComment(string label)
        {
            return string.Format("{0}|{1}|{2}",
              Settings.Get<string>("Magic", Name),
              GetVersion(), label);
        }

        public string GetComment()
        {
            return string.Format("{0}|{1}",
              Settings.Get<string>("Magic", Name),
              GetVersion());
        }

        public string GetVersion()
        {
            var moduleVersions = Settings.GetAllKeys()
              .Where(s => s.Contains("Version"))
              .Select(s =>
              {
                  return Settings.Get<string>(s);
              });
            return string.Join(":", moduleVersions);
            //return string.Format("{0}:{1}:{2}", Settings.Get<int>("GetLevelsVersion"), Settings.Get<int>("PlaceOrdersVersion"), Settings.Get<int>("ManageOrdersVersion"));
        }

        public string GetAccountInfo()
        {
            var _strategy = this;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("*{0}*\n", _strategy.GetLabel() + "-" + _strategy.Robot.Account.Number);
            sb.AppendFormat("{0} TickVal: ${1:0.00} per lot\n", _strategy.Symbol.Code, _strategy.Symbol.PipValue * _strategy.Symbol.LotSize);
            sb.AppendFormat("Balance: {0:0.00}\n", _strategy.Robot.Account.Balance);
            sb.AppendFormat("Equity: {0:0.00}\n", _strategy.Robot.Account.Equity);
            sb.AppendFormat("Float: {0:0.00}\n", _strategy.Robot.Account.UnrealizedNetProfit);
            sb.AppendFormat("Margin: {0:0.00}\n", _strategy.Robot.Account.Margin);
            sb.AppendFormat("Free Margin: {0:0.00}\n", _strategy.Robot.Account.FreeMargin);
            sb.AppendFormat("Margin Level: {0:0.0}%\n", _strategy.Robot.Account.MarginLevel);
            sb.Append(_strategy.Robot.Account.IsLive ? "*Live*" : "*Demo*");
            return sb.ToString();
        }

        [Obsolete]
        public string GetBotName()
        {
            return Name;
        }

        /// <summary>
        /// Gets official label for strategy.  Botname-Magic
        /// </summary>
        /// <returns></returns>
        public string GetLabel()
        {
            return string.Format("{0}-{1}",
              Name,
              Settings.Get<string>("Magic", "000"));
        }

        #endregion

        #region cAlgo OnTick OnBar methods that interate over all registered modules

        internal void OnTick()
        {
            foreach (var module in _onTickModules)
            {
                module.OnTick();
            }
        }

        internal void OnBar()
        {
            foreach (var module in _onBarModules)
            {
                module.OnBar();
            }
        }

        #endregion

        #region Dispose / Shutdown / Cleanup

        public void Dispose()
        {
            if (_telegramClient != null)
            {
                _telegramClient.Dispose();
                _telegramClient = null;
            }
            _onBarModules.Clear();
            _onTickModules.Clear();
            _allModules.Clear();
            _orderSpacingModules.Clear();
            _takeProfitModule = null;
            _stopLossModule = null;
            _lotSizeModule = null;
            foreach (var pipeline in _orderPipelines)
            {
                pipeline.Dispose();
            }
            _orderPipelines.Clear();
        }

        #endregion
    }
}
