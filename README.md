
# CarbonFx Modules Lib

This library allows for modularized strategy development using cAlgo. 

## The Problem 

I've been building strategies for several years now and have a pretty extensive colleciton of bots and indicators.  
The problem emerged that I had lots of interesting pieces of entry, exit, money management logic in all these files,
but no way to easily mix and match them into new strategies.

I tried subclassing, utility libs, and extension methods, but it always bound my logic to a particular 
file.  I ways had to run functions at a particular place in the code, deal with instantiations, and dependencies. 

Everything had to conform to cAlgo's onBar, onTick, PositionFilled, PositionClosed, hooks. The issue is that, most 
interesting pieces of logic span more than one of those hooks, so you have to copy and paste functions into them.
It was a pain in the butt and prone to errors, so I took a step back.

Most strategies boiled down to some very basic pieces.

- Market Filters
- Order Filters
- Entry
    - Take Profit
    - Stop Loss
    - Lot sizing
- Money Management


## A Solution

To be able to focus on building reusable modules instead of complete strategies, I 
needed a way to inject code into many different places, without interfering with anything else.  To accomplish this, 
I created my own hooks and allowed modules to implement Interfaces to run code at
particular places in a strategies lifecycle.

The basic **Interfaces** mirror the standard cAlgo hooks. 

`IModuleInit` - Runs code OnStart

`IOnBar` - Runs code OnBar

`IOnTick` - Runs code OnTick

`IPositionClose` - Runs when position is closed

`IPositionOpened` - Runs when position is opened


In addition to the standard hooks, I also defined what I like to call an Order Pipeline, for market orders.

An **Order Pipeline** is a common sequence of checks that must be done before opening a position.

1. `IMarketFilter (bool)` - Check to make sure the overall market condition is OK.  Things like volume, time of day, no news, etc.

2. `IMarketEntryLogic (buy|sell|null)` - Should we buy or sell?  Maybe check overbought oversold levels?  MACD? Divergences?

3. `IOrderFilter (bool)` - If IMarketEntry returns a buy or sell, it is passed to this method, so you can do further checks.   
    - For example: Check order spacing, or maybe a higher timeframe market filter.   

4. Execute all order related interfaces.  These interfaces allow you to fiddle with LotSize, TakeProfit, and StopLoss completely independent of other modules and even override them. Each of these methods will pass the TradeType.
    - `IStopLoss (double)`   
    - `ITakeProfit (double)`
    - `ILotSize (double)`

5. `IBeforeExecutePosition (bool)` - Last chance to check all the order variables before execution.  The values (TradeType,LotSize,SL,TP) from previous steps are passed into this method.
    

6. `IPositionOpened `- Position is opened


Having interfaces arranged in a pipeline allow a lot of freedom to build smart modules that handle one particular task. 

Here are some examples: 

An `ITakeProfit` module that sets the TP to a previous support or resistance level.

An `IMarketFilter` module that prevents orders from being placed during quiet market hours. 20-22 GMT

An `IOrderFilter` module that makes sure that orders are at least 1 hour and 15 pips apart. Both params are configurable settings.

An `IMarketEntryLogic` module that ensures the RSI has reached the top and is trending back, plus MACD is converging. 


All modules are independent, so you can mix and match without issue.  

Strategies go from being a large block of monolithic code, to the following.   


```

void OnStart()
{
    // Create strategy based on modules
    var strategy1 = StrategyBuilder.CreateNewStrategy(
        this, "SuperAwesomeBot", 
        new string[]{   // Strategy Level Modules
            "MarketCondition.DontTradeOnFridays",
            "EquityManagement.GlobalEquityHighWater",
            "OrderProtection.TrailingStops",
            "TakeProfit.FixedTakeProfit",
            "BaconWrap.cAlgo.Telegram.PipelineManager, BaconWrap.cAlgo",
        });

    // Define and load modules for OnBar pipeline
    sb.AddOrderPipeline("overboughtoversold", CheckForEntry.OnBar, new string[] 
        {
            "OrderFilter.MarketProfile",
            "OrderFilter.FixedOrderSpacing",
            "EntryLogic.OverboughtOversold",
        });

    // Define and load modules for another pipeline
    sb.AddOrderPipeline("srpowerlevels", CheckForEntry.OnTick, new string[] 
        {  
            "OrderFilter.SRClustering",
            "EntryLogic.SRPowerLevels",
        });
    

    this.AddStrategy(strategy1.InitializeStrategy());
}

```




**Utility Interfaces**

IReportStatus (string) - This can be used to report current state of a module in a friendly way.  For example: an RSI module that measures trend strength.  These are primary used to report back to a Telegram client.

IValidateSettings - Each module can have it's own settings, this interface allows you to define which settings are available and required (throws exception is not defined).

# Disclaimer

This project is still a work in progress.  It might not work correctly, may throw exceptions, and may cause your account to blow up.

# License

GNU AGPLv3

