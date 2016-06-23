using System;
using System.Linq;
using cAlgo.API.Internals;
using CarbonFxModules.Lib;
using System.Collections.Generic;

namespace CarbonFxModules.Utils
{
    public class CandlePatterns
    {
        MarketSeries _series;
        int _seriesIdx;

        /// <summary>
        /// Wraps the MarketSeries and provides nice methods for testing candle patterns.
        /// </summary>
        /// <param name="series">Series.</param>
        public CandlePatterns(MarketSeries series)
        {
            _series = series;
            _seriesIdx = _series.Close.Count - 1;
        }


        /// <summary>
        /// Refresh the series idx
        /// </summary>
        public void Refresh()
        {
            _seriesIdx = _series.Close.Count - 1;
        }

        /// <summary>
        /// Determines whether this instance is up candle the specified offset.
        /// </summary>
        /// <returns><c>true</c> if this instance is up candle the specified offset; otherwise, <c>false</c>.</returns>
        /// <param name="offset">Offset.</param>
        public bool IsUpCandle(int offset = 0)
        {
            int cnt = _seriesIdx - offset;
            return _series.Close[cnt] > _series.Open[cnt - 1];
        }

        /// <summary>
        /// Bullishs the engulfing.
        /// </summary>
        /// <returns><c>true</c>, if engulfing was bullished, <c>false</c> otherwise.</returns>
        /// <param name="offset">Offset.</param>
        public bool BullishEngulfing(int offset = 0)
        {
            int cnt = _seriesIdx - offset;
            bool lastIsUp = _series.Close[cnt] > _series.Open[cnt - 1];
            bool previousIsDown = _series.Close[cnt - 1] < _series.Open[cnt - 2];

            double heightLatest = _series.High[cnt] - _series.Low[cnt];
            double heightPrevious = _series.High[cnt - 1] - _series.Low[cnt - 1];

            double avg = 0;
            for (int k = 1; k < 4; k++)
            {
                avg += _series.High[cnt - k] - _series.Low[cnt - k];
            }
            avg /= 3;

            return lastIsUp &&
                previousIsDown &&
                heightLatest > heightPrevious &&
                heightLatest > avg;
        }

        /// <summary>
        /// Bearishs the engulfing.
        /// </summary>
        /// <returns><c>true</c>, if engulfing was bearished, <c>false</c> otherwise.</returns>
        /// <param name="offset">Offset.</param>
        public bool BearishEngulfing(int offset = 0)
        {
            int cnt = _seriesIdx - offset;
            bool lastIsDown = _series.Close[cnt] < _series.Open[cnt - 1];
            bool previousIsUp = _series.Close[cnt - 1] > _series.Open[cnt - 2];

            double heightLatest = _series.High[cnt] - _series.Low[cnt];
            double heightPrevious = _series.High[cnt - 1] - _series.Low[cnt - 1];

            double avg = 0;
            for (int k = 1; k < 4; k++)
            {
                avg += _series.High[cnt - k] - _series.Low[cnt - k];
            }
            avg /= 3;

            return lastIsDown &&
                previousIsUp &&
                heightLatest > heightPrevious &&
                heightLatest > avg;
        }

        /// <summary>
        /// Determines whether this instance is higher high the specified offset.
        /// </summary>
        /// <returns><c>true</c> if this instance is higher high the specified offset; otherwise, <c>false</c>.</returns>
        /// <param name="offset">Offset.</param>
        public bool IsHigherHigh(int offset = 0)
        {
            return _series.High[_seriesIdx - offset] >= _series.High[_seriesIdx - offset - 1];
        }

        /// <summary>
        /// Determines whether this instance is lower low the specified offset.
        /// </summary>
        /// <returns><c>true</c> if this instance is lower low the specified offset; otherwise, <c>false</c>.</returns>
        /// <param name="offset">Offset.</param>
        public bool IsLowerLow(int offset = 0)
        {
            return _series.Low[_seriesIdx - offset] <= _series.Low[_seriesIdx - offset - 1];
        }

        /// <summary>
        /// TODO:  NOT COMPLETED YET
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        public bool IsHugeCandle(int offset = 0, int lookback = 10)
        {
            int cnt = _series.High.Count - 1;
            double[,] diff = new double[lookback, 2];

            for (int i = 0; i < lookback; i++)
            {
                diff[i, 0] = i;
                diff[i, 1] = _series.High[cnt - offset - i] - _series.Low[cnt - offset - i];
            }

            return false;
            // Sort, find talest and see if the current candle compares to the tallest oines
        }


        /// <summary>
        /// Looks back and determines the percentage the price is currently at according to last candles
        /// > 100% or < 0%  can occur if offset is used
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        public double GetPriceLocation(int offset = 0, int lookback = 10)
        {
            double currentPrice = _series.Close.LastValue;
            double max = 0;
            double min = double.MaxValue;
            int idx = _series.High.Count - 1;
            for (int i = 0; i < lookback; i++)
            {
                max = Math.Max(max, _series.High[idx - i]);
                min = Math.Min(min, _series.Low[idx - i]);
            }

            double range = max - min;
            double curLoc = max - currentPrice;
            double percent = (range - curLoc) / range;
            return percent;
        }


