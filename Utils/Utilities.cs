using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CarbonFxModules.Utils
{
    public class Utilities
    {
        public static double[] FibonacciLevels = new double[] { 0.236, 0.382, 0.50, 0.618, 0.786, 1.0, 1.618, 2.618, 4.236 };
        public static bool FuzzyCompare(double p1, double p2, double fuzz)
        {
            return Math.Abs(p1 - p2) < fuzz;
        }

        public static TimeSpan ParseTimeSpan(string duration)
        {
            if (duration.Contains("m"))
            {
                return new TimeSpan(0, int.Parse(duration.Substring(0, duration.Length - 1)), 0);
            }

            if (duration.Contains("h"))
            {
                return new TimeSpan(int.Parse(duration.Substring(0, duration.Length - 1)), 0, 0);
            }

            if (duration.Contains("d"))
            {
                return new TimeSpan(int.Parse(duration.Substring(0, duration.Length - 1)), 0, 0, 0);
            }

            if (duration.Contains("w"))
            {
                return new TimeSpan(int.Parse(duration.Substring(0, duration.Length - 1)) * 7, 0, 0, 0);
            }

            if (duration.Contains("M"))
            {
                return new TimeSpan(int.Parse(duration.Substring(0, duration.Length - 1)) * 30, 0, 0, 0);
            }

            if (duration.Contains("y"))
            {
                return new TimeSpan(int.Parse(duration.Substring(0, duration.Length - 1)) * 365, 0, 0, 0);
            }

            return new TimeSpan(24, 0, 0);
        }


        
    }
}
