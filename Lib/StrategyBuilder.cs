using CarbonFxModules.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using CarbonFxModules.Modules;
using cAlgo.API.Internals;
using CarbonFxModules.Lib;

namespace CarbonFxModules.Lib
{
    /// <summary>
    /// Factory that builds strategies
    /// </summary>
    public class StrategyBuilder
    {
        /// <summary>
        /// Creates a new Strategy with the following modules
        /// </summary>
        /// <param name="bot">cAlgoBot</param>
        /// <param name="label">Strategy Name aka label</param>        
        /// <param name="moduleNames">Modules for strategy, reverse loaded</param>
        /// <returns></returns>
        public static StrategyBuilder CreateNewStrategy(Robot bot, string label, string[] moduleNames)
        {
            var modules = FindAndActivateModules(bot, moduleNames);
            return new StrategyBuilder(bot, label, bot.Symbol, modules);
        }

        private static List<ModuleBase> FindAndActivateModules(Robot bot, string[] moduleNames)
        {
            List<ModuleBase> modules = new List<ModuleBase>();
            foreach (var module in moduleNames.Reverse())
            {
                try
                {
                    var modObj = ActivateModule(module);
                    modules.Add(modObj);
                }
                catch (Exception ex)
                {
                    bot.Print("Warning: Failed to load module:" + module + "--" + ex.Message);
                }
            }
            return modules;
        }

        /// <summary>
        /// Pulls module from this assembly and instantiates it
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ModuleBase ActivateModule(string type)
        {
            Type typeObj = null;
            string assemblyName = string.Empty;
            if (type.Contains(","))
            {
                var parts = type.Split(',');
                type = parts[0].Trim();
                assemblyName = parts[1].Trim();

                var asm = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => a.FullName.Contains(assemblyName)).FirstOrDefault();
                if (asm == null)
                {
                    throw new Exception("Unable to find assembly (dll): " + assemblyName);
                }

                typeObj = asm.GetTypes()
                                 .Where(t => t.FullName == type)
                                 .FirstOrDefault();
            }
            else
            {
                typeObj = Type.GetType(type);
            }

            if (typeObj == null)
            {
                throw new Exception("Unable to find Module: " + type);
            }
            var obj = Activator.CreateInstance(typeObj);
            return (ModuleBase)obj;
        }

        private Robot bot;
        private Symbol symbol;
        StrategyBase _strategy;

        private StrategyBuilder(Robot bot, string label, Symbol symbol, List<ModuleBase> modules)
        {
            this.bot = bot;
            this.symbol = symbol;
            _strategy = new StrategyBase(bot, symbol, label);
            foreach (var module in modules)
            {
                _strategy.AddModule(module);
            }
        }

        public StrategyBase InitializeStrategy()
        {
            _strategy.Init();
            var temp = _strategy;
            _strategy = null;

            if (temp.Telegram != null)
            {
                temp.Telegram.SendMessage("*Starting:*\n{0}", temp.GetAccountInfo());
            }
            return temp;
        }

        public StrategyBuilder AddModule(string name)
        {
            _strategy.AddModule(ActivateModule(name));
            return this;
        }

        public StrategyBuilder AddModule(ModuleBase module)
        {
            _strategy.AddModule(module);
            return this;
        }

        public StrategyBuilder AddOrderPipeline(string label, CheckForEntry entryType, string[] moduleNames, Dictionary<string, object> settingsOverrides = null)
        {
            var modules = FindAndActivateModules(_strategy.Robot, moduleNames);
            if (entryType == CheckForEntry.OnBar)
            {
                _strategy.AddOrderPipeline(
                    new OnBarOrderPipeline(_strategy, label, modules, settingsOverrides)
                );
            }
            else if (entryType == CheckForEntry.OnTick)
            {
                _strategy.AddOrderPipeline(
                    new OnTickOrderPipeline(_strategy, label, modules, settingsOverrides)
                );
            }
            return this;
        }
    }
}