        /// <summary>
        /// Finds the Support or resistance levels based on candle high-lows.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <returns></returns>
        public IEnumerable<double> GetScoredLevels(SRLevels type, int offset = 0, int lookback = 10, double fuzz = 0.0001)
        {
            double currentPrice = _series.Close.LastValue;
            double lastHigh = type == SRLevels.Resistance ? double.MinValue : double.MaxValue;
            double lastLow = type == SRLevels.Resistance ? double.MinValue : double.MaxValue;

            List<double> firstMove = new List<double>();
            List<double> secondMove = new List<double>();

            int idx = _series.High.Count - 1;
            for (int i = 0; i < lookback; i++)
            {
                double high = _series.High[idx - i];
                double low = _series.Low[idx - i];

                if (high > lastHigh)
                {
                    firstMove.Add(high);
                    lastHigh = high;
                }
                else
                {
                    secondMove.Add(high);
                }

                if (low < lastLow)
                {
                    firstMove.Add(high);
                    lastLow = low;
                }
                else
                {
                    secondMove.Add(low);
                }
            }

            // Filter levels by SR
            return ScorePriceLevels(firstMove, secondMove, fuzz)
                .Where(level =>
                    type == SRLevels.Resistance
                    ? level > currentPrice
                    : level < currentPrice
                );
        }


        /// <summary>
        /// Returns the slope angle based on Pips / Bars.  Be sure to pass the correct PipSize
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <param name="pipSize"></param>
        /// <returns></returns>
        public double GetAngleAtMedian(int offset = 0, int lookback = 100, double pipSize = 0.0001)
        {
            int end = _series.High.Count - offset - 1;
            int start = end - lookback;

            double endPrice = _series.Median[end];
            double startPrice = _series.Median[start];

            double diff = (endPrice - startPrice) / pipSize;
            double degree = Math.Atan2(diff, lookback) * (180 / Math.PI);
            return degree;
        }

        /// <summary>
        /// Calculates the slope between the start point and the high
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <param name="pipSize"></param>
        /// <returns></returns>
        public double GetAngleToMax(int offset = 0, int lookback = 100, double pipSize = 0.0001)
        {
            int end = _series.High.Count - offset - 1;
            int start = end - lookback;

            double startPrice = _series.High[start];
            double max = 0.0;

            while (start <= end)
            {
                max = Math.Max(max, _series.High[start]);
                start++;
            }

            double diff = (max - startPrice) / pipSize;
            double degree = Math.Atan2(diff, lookback) * (180 / Math.PI);
            return degree;
        }

        /// <summary>
        /// Calculates the slope between the start point and the low
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lookback"></param>
        /// <param name="pipSize"></param>
        /// <returns></returns>
        public double GetAngleToMin(int offset = 0, int lookback = 100, double pipSize = 0.0001)
        {
            int end = _series.High.Count - offset - 1;
            int start = end - lookback;

            double startPrice = _series.Low[start];
            double min = double.MaxValue;

            while (start <= end)
            {
                min = Math.Min(min, _series.Low[start]);
                start++;
            }

            double diff = (min - startPrice) / pipSize;
            double degree = Math.Atan2(diff, lookback) * (180 / Math.PI);
            return degree;
        }

        /// <summary>
        /// Scores scores each set doing an AND check
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        public IEnumerable<double> ScorePriceLevels(IEnumerable<double> a1, IEnumerable<double> a2, double fuzz = 0.0001)
        {
            Dictionary<double, double> scoredLevels = new Dictionary<double, double>();
            foreach (var hh in a1)
            {
                if (scoredLevels.ContainsKey(hh))
                {
                    scoredLevels[hh] += 0.25;
                }
                else
                {
                    scoredLevels.Add(hh, 0.75);
                }

                foreach (var lh in a2)
                {
                    if (scoredLevels.ContainsKey(lh))
                    {
                        scoredLevels[lh] += 1.0;
                    }
                    else
                    {
                        scoredLevels.Add(lh, 0.50); // lh level scored less
                    }
                }
            }
            // Sort by rating, then discard similar levels only returning the strongest to weakest;
            KeyValuePair<double, double> prevLevel = new KeyValuePair<double, double>(0, 0);
            List<KeyValuePair<double, double>> weakLevels = new List<KeyValuePair<double, double>>();
            return scoredLevels.OrderByDescending(k => k.Key).Where((k) =>
            {
                if (Math.Abs(k.Key - prevLevel.Key) > fuzz)
                {
                    prevLevel = k;
                    return true;
                }
                else
                {
                    if (k.Value > prevLevel.Value)
                    {
                        weakLevels.Add(prevLevel);
                        prevLevel = k;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            })
            .Where((k) =>
            {
                // Filter out weak levels
                return weakLevels.Contains(k) == false;
            }).OrderByDescending(k => k.Value).Select(k => k.Key);
        }
    }
}

