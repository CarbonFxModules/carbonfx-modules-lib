using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib
{
    [Flags]
    public enum eMarketState
    {
        Undefined = 0,
        Ranging = 1,
        Bullish = 2,
        Bearish = 4
    }

    public enum CheckForEntry
    {
        OnTick,
        OnBar
    }

    /// <summary>
    /// SupportResistanceLevels
    /// </summary>
    public enum SRLevels
    {
        Support,
        Resistance
    }

    public enum OrderPositionType
    {
        Order,
        Position
    }

    public enum PendingOrderType
    {
        Stop,
        Limit
    }
}
