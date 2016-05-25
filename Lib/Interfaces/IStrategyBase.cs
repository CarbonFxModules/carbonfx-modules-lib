using cAlgo.API;
using cAlgo.API.Internals;
using CarbonFxModules.Lib.Telegram;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Lib
{
    public interface IStrategyBase
    {
        IStrategySettings Settings { get; }
        Robot Robot { get; }
        Symbol Symbol { get; }
        ITelegram Telegram { get; }
        IEnumerable<OrderPipeline> OrderPipelines { get; }
        IEnumerable<Position> GetPositions();
        IEnumerable<PendingOrder> GetPendingOrders();
        string GetLabel();
    }
}
