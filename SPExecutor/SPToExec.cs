using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor
{
    /// <summary>
    /// Defines the name of SP to execute and the command obj 
    /// to use. Needed for defining a sequence of sps to execute over a sequence of objects
    /// </summary>
    internal class SPToExec
    {
        public string SPName { get; set; }
        public object ParameterObj { get; set; }


    }
}
