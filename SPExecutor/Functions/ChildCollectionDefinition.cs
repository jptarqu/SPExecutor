using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.Functions
{
    internal class ChildCollectionReadDefinition
    {
        public Func<object, object> GetParentFunc { get; private set; }
        public Action<object, object> ObjAddAction { get; private set; }
        public string SqlFuncName { get; private set; }
        public ChildCollectionReadDefinition(Func<object, object> get_parent_func
            , Action<object, object> obj_add_action
            , Func<object> constructor_method
            , string sql_func_name
            )
        {
            GetParentFunc = get_parent_func;
            ObjAddAction = obj_add_action;
            SqlFuncName = sql_func_name;
            ConstructorMethod = constructor_method;
        }

        public Func<object> ConstructorMethod { get; set; }
    }
}
