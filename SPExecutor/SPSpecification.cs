using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor
{

    /// <summary>
    /// Defines a store procedure to be called
    /// </summary>
    /// <typeparam name="CommandCoreClass"></typeparam>
    /// <typeparam name="ChildClass"></typeparam>
    public class SPSpecification<CommandCoreClass>
    {
        public string SPName { get; set; }

        /// <summary>
        /// Use this property to specify that the children objects should be passed to the Stored procedure instead of the parent
        /// </summary>
        public Func<CommandCoreClass, IEnumerable<object>> ChildrenToSend { get; set; }
        public Func<CommandCoreClass, object> KeyForChildren { get; set; }
    }
}
