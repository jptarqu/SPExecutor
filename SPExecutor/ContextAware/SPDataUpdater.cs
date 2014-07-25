using Foundation.ContextAwareWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.ContextAware
{
    public class SPDataUpdater<CommandCoreClass> : IDataUpdater<CommandCoreClass>
    {
        private SPExecutor _executor = new SPExecutor();

        public string SPName { get; set; }

        public SPDataUpdater()
        {
        }
        public SPDataUpdater(string connection_str)
        {
            _executor = new SPExecutor(connection_str) ;
        }

        public void Execute(WorkflowContext<CommandCoreClass> context)
        {
            string results = _executor.RunStoredProc(SPName, context.NewObjectVersion);
            if (!string.IsNullOrEmpty(results))
            {
                throw new Exception("The Stored Procedure returned errors: " + results);
            }
        }
    }
}
