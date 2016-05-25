using CarbonFxModules.Lib;
using CarbonFxModules.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonFxModules.Modules
{
    public abstract class ModuleBase
    {
        public IStrategyBase Strategy
        {
            get; internal set;
        }

        public IStrategySettings Settings
        {
            get; internal set;
        }

        // Not sure if this helps at all
        //public int Version
        //{
        //    get; set;
        //}

        string _name = null;
        public string Name
        {
            get
            {
                lock (this)
                {
                    if (_name == null)
                    {
                        _name = this.GetType().Name;
                    }
                    return _name;
                }
            }            
        }
        
    }
}
