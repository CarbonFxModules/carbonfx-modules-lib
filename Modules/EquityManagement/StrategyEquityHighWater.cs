using cAlgo.API;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.EquityManagement
{
    public class StrategyEquityHighWater : ModuleBase, IOnBar, IModuleInit, IValidateSettings, IPositionClosed
    {
        const string modulePrefix = "StratEquityHW_";
        const string HighWaterPlus = modulePrefix + "HighWaterPlus";

        double _highWaterPlus;
        private double _highwaterMark = 0;
        private double _positionProfits = 0;

        public void InitModule()
        {
            // Dollar amount equity must increase before closing orders
            _highWaterPlus = Settings.Get<double>(HighWaterPlus, 5.0);
            _highwaterMark = Strategy.Robot.Account.Equity;
        }

        public void OnBar()
        {
            if (Strategy.Robot.Account.Equity > (_highwaterMark + _highWaterPlus))
            {
                var positions = Strategy.GetPositions();
                double netProfit = positions.Sum(p => p.NetProfit);
                // Only close positions if we've accumulated move profit
                // than our floating positions and if they are negative
                if (_positionProfits > netProfit && netProfit < 0)
                {
                    foreach (var p in positions)
                    {
                        Strategy.Robot.ClosePosition(p);
                    }
                }
                _positionProfits = 0;
                _highwaterMark = Strategy.Robot.Account.Equity;
            }
        }

        public void OnPositionClosed(Position p)
        {
            _positionProfits += p.NetProfit;
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
