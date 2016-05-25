﻿using CarbonFxModules.Bots;
using CarbonFxModules.Lib;

namespace CarbonFxModules
{
    public class ExampleStrategy : StrategyBotBase
    {

        protected override void OnStart()
        {

            var strategy1 =
                StrategyBuilder.CreateNewStrategy(
                    this,
                    "Test1",  // Label                    
                    new string[]{   // All modules for this this strategy
                        "EntryLogic.CandleHighLowLevels",
                        "MarketCondition.DontTradeOnFridays",
                        "EquityManagement.GlobalEquityHighWater",
                        "OrderProtection.TrailingStops",
                        "OrderSpacing.FibonaciiOrderSpacing",
                        "TakeProfit.FixedTakeProfit"
                    }).InitializeStrategy();

            this.AddStrategy(strategy1);
            
        }

    }
}
