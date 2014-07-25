using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation.Workflow;

namespace SPExecutor
{
    public class SPDataUpdater<CommandCoreClass> : IDataUpdater<CommandCoreClass>
    {
        private SPExecutor _executor = new SPExecutor();

        public string SPName { get; set; }

        public BulletOnRailsNET.Web.DataAccess.WindowsDataProvider SecurityProvider
        {
            get;
            set;
        }

        public void Execute(CommandCoreClass command_obj, string username)
        {
            _executor.RunStoredProc(SPName, command_obj);
        }
    }
}
