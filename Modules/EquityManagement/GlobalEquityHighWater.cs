using CarbonFxModules.Lib;
using System;

namespace CarbonFxModules.Modules.EquityManagement
{
    public class GlobalEquityHighWater : ModuleBase, IOnBar, IModuleInit, IValidateSettings
    {
        const string modulePrefix = "GlobalEquityHW_";
        const string HighWaterPlus = modulePrefix + "HighWaterPlus";

        double _highWaterPlus;
        private double highwaterMark = 0;

        public void InitModule()
        {
            // Use balance so that underwater orders movement don't count towards this
            highwaterMark = Strategy.Robot.Account.Balance;

            // Dollar amount equity must increase before closing orders
            _highWaterPlus = Settings.Get<double>(HighWaterPlus, 5.0);
        }

        public void OnBar()
        {
            if (Strategy.Robot.Account.Equity > (highwaterMark + _highWaterPlus))
            {
                foreach (var p in Strategy.Robot.Positions)
                {
                    Strategy.Robot.ClosePosition(p);
                }
                highwaterMark = Strategy.Robot.Account.Equity;
            }
        }

        public string[] CheckRequiredSettings()
        {
            return new string[] { };
        }

        public string[] GetAvailableSettings()
        {
            return new string[] { HighWaterPlus };
        }
    }
}
