using SPExecutor.Functions;
using SPExecutor.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Reflection;

namespace SPExecutor
{
    public class FunctionExecutor
    {
        private Dictionary<string, IEnumerable<string>> _cached_func_info = new Dictionary<string, IEnumerable<string>>();
        private string _conn_str;

        public FunctionExecutor()
        {
            if (ConfigurationManager.ConnectionStrings["BulletSPDB"] != null)
                _conn_str = ConfigurationManager.ConnectionStrings["BulletSPDB"].ConnectionString;
        }
        public FunctionExecutor(string connection_str)
        {
                _conn_str = connection_str;
        }

        protected IEnumerable<string> GetSPParamTypes(string proc_name)
        {
            if (_cached_func_info.ContainsKey(proc_name))
            {
                return _cached_func_info[proc_name];
            }
            else
            {
                var sp_params = new LinkedList<string>();
                var queryString = @" select PARAMETER_NAME from INFORMATION_SCHEMA.PARAMETERS where 
SPECIFIC_NAME = @sp_name
order by ORDINAL_POSITION";

                using (SqlConnection connection =
                new SqlConnection(_conn_str))
                {
                    // Create the Command and Parameter objects.
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@sp_name", proc_name);

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        sp_params.AddLast(reader[0].ToString());
                    }
                    reader.Close();
                }
                _cached_func_info.Add(proc_name, sp_params);
                return sp_params;
            }
        }

        internal void PopulateOldVersionFromNew(string parent_function_name, object old_parent_obj,object new_parent_obj,
            IEnumerable<ChildCollectionReadDefinition> children_to_populate)
        {
            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                conn.Open();
                if (parent_function_name != null)
                {
                    this.RunFunction(parent_function_name, new_parent_obj, old_parent_obj, conn);
                }

                var master_obj = old_parent_obj;
                foreach (var child_to_populate in children_to_populate)
                {
                    var parent_value = child_to_populate.GetParentFunc(master_obj);
                    this.RunFunctionAsCollection(child_to_populate.SqlFuncName, parent_value, child_to_populate.ConstructorMethod, child_to_populate.ObjAddAction, conn, null);
                }
                conn.Close();
            }
        }

        public void RunFunctionAsCollection(string func_name, object parent_obj, Func<object> child_constructor, Action<object, object> collection_add_obj)
        {
            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                conn.Open();
                RunFunctionAsCollection(func_name, parent_obj, child_constructor, collection_add_obj, conn, null);
                conn.Close();
            }
        }

        public void RunFunctionAsCollection(string func_name, object command_obj, Func<object> child_constructor, Action<object, object> collection_add_obj,
            SqlConnection conn, SqlTransaction sql_transaction)
        {

            var sql_qry = "select * from " + func_name;

            try
            {
                var sql_sp_param_names = GetSPParamTypes(func_name);

                sql_qry += "(" + string.Join(",", sql_sp_param_names) + ")";

                // 1. create a command object identifying
                // the stored procedure
                SqlCommand cmd;

                if (sql_transaction == null)
                {
                    cmd = new SqlCommand(
                    sql_qry, conn);
                }
                else
                {
                    cmd = new SqlCommand(
                    sql_qry, conn, sql_transaction);
                }

                string normal_name = "";
                foreach (var param_name in sql_sp_param_names)
                {
                    normal_name = param_name.Replace("@", "");
                    cmd.Parameters.AddWithValue(param_name, GetValueOfProp(command_obj, normal_name) ?? DBNull.Value);
                }

                SqlDataReader reader = cmd.ExecuteReader();
                PropertyInfo[] cols_prop_mapper = null;
                while (reader.Read())
                {
                    var result_obj = child_constructor();
                    cols_prop_mapper = MappingHelper.AssignDBValuesToObjectProps(func_name, result_obj, reader, cols_prop_mapper);
                    collection_add_obj(command_obj, result_obj);
                }
                reader.Close();


            }
            catch (Exception e)
            {
                throw new Exception("\nError using object as params: " + command_obj.ToString() + " and sql: " + sql_qry + " " + e.Message);
            }
        }


        public void RunFunction(string func_name, object command_obj, object result_obj)
        {
            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                conn.Open();
                RunFunction(func_name, command_obj, result_obj, conn, null);
                conn.Close();
            }
        }

        public void RunFunction(string func_name, object command_obj, object result_obj,  SqlConnection conn, SqlTransaction sql_transaction = null)
        {

            var sql_qry = "select top 1 * from " + func_name;

            try
            {
                var sql_sp_param_names = GetSPParamTypes(func_name);

                sql_qry += "(" + string.Join(",", sql_sp_param_names) + ")";

                // 1. create a command object identifying
                // the stored procedure
                SqlCommand cmd;
                
                if (sql_transaction == null)
                {
                    cmd = new SqlCommand(
                    sql_qry, conn);
                }
                else
                {
                    cmd  = new SqlCommand(
                    sql_qry, conn, sql_transaction);
                }

                string normal_name = "";
                foreach (var param_name in sql_sp_param_names)
                {
                    normal_name = param_name.Replace("@", "");
                    cmd.Parameters.AddWithValue(param_name, GetValueOfProp(command_obj, normal_name) ?? DBNull.Value);
                }

                SqlDataReader reader = cmd.ExecuteReader();
                PropertyInfo[] cols_prop_mapper = null;
                while (reader.Read())
                {
                    cols_prop_mapper = MappingHelper.AssignDBValuesToObjectProps(func_name, result_obj, reader, cols_prop_mapper);
                }
                reader.Close();


            }
            catch (Exception e)
            {
                throw new Exception("\nError using object as params: " + command_obj.ToString() + " and sql: " + sql_qry + " " + e.Message);
            }
        }

        

        

        /// <summary>
        /// Gets the value of a property of an object by name
        /// </summary>
        /// <param name="command_obj"></param>
        /// <param name="normal_name"></param>
        /// <returns></returns>
        private object GetValueOfProp(object command_obj, string normal_name)
        {
            object value = null;
            var prop = command_obj.GetType().GetProperty(normal_name);
            if (prop != null)
            {
                value = prop.GetValue(command_obj);
            }
            return value;
        }
    }
}
