using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.ContextAware
{
    class ChildCollectionUpdateDefinition<ParentClass, ChildClass> 
    {
        public Func<object, object> GetParentFunc { get; private set; }
        public Func<ParentClass, IEnumerable<ChildClass>> GetCollectionFunc { get; private set; }
        public Action<ParentClass,ChildClass>  OnBeforeUpdate { get; private set; }
        public string SqlSPName { get; private set; }

        public ChildCollectionUpdateDefinition(Func<object, object> get_parent_func
            ,Func<ParentClass, IEnumerable<ChildClass>> get_collection_func
            , string sql_sp_name
            , Action<ParentClass, ChildClass> on_before_update
            )
        {
            GetParentFunc = get_parent_func;
            GetCollectionFunc = get_collection_func;
            SqlSPName = sql_sp_name;
            OnBeforeUpdate  = on_before_update;
        }

    }
}
