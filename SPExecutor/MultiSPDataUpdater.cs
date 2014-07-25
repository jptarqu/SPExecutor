using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation.Workflow;

namespace SPExecutor
{
    public class MultiSPDataUpdater<CommandCoreClass> : IDataUpdater<CommandCoreClass> 
    {
        private SPExecutor _executor = new SPExecutor();

        public IEnumerable<SPSpecification<CommandCoreClass>> SPDefinitions { get; set; }

         
        public BulletOnRailsNET.Web.DataAccess.WindowsDataProvider SecurityProvider
        {
            get;
            set;
        }

        /// <summary>
        /// Executes a series of SPs wrapping them in one transaction. It allows sending children objects to an SP one by one.
        /// </summary>
        /// <param name="command_obj"></param>
        /// <param name="username"></param>
        public void Execute(CommandCoreClass command_obj, string username)
        {
            LinkedList<SPToExec> sps_to_exec = new LinkedList<SPToExec>();
            foreach (SPSpecification<CommandCoreClass> sp_definition in SPDefinitions)
            {
                if (sp_definition.ChildrenToSend == null)
                {
                    sps_to_exec.AddLast(new SPToExec() { SPName = sp_definition.SPName, ParameterObj = command_obj });
                }
                else
                {
                    IEnumerable<object> children = sp_definition.ChildrenToSend(command_obj);
                    foreach (var child in children)
                    {
                        sps_to_exec.AddLast(new SPToExec() { SPName = sp_definition.SPName, ParameterObj = child
                        });
                    }
                }
            }
            _executor.ExecuteInTransaction(sps_to_exec);
        }

    }
}
