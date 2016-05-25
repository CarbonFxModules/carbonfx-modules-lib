
# Why modules?

Modules are an useful way of organizing little pieces of a strategy. 
Instead of starting from scratch or copy and pasting, you should be able to 
drop in a new entry, SL, TP, or other modules without having to rearrange 
code.  

Just build the new piece and combine it with oldder pieces.  This way 
no functionality gets locked into a bot and instead can be swapped, improved or removed at any time.


```

Module Loading:
Module loading starts from top down, so later modules override 
previous modules by default.
However, I can create an Attribute that defines the behavior differently.
 

	var strategy = BotFactory.CreateNewStrategy(this, "Test1", "EURUSD", 
		string[]{ 
			"CandleHighLowLevels", 
			"GlobalEquityHighWater", 
			"TrailingStops", 
			"FibonaciiOrderSpacing", 
			"FixedTakeProfit"
		})
		.OverrideSettings(eurusdSettings)
		.Init();

strategy.createOrderPipeline(
		"DontTradeOnFriday",
		"RSIOversold",
		"BullishLevels"


BotFactory.CreateNew(Label, 
	SingleOrderStrategy()
		.SetEntry(EntryVersion)
		.SetTP(TPVersion)
		.SetSL(SLVersion)
		.SetOrderSpacing(OrderSpacingVersion)
		.SetRiskManagement(RiskManagementModule));

BotFactory.CreateNew(Label, 
	MultiOrderStrategy()
		.SetOrder(

BotFactory.CreateNew(Label, 
	SingleOrderStrategy(GetSymbol(EURUSD)));
BotFactory.CreateNew("GBPUSD", "", 
	SingleOrderStrategy());


BotFactory.FetchPackages(
	"CandleHighLowLevels", 
	"GlobalEquityHighWater", 
	"TrailingStops", 
	"FibonaciiOrderSpacing", 
	"FixedTakeProfit"



BotFactory.CreateNew("GBPUSD", "", SingleOrderStrategy()

);

OnStart()
{


	var bot = new StrategyBase(this, Symbol.Code, settingsOverride);


}



```