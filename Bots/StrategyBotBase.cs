using cAlgo.API;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Bots
{
    public class StrategyBotBase : Robot
    {
        List<StrategyBase> _strategies = new List<StrategyBase>();

        /// <summary>
        /// Adds a strategy to this bot
        /// </summary>
        /// <param name="strat"></param>
        public void AddStrategy(StrategyBase strat)
        {
            _strategies.Add(strat);
        }

        protected override void OnTick()
        {
            foreach (var strat in _strategies)
            {
                if (strat.IsInitialized())
                {
                    strat.OnTick();
                }
            }
        }

        protected override void OnBar()
        {
            foreach (var strat in _strategies)
            {
                if (strat.IsInitialized())
                {
                    strat.OnBar();
                }
            }
        }

        protected override void OnStop()
        {
            foreach (var strat in _strategies)
            {
                if (!IsBacktesting)
                {
                    strat.Telegram.SendMessage("*Shutting down: {0}*", strat.GetLabel());
                }
                strat.Dispose();
            }
        }

    }
}
