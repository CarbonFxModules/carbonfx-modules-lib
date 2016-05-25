using cAlgo.API;
using cAlgo.API.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// AppendLine + AppendFormat
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="msg"></param>
        /// <param name="obj"></param>
        public static void AppendLine(this StringBuilder sb, string msg, params object[] obj)
        {
            sb.AppendLine();
            sb.AppendFormat(msg, obj);
        }

        /// <summary>
        /// Slice an array
        /// </summary>
        /// <param name="list"></param>
        /// <param name="startIdx"></param>
        /// <param name="endIdx"></param>
        /// <returns></returns>
        public static IList<T> Slice<T>(this IList<T> list, int startIdx, int endIdx = -1)
        {
            List<T> outArray = new List<T>();
            if (endIdx == -1) endIdx = list.Count;
            while(startIdx < endIdx)
            {
                outArray.Add(list[startIdx++]);
            }
            return outArray;
        }

        /// <summary>
        /// Average price between ask and bid--(sym.Ask + sym.Bid) / 2;
        /// </summary>
        /// <param name="sym"></param>
        /// <returns></returns>
        public static double AveragePrice(this Symbol sym)
        {
            return (sym.Ask + sym.Bid) / 2;
        }

        public static double CostPerPip(this Symbol sym, long volume)
        {
            return sym.PipValue * volume;
        }


        /// <summary>
        ///  Gets the WeightedAverage Price of open positions
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static double WeightedPriceLevel(this IEnumerable<Position> positions)
        {
            double weightedPrice = 0.0;
            double totalVol = 0;
            foreach (var p in positions)
            {
                weightedPrice += p.EntryPrice * p.Volume;
                totalVol += p.Volume;
            }
            double beLevel = weightedPrice / totalVol;
            return beLevel;
        }

        /// <summary>
        ///  Gets the WeightedAverage Price of PositionOrder
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        public static double WeightedPriceLevel(this IEnumerable<ICommonPositionOrder> positions)
        {
            double weightedPrice = 0.0;
            double totalVol = 0;
            foreach (var p in positions)
            {
                weightedPrice += p.Price * p.Volume;
                totalVol += p.Volume;
            }
            double beLevel = weightedPrice / totalVol;
            return beLevel;
        }

        /// <summary>
        /// Trys to parse string for cAlgo.API.TimeFrame value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="timeFrameOut"></param>
        /// <returns></returns>
        public static bool TryParseTimeFrame(string name, out TimeFrame timeFrameOut)
        {
            switch (name)
            {
                case "Daily":
                    timeFrameOut = TimeFrame.Daily;
                    return true;

                case "Day2":
                    timeFrameOut = TimeFrame.Day2;
                    return true;

                case "Day3":
                    timeFrameOut = TimeFrame.Day3;
                    return true;

                case "Hour":
                    timeFrameOut = TimeFrame.Hour;
                    return true;

                case "Hour12":
                    timeFrameOut = TimeFrame.Hour12;
                    return true;

                case "Hour2":
                    timeFrameOut = TimeFrame.Hour2;
                    return true;

                case "Hour3":
                    timeFrameOut = TimeFrame.Hour3;
                    return true;

                case "Hour4":
                    timeFrameOut = TimeFrame.Hour4;
                    return true;

                case "Hour6":
                    timeFrameOut = TimeFrame.Hour6;
                    return true;

                case "Hour8":
                    timeFrameOut = TimeFrame.Hour8;
                    return true;

                case "Minute":
                    timeFrameOut = TimeFrame.Minute;
                    return true;

                case "Minute10":
                    timeFrameOut = TimeFrame.Minute10;
                    return true;

                case "Minute15":
                    timeFrameOut = TimeFrame.Minute15;
                    return true;

                case "Minute2":
                    timeFrameOut = TimeFrame.Minute2;
                    return true;

                case "Minute20":
                    timeFrameOut = TimeFrame.Minute20;
                    return true;

                case "Minute3":
                    timeFrameOut = TimeFrame.Minute3;
                    return true;

                case "Minute30":
                    timeFrameOut = TimeFrame.Minute30;
                    return true;

                case "Minute4":
                    timeFrameOut = TimeFrame.Minute4;
                    return true;

                case "Minute45":
                    timeFrameOut = TimeFrame.Minute45;
                    return true;

                case "Minute5":
                    timeFrameOut = TimeFrame.Minute5;
                    return true;

                case "Minute6":
                    timeFrameOut = TimeFrame.Minute6;
                    return true;

                case "Minute7":
                    timeFrameOut = TimeFrame.Minute7;
                    return true;

                case "Minute8":
                    timeFrameOut = TimeFrame.Minute8;
                    return true;

                case "Minute9":
                    timeFrameOut = TimeFrame.Minute9;
                    return true;

                case "Monthly":
                    timeFrameOut = TimeFrame.Monthly;
                    return true;

                case "Weekly":
                    timeFrameOut = TimeFrame.Weekly;
                    return true;

                default:
                    timeFrameOut = null;
                    return false;
            }
        }

        /// <summary>
        /// Mainly used during optimizations
        /// </summary>
        /// <param name="timeFramevale"></param>
        /// <returns></returns>
        public static TimeFrame TimeFrameFromInteger(int timeFramevale)
        {
            switch (timeFramevale)
            {
                case 1:
                    return TimeFrame.Minute;
                case 2:
                    return TimeFrame.Minute2;
                case 3:
                    return TimeFrame.Minute3;
                case 4:
                    return TimeFrame.Minute4;
                case 5:
                    return TimeFrame.Minute5;
                case 6:
                    return TimeFrame.Minute6;
                case 7:
                    return TimeFrame.Minute7;
                case 8:
                    return TimeFrame.Minute8;
                case 9:
                    return TimeFrame.Minute9;
                case 10:
                    return TimeFrame.Minute10;
                case 11:
                    return TimeFrame.Minute15;
                case 12:
                    return TimeFrame.Minute20;
                case 13:
                    return TimeFrame.Minute30;
                case 14:
                    return TimeFrame.Minute45;
                case 15:
                    return TimeFrame.Hour;
                case 16:
                    return TimeFrame.Hour2;
                case 17:
                    return TimeFrame.Hour3;
                case 18:
                    return TimeFrame.Hour4;
                case 19:
                    return TimeFrame.Hour6;
                case 20:
                    return TimeFrame.Hour8;
                case 21:
                    return TimeFrame.Hour12;
                case 22:
                    return TimeFrame.Daily;
                case 23:
                    return TimeFrame.Day2;
                case 24:
                    return TimeFrame.Day3;
                case 25:
                    return TimeFrame.Weekly;
                case 26:
                    return TimeFrame.Monthly;
            }
            return TimeFrame.Hour;
        }
    }
}
