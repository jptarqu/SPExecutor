using Foundation.ContextAwareWorkflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.Functions
{
    /// <summary>
    /// Allows populating a POCO and its chidlren collections from a set of functions
    /// </summary>
    public class GraphObjectReader<CommandCoreClass> : IEntityReader<CommandCoreClass>
    {
        LinkedList<
            ChildCollectionReadDefinition> _children_to_populate = new LinkedList<ChildCollectionReadDefinition>();
        private FunctionExecutor _executor = new FunctionExecutor();

        /// <summary>
        /// The function to be called to populate the master (root) object
        /// </summary>
        public string MasterFunctionName { get; set; }

        public GraphObjectReader() 
        {
        }
        public GraphObjectReader(string master_function_name)
        {
            MasterFunctionName = master_function_name;
        }
        public GraphObjectReader(string master_function_name, string connection_str)
        {
            MasterFunctionName = master_function_name;
            _executor = new FunctionExecutor(connection_str);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="get_parent_func">The function that when passed the master object, would return the parent of the collection</param>
        /// <param name="obj_add_action">The action that when passed the parent of the collection and the objk to add would add the obj to the parent</param>
        /// <param name="constructor_method">The function that would instantiate a new version of the child object</param>
        /// <param name="sql_func_name"></param>
        public void AddChildrenToPopulate(Func<object, object> get_parent_func, Action<object, object> obj_add_action, Func<object> constructor_method, string sql_func_name)
        {
            _children_to_populate.AddLast(new ChildCollectionReadDefinition(get_parent_func, obj_add_action, constructor_method, sql_func_name));
        }

        /// <summary>
        /// Assumes that the get_parent_func is the the same as returning thwe master object and that the child contructor method is a simple public, no param constructor.
        /// </summary>
        /// <typeparam name="ParentClass">The class of the master object</typeparam>
        /// <typeparam name="ChildClass">The class of the child object</typeparam>
        /// <param name="constructor_method">The function that would instantiate a new version of the child object</param>
        /// <param name="sql_func_name"></param>
        public void AddChildrenToPopulate<ParentClass, ChildClass>(Action<object, object> obj_add_action, string sql_func_name)
        {
            Func<object, object> get_self = (new_obj) => new_obj;
            Func<object> constructor_method = () => Activator.CreateInstance<ChildClass>(); 
            _children_to_populate.AddLast(new ChildCollectionReadDefinition(get_self, obj_add_action, constructor_method, sql_func_name));
        }
        /// <summary>
        /// Assumes that the get_parent_func is the the same as returning the master object and that the child contructor method is a simple public, no param constructor. Also,
        /// it assumes the collection_func returns the ICollection to use for adding children.
        /// </summary>
        /// <typeparam name="ParentClass"></typeparam>
        /// <typeparam name="ChildClass"></typeparam>
        /// <param name="collection_func">The function that returns the collection to use for adding children</param>
        /// <param name="sql_func_name"></param>
        public void AddChildrenToPopulate<ParentClass, ChildClass>(Func<ParentClass, ICollection<ChildClass>> collection_func, string sql_func_name)
        {
            Func<object, object> get_self = (new_obj) => new_obj;
            Action<object, object> collection_add_action = (object parent, object child) => (collection_func((ParentClass)parent)).Add((ChildClass)child);
            Func<object> constructor_method = () => Activator.CreateInstance<ChildClass>();
            _children_to_populate.AddLast(new ChildCollectionReadDefinition(get_self, collection_add_action, constructor_method, sql_func_name));
        }

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

        public void PopulateOldVersionFromNew(WorkflowContext<CommandCoreClass> context)
        {
            _executor.PopulateOldVersionFromNew(MasterFunctionName, context.OldObjectVersion, context.NewObjectVersion, _children_to_populate);
            //_executor.RunFunction(MasterFunctionName, context.NewObjectVersion, context.OldObjectVersion);

            //var master_obj = context.OldObjectVersion;
            //foreach (var child_to_populate in _children_to_populate)
            //{
            //    var parent_value = child_to_populate.GetParentFunc(master_obj);
            //    _executor.RunFunctionAsCollection(child_to_populate.SqlFuncName, parent_value, child_to_populate.ConstructorMethod, child_to_populate.ObjAddAction);
            //}
        }
    }
}
