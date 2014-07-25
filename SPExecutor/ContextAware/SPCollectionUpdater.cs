using Foundation.ContextAwareWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.ContextAware
{
    public class SPCollectionUpdater<CommandCoreClass> : IDataUpdater<CommandCoreClass>
    {
        private SPExecutor _executor = new SPExecutor();

        LinkedList<
            ChildCollectionUpdateDefinition<object, object>> _children_to_update_def = new LinkedList<ChildCollectionUpdateDefinition<object, object>>();
        private string _master_sp_to_exec;

        public SPCollectionUpdater(string master_sp_name)
        {
            _master_sp_to_exec = master_sp_name;
        }
        public SPCollectionUpdater(string master_sp_name, string conn_str)
        {
            _executor = new SPExecutor(conn_str);
            _master_sp_to_exec = master_sp_name;
        }

        public void AddChildrenToUpdate
       (Func<object, object> get_parent_func
            ,Func<object, IEnumerable<object>> get_collection_func
            , string sql_sp_name
            , Action<object, object> on_before_update = null)
        {
            _children_to_update_def.AddLast(
                new ChildCollectionUpdateDefinition<object, object>(get_parent_func, get_collection_func, sql_sp_name, on_before_update));
        }

        public void AddChildrenToUpdate
       (Func<object, IEnumerable<object>> get_collection_func
            , string sql_sp_name
            , Action<object, object> on_before_update = null)
        {
            Func<object, object> get_self = (new_obj) => new_obj;
            _children_to_update_def.AddLast(
                new ChildCollectionUpdateDefinition<object, object>(get_self, get_collection_func, sql_sp_name, on_before_update));
        }

        public string SPName { get; set; }
        
        public void Execute(WorkflowContext<CommandCoreClass> context)
        {
            var master_obj = context.NewObjectVersion;

            LinkedList<SPToExec> sps_to_exec = new LinkedList<SPToExec>();
            AddMasterSP(master_obj, sps_to_exec);

            foreach (var child_update_def in _children_to_update_def)
            {
                var parent_value = child_update_def.GetParentFunc(master_obj);
                IEnumerable<object> children = child_update_def.GetCollectionFunc(parent_value);
                foreach (var child in children)
                {
                    AddChildSP(sps_to_exec, child_update_def, parent_value, child);
                }
            }
            _executor.ExecuteInTransaction(sps_to_exec);
        }

        private static void AddChildSP(LinkedList<SPToExec> sps_to_exec, ChildCollectionUpdateDefinition<object, object> child_update_def, object parent_value, object child)
        {
            //Execute any logic to update the child before sending it to the SP
            if (child_update_def.OnBeforeUpdate != null)
                child_update_def.OnBeforeUpdate(parent_value, child);

            sps_to_exec.AddLast(new SPToExec()
            {
                SPName = child_update_def.SqlSPName,
                ParameterObj = child
            });
        }

        private void AddMasterSP(CommandCoreClass master_obj, LinkedList<SPToExec> sps_to_exec)
        {
            if (_master_sp_to_exec != null)
            {
                sps_to_exec.AddLast(new SPToExec()
                {
                    SPName = _master_sp_to_exec,
                    ParameterObj = master_obj
                });
            }
        }

    }
}
