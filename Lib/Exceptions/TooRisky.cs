using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Exceptions
{
    public class TooRisky : Exception
    {
        public TooRisky(string message) : base(message) { }
    }
}
