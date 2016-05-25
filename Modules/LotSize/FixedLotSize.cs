using cAlgo.API;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.LotSize
{
    public class FixedLotSize: ModuleBase, ILotSize, IValidateSettings
    {
        const string modulePrefix = "FixedLot_";
        const string @LotSize = modulePrefix + "Size";
                
        public double? GetLotSize(TradeType direction)
        {
            return Settings.Get<double>(@LotSize, 0.01);
        }

        public string[] CheckRequiredSettings()
        {
            return new string[]
            {
                @LotSize
            };
        }

        public string[] GetAvailableSettings()
        {
            return new string[] {};
        }
    }
}
