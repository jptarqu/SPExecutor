using Foundation.ContextAwareWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor
{
    public class FunctionDataReader<CommandCoreClass> : IEntityReader<CommandCoreClass>
    {

        private FunctionExecutor _executor = new FunctionExecutor();
        public string FunctionName { get; set; }


        public bool ExecuteBooleanQuery(string query_name, WorkflowContext<CommandCoreClass> context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommandCoreClass> ExecuteQuery(string query_name, WorkflowContext<CommandCoreClass> context)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<CommandCoreClass> ExecuteQuery(WorkflowContext<CommandCoreClass> context)
        {
            throw new NotImplementedException();
        }

        public object ExecuteSingleQuery(string query_name, WorkflowContext<CommandCoreClass> context)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Using the values of the New object version as paramters, call the SQL function to populate the Old Version.
        /// </summary>
        /// <param name="context"></param>
        public void PopulateOldVersionFromNew(WorkflowContext<CommandCoreClass> context)
        {
            _executor.RunFunction(FunctionName, context.NewObjectVersion , context.OldObjectVersion);
        }
    }
}
