using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SPExecutor.Helpers
{
    class MappingHelper
    {
        private static Dictionary<string, PropertyInfo[]> _cached_func_return_mapper = new Dictionary<string, PropertyInfo[]>();

        public static PropertyInfo[] MapColumnsToProperties(SqlDataReader reader, object command_obj, string func_name)
        {
            var mapping_name = command_obj.GetType().FullName + "|" + func_name;
            if (_cached_func_return_mapper.ContainsKey(mapping_name))
            {
                return _cached_func_return_mapper[mapping_name];
            }
            else
            {
                PropertyInfo[] cols_prop_mapper = new PropertyInfo[reader.FieldCount];
                for (int col_idx = 0; col_idx < reader.FieldCount; col_idx++)
                {
                    var prop_name = reader.GetName(col_idx);
                    cols_prop_mapper[col_idx] = command_obj.GetType().GetProperty(prop_name);

                }
                _cached_func_return_mapper.Add(mapping_name, cols_prop_mapper);
                return cols_prop_mapper;
            }
        }

        public static PropertyInfo[] AssignDBValuesToObjectProps(string func_name, object result_obj, SqlDataReader reader, PropertyInfo[] cols_prop_mapper)
        {
            if (cols_prop_mapper == null)
            {
                cols_prop_mapper = MappingHelper.MapColumnsToProperties(reader, result_obj, func_name);
            }
            for (int col_idx = 0; col_idx < cols_prop_mapper.Length; col_idx++)
            {
                if (cols_prop_mapper[col_idx] != null)
                {
                    var value = reader.GetValue(col_idx);
                    if (value == DBNull.Value)
                        value = null; //Needed when dealing with NULLs from the db
                    cols_prop_mapper[col_idx].SetValue(result_obj, value);
                }

            }
            return cols_prop_mapper;
        }
    }
}
