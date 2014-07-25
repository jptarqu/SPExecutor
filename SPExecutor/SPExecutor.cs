using SPExecutor.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SPExecutor
{
    /// <summary>
    /// Allows execution of SQL stored procedures wihout using any ORM, just plain ADO.net
    /// </summary>
    public class SPExecutor
    {
        private Dictionary<string, Dictionary<string, int>> _cached_sp_info = new Dictionary<string, Dictionary<string, int>>();

        private string _conn_str;

        public SPExecutor()
        {
            if (ConfigurationManager.ConnectionStrings["BulletSPDB"] != null)
                _conn_str = ConfigurationManager.ConnectionStrings["BulletSPDB"].ConnectionString;
        }
        public SPExecutor(string connection_str)
        {
                _conn_str = connection_str;
        }
        /// <summary>
        /// Execute a SQL stored procedure by name. The procedure must live in the db that the connection string points at.
        /// </summary>
        /// <param name="proc_name">Nmae of the stored procedure</param>
        /// <returns>Any text outputed to the console by the store procedure</returns>
        public string RunStoredProc(string proc_name)
        {
            SqlConnection conn = null;
            SqlDataReader rdr = null;

            var results = "";

            try
            {
                // create and open a connection object
                conn = new
                    SqlConnection(_conn_str);
                conn.Open();
                conn.InfoMessage += new SqlInfoMessageEventHandler((obj, msg) => results += msg.Message);

                // 1. create a command object identifying
                // the stored procedure
                SqlCommand cmd = new SqlCommand(
                    proc_name, conn);

                // 2. set the command object so it knows
                // to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                // execute the command
                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                results += e.Message;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (rdr != null)
                {
                    rdr.Close();
                }
            }
            return results;
        }

        /// <summary>
        /// Returns information about the parameters used by a stored procedure. 
        /// The procedure must live in the db that the connection string points at.
        /// The parameter info is cached, so it does nto have to be retrieved every time.
        /// </summary>
        /// <param name="proc_name">Name of the stored procedure</param>
        /// <returns>A dictionary with the name of the param as the key and its type as the value</returns>
        protected Dictionary<string, int> GetSPParamTypes(string proc_name)
        {
            if (_cached_sp_info.ContainsKey(proc_name))
            {
                return _cached_sp_info[proc_name];
            }
            else
            {
                var sp_params = new Dictionary<string, int>();
                var queryString = @"select pr.name, p.name, p.system_type_id
    from sys.procedures pr 
    inner join sys.parameters p on pr.object_id = p.object_id
    where pr.name = @sp_name";

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
                        sp_params.Add(reader[1].ToString(), (byte)reader[2]);
                    }
                    reader.Close();
                }
                _cached_sp_info.Add(proc_name, sp_params);
                return sp_params;
            }
        }



        /// <summary>
        /// Executes a sequence of SPs using the command parameter passed in the sps_to_execute sequence. All of them
        /// are included in a single transaction.
        /// </summary>
        /// <param name="sps_to_execute"></param>
        internal void ExecuteInTransaction(IEnumerable<SPToExec> sps_to_execute)
        {

            StringBuilder results = new StringBuilder();
            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                conn.Open();
                conn.InfoMessage += new SqlInfoMessageEventHandler((obj, msg) => results.Append(msg.Message));

                using (SqlTransaction tr = conn.BeginTransaction())
                {

                    foreach (var sp_to_execute in sps_to_execute)
                    {
                       
                       results.Append(RunStoredProc(sp_to_execute.SPName, sp_to_execute.ParameterObj, conn, tr));
                    }

                    if (results.Length == 0)
                        tr.Commit();
                    else
                    {
                        tr.Rollback();
                        throw new Exception("ExecuteInTransaction Failed: " + results.ToString());
                    }
                }

            }
        }

        public string RunStoredProcs(string proc_name, object command_obj)
        {
            StringBuilder results = new StringBuilder();


            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                //try
                //{
                conn.Open();
                conn.InfoMessage += new SqlInfoMessageEventHandler((obj, msg) => results.Append(msg.Message));

                using (SqlTransaction tr = conn.BeginTransaction())
                {

                    results.Append(RunStoredProc(proc_name, command_obj, conn, tr));

                    if (results.Length == 0)
                        tr.Commit();
                }

                //} let the higher layer catch the error
                //catch (Exception e)
                //{
                //    results.Append(e.Message);
                //}
            }
            return results.ToString();
        }

        /// <summary>
        /// Executes the stored procedure, mapping the parameters to the command_obj's properties. If a result is returned by the 
        /// stored procedure, the result will be mapped back to the command_obj's properties.
        /// </summary>
        /// <param name="proc_name"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        public string RunStoredProc(string proc_name, object command_obj)
        {
            StringBuilder results = new StringBuilder();


            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                //try
                //{
                conn.Open();
                conn.InfoMessage += new SqlInfoMessageEventHandler((obj, msg) => results.Append(msg.Message));

                using (SqlTransaction tr = conn.BeginTransaction())
                {

                    results.Append(RunStoredProc(proc_name, command_obj, conn, tr));

                    if (results.Length == 0)
                        tr.Commit();
                    else
                        tr.Rollback();
                }

                //} let the higher layer catch the error
                //catch (Exception e)
                //{
                //    results.Append(e.Message);
                //}
            }
            return results.ToString();
        }

        public string RunStoredProc(string proc_name, IEnumerable<NameValueCollection> records)
        {
            StringBuilder results = new StringBuilder();


            using (SqlConnection conn =
            new SqlConnection(_conn_str))
            {
                try
                {
                    conn.Open();
                    conn.InfoMessage += new SqlInfoMessageEventHandler((obj, msg) => results.Append(msg.Message));

                    using (SqlTransaction tr = conn.BeginTransaction())
                    {
                        foreach (var record in records)
                        {
                            results.Append(RunStoredProc(proc_name, record, conn, tr));
                        }
                        if (results.Length == 0)
                            tr.Commit();
                    }

                }
                catch (Exception e)
                {
                    results.Append(e.Message);
                }
            }
            return results.ToString();
        }

        /// <summary>
        /// Execute a SQL stored procedure by name and passes parameters to it. The parameters are passed via a NameValueCollection.
        /// The procedure must live in the db that the connection string points at.
        /// </summary>
        /// <param name="proc_name">Name of the stored procedure</param>
        /// <param name="sp_params">The collection of parameter names (keys) and their text values.</param>
        /// <returns>Any text outputed to the console by the store procedure</returns>
        public string RunStoredProc(string proc_name, NameValueCollection sp_params)
        {

            return RunStoredProc(proc_name, new[] { sp_params });
        }

        public string RunStoredProc(string proc_name, NameValueCollection sp_params, SqlConnection conn, SqlTransaction sql_transaction)
        {

            var results = "";

            try
            {

                // 1. create a command object identifying
                // the stored procedure
                SqlCommand cmd = new SqlCommand(
                    proc_name, conn, sql_transaction);

                // 2. set the command object so it knows
                // to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                var sql_sp_params = GetSPParamTypes(proc_name);
                string normal_name = "";
                foreach (var param_name in sql_sp_params.Keys)
                {
                    normal_name = param_name.Replace("@", "");
                    cmd.Parameters.AddWithValue(param_name, sp_params[normal_name]);

                }

                // execute the command
                cmd.ExecuteNonQuery();


            }
            catch (Exception e)
            {
                results += "\nError using params: " + string.Join("\t", sp_params.AllKeys.Select(key => sp_params[key])) + " " + e.Message;
            }

            return results;
        }

        /// <summary>
        /// Runs a SP. It uses the command_obj to lookup values for the parameters, if a parameter is not found there, the properties 
        /// from parent_key_obj will be used. Useful when running SPs that depend on values from parent objects
        /// </summary>
        /// <param name="proc_name"></param>
        /// <param name="parent_key_obj"></param>
        /// <param name="command_obj"></param>
        /// <param name="conn"></param>
        /// <param name="sql_transaction"></param>
        /// <returns></returns>
        public string RunStoredProc(string proc_name, object parent_obj, object command_obj, SqlConnection conn, SqlTransaction sql_transaction)
        {

            var results = "";

            try
            {

                SqlCommand cmd = new SqlCommand(
                    proc_name, conn, sql_transaction);

                // 2. set the command object so it knows
                // to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                var sql_sp_params = GetSPParamTypes(proc_name);
                string normal_name = "";
                foreach (var param_name in sql_sp_params.Keys)
                {
                    normal_name = param_name.Replace("@", "");
                    //Pupolate Prent's values first
                    //cmd.Parameters.AddWithValue(param_name, GetValueOfProp(command_obj, normal_name) ?? DBNull.Value);
                    //Then Child value
                    cmd.Parameters.AddWithValue(param_name, GetValueOfProp(command_obj, normal_name) ?? DBNull.Value);
                }

                // execute the command and map any results back to the command object
                SqlDataReader reader = cmd.ExecuteReader();
                PropertyInfo[] cols_prop_mapper = null;
                if (reader.Read())
                {
                    MappingHelper.AssignDBValuesToObjectProps(proc_name, command_obj, reader, cols_prop_mapper);
                }
                reader.Close();


            }
            catch (Exception e)
            {
                results += "\nError using object as params: " + command_obj.ToString() + " " + e.Message;
            }

            return results;
        }

        /// <summary>
        /// Executes the stored procedure, mapping the parameters to the command_obj's properties. If a result is returned by the 
        /// stored procedure, the first row of the first result will be mapped back to the command_obj's properties.
        /// </summary>
        /// <param name="proc_name"></param>
        /// <param name="command_obj"></param>
        /// <param name="conn"></param>
        /// <param name="sql_transaction"></param>
        /// <returns></returns>
        public string RunStoredProc(string proc_name, object command_obj, SqlConnection conn, SqlTransaction sql_transaction)
        {

            var results = "";

            try
            {

                // 1. create a command object identifying
                // the stored procedure
                SqlCommand cmd = new SqlCommand(
                    proc_name, conn, sql_transaction);

                // 2. set the command object so it knows
                // to execute a stored procedure
                cmd.CommandType = CommandType.StoredProcedure;

                var sql_sp_params = GetSPParamTypes(proc_name);
                string normal_name = "";
                foreach (var param_name in sql_sp_params.Keys)
                {
                    normal_name = param_name.Replace("@", "");
                    //if (
                    cmd.Parameters.AddWithValue(param_name, GetValueOfProp(command_obj, normal_name) ?? DBNull.Value);
                }

                // execute the command and map any results back to the command object
                SqlDataReader reader = cmd.ExecuteReader();
                PropertyInfo[] cols_prop_mapper = null;
                if (reader.Read())
                {
                    MappingHelper.AssignDBValuesToObjectProps(proc_name, command_obj, reader, cols_prop_mapper);
                }
                reader.Close();


            }
            catch (Exception e)
            {
                results += "\nError using object as params: " + command_obj.ToString() + " " + e.Message;
            }

            return results;
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
