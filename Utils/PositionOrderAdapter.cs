using cAlgo.API;
using CarbonFxModules.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{
    public interface ICommonPositionOrder
    {
        TradeType Direction { get; }
        double Price { get; }
        double? TakeProfit { get; }
        double? StopLoss { get; }
        double? TakeProfitDiff { get; }
        double? StopLossDiff { get; }
        long Volume { get; }
        OrderPositionType Type { get; }
    }
    public class PositionOrderAdapter : ICommonPositionOrder
    {
        private Position p = null;
        private PendingOrder o = null;

        public double Price
        {
            get
            {
                if (p != null)
                {
                    return p.EntryPrice;
                }
                if (o != null)
                {
                    return o.TargetPrice;
                }
                return double.NaN;
            }
        }

        public double? TakeProfit
        {
            get
            {
                if (p != null)
                {
                    return p.TakeProfit;
                }
                if (o != null)
                {
                    return o.TakeProfit;
                }
                return double.NaN;
            }
        }

        public double? StopLoss
        {
            get
            {
                if (p != null)
                {
                    return p.StopLoss;
                }
                if (o != null)
                {
                    return o.StopLoss;
                }
                return double.NaN;
            }
        }

        public double? TakeProfitDiff
        {
            get
            {
                if (p != null)
                {   
                    return p.TakeProfit.HasValue ? Math.Abs(p.TakeProfit.Value - p.EntryPrice) : (double?)null;
                }
                if (o != null)
                {
                    return o.TakeProfit.HasValue ? Math.Abs(o.TakeProfit.Value - o.TargetPrice) : (double?)null;
                }
                return double.NaN;
            }
        }

        public double? StopLossDiff
        {
            get
            {
                if (p != null)
                {
                    return p.StopLoss.HasValue ? Math.Abs(p.StopLoss.Value - p.EntryPrice) : (double?)null;
                }
                if (o != null)
                {
                    return o.StopLoss.HasValue ? Math.Abs(o.StopLoss.Value - o.TargetPrice) : (double?)null;
                }
                return double.NaN;
            }
        }

        public long Volume
        {
            get
            {
                if (p != null)
                {
                    return p.Volume;
                }
                if (o != null)
                {
                    return o.Volume;
                }
                return 0;
            }
        }

        public TradeType Direction
        {
            get
            {
                if (p != null)
                {
                    return p.TradeType;
                }
                else
                {
                    return o.TradeType;
                }
            }
        }

        public OrderPositionType Type
        {
            get
            {
                if (p != null)
                {
                    return OrderPositionType.Position;
                }
                else
                {
                    return OrderPositionType.Order;
                }
            }
        }

        public PositionOrderAdapter(Position p)
        {
            this.p = p;
        }

        public PositionOrderAdapter(PendingOrder o)
        {
            this.o = o;
        }
    }
}
