using CarbonFxModules.Lib;

namespace CarbonFxModules.Modules.MarketCondition
{
    public class AsianSession : ModuleBase, IMarketFilter
    {
        public bool IsValidMarketCondition()
        {
            var serverTime = Strategy.Robot.Server.Time;
            return serverTime.Hour >= 0 && serverTime.Hour <= 7;
        }
    }
}
