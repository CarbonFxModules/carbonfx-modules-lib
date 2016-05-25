using cAlgo.API;
using cAlgo.API.Internals;
using CarbonFxModules.Lib;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules.OrderFilter
{
    /// <summary>
    /// FibonaciiOrderSpacing looks at open positions and pending orders and ensure that the current market price
    /// has moved away a certain amount based on preview H8 candle high-low * fib[positionCnt or orderCnt]
    /// </summary>
    public class FibonaciiOrderSpacing : ModuleBase, IOrderFilter, IModuleInit
    {
        MarketSeries _largeCandles;
        double[] _fibonacciLevels = Utilities.FibonacciLevels;

        public void InitModule()
        {
            _largeCandles = Strategy.Robot.MarketData.GetSeries(Strategy.Symbol, TimeFrame.Hour8);
        }

        public bool ConfirmOrder(TradeType direction)
        {
            var positions = Strategy.GetPositions().Where(p=> p.TradeType == direction);
            var orders = Strategy.GetPendingOrders().Where(p => p.TradeType == direction);

            int positionCount = positions.Count();
            int orderCount = orders.Count();

            if (positionCount == 0 && orderCount == 0) return true;

            double pipSize = Strategy.Symbol.PipSize;

            // find distance the market moved over the last 8 hours.
            int cnt = _largeCandles.High.Count-1;
            var lastCandleRangePips = (_largeCandles.High[cnt-1] - _largeCandles.Low[cnt - 1]) / pipSize;

            if (positionCount > 0)
            {
                // find last order
                var closestPosition = positions.OrderBy(p => Math.Abs(p.Pips)).First();
                // see if pips is greater than last candle range * some fib percentage
                if (Math.Abs(closestPosition.Pips) > lastCandleRangePips * _fibonacciLevels[positionCount])
                {
                    return true;
                }
            }

            if (orderCount > 0)
            {
                // find last order
                var closestOrder = orders.OrderBy(p => Math.Abs(Strategy.Symbol.Ask - p.TargetPrice)).First();
                // how far has market moved from last order
                var orderToherePips = Math.Abs(Strategy.Symbol.Ask - closestOrder.TargetPrice) / pipSize;
                // see if pips is greater than last candle range * some fib percentage
                if (orderToherePips > lastCandleRangePips * _fibonacciLevels[positionCount])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
