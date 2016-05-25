using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules
{
    /// <summary>
    ///  Meant to allow the module to report it's status if asked
    /// </summary>
    public interface IReportStatus
    {
        string GetStatus();
    }
}
