using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLServerHelper
{
    public static class SqlServer
    {
        public static Action<string> RunLog;

        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回object类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static string runSql(string connectionString, string sql, out object result)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                reader.Read();
                result = reader[0];
            }
            catch (SqlException e)
            {
                result = null;
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return e.Message;
            }
            connection.Close();
            return "Success";
        }
        /// <summary>
        /// 异步获取单条数据
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static async Task<object> runSqlAsync(string connectionString, string sql)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                SqlDataReader reader = await command.ExecuteReaderAsync();
                reader.Read();
                object res = reader[0];
                connection.Close();
                return res;
            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return e.Message;
            }
        }

        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回string类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static string runSql(string connectionString, string sql, out string result)
        {
            string b = runSql(connectionString, sql, out object res);

            if (b == "Success")
            {
                try
                {
                    result = res.ToString();
                }
                catch (Exception ex)
                {
                    result = null;
                    RunLog("命令：" + sql + "，错误信息：" + ex.Message);
                    return ex.Message;
                }
            }
            else result = null;
            return b;
        }

        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回int类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息，此时result=0</returns>
        public static string runSql(string connectionString, string sql, out int result)
        {
            string b = runSql(connectionString, sql, out object res);
            if (b == "Success")
            {
                try
                {
                    result = int.Parse(res.ToString());
                    return b;
                }
                catch (Exception ex)
                {
                    result = 0;
                    RunLog("命令：" + sql + "，错误信息：" + ex.Message);
                    return ex.Message;
                }
            }
            result = 0;
            return b;
        }

        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回doubel类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息，此时result=0</returns>
        public static string runSql(string connectionString, string sql, out double result)
        {
            string b = runSql(connectionString, sql, out object res);
            if (b == "Success")
            {
                try
                {
                    result = double.Parse(res.ToString());
                    return b;
                }
                catch (Exception ex)
                {
                    result = 0; 
                    RunLog("命令：" + sql + "，错误信息：" + ex.Message);
                    return ex.Message;
                }
            }
            result = 0;
            return b;
        }

        /// <summary>
        /// 获取DataTable
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回DataTable类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static string runSql(string connectionString, string sql, out DataTable result)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            result = new DataTable();
            try
            {
                connection.Open();
                adapter.Fill(result);
            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return e.Message;
            }
            connection.Close();
            return "Success";
        }


        /// <summary>
        /// 获取DataTable并装入DataSet中
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="name">DataTable命名</param>
        /// <param name="result">返回DataTable类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static string runSql(string connectionString, string sql, string name, out DataSet result)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            result = new DataSet();
            try
            {
                connection.Open();
                adapter.Fill(result, name);
            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return e.Message;
            }
            connection.Close();
            return "Success";
        }

        /// <summary>
        /// 获取List<object>
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <param name="result">返回List<object>类型</param>
        /// <returns>成功返回"Success"，失败返回错误信息</returns>
        public static string runSql(string connectionString, string sql, out List<object> result)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            result = new List<object>();
            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader[0]);
                }
            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return e.Message;
            }
            connection.Close();
            return "Success";
        }

        public static int runSql(string connectionString, DataTable dataTable, string tableName)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BatchSize = 50000;  // 每批处理行数
                    bulkCopy.ColumnMappings.Add("id", "id");
                    bulkCopy.ColumnMappings.Add("pass", "pass");
                    bulkCopy.ColumnMappings.Add("total", "total");
                    bulkCopy.ColumnMappings.Add("usedTime", "usedTime");
                    bulkCopy.WriteToServer(dataTable);  // dataTable 为 DataTable 对象
                }
                return 1; // 成功返回1
            }
            catch (Exception ex)
            {
                connection.Close();
                RunLog(ex.Message);
                return -2;
            }
        }

        /// <summary>
        /// 执行非查询sql，返回影响的行数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>执行错误返回-1</returns>
        public static int runSqlNonQuery(string connectionString, string sql)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                int res = command.ExecuteNonQuery();
                connection.Close();
                return res;

            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return -2;
            }
        }

        /// <summary>
        /// 异步执行非查询sql，返回影响的行数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="sql">sql语句</param>
        /// <returns>执行错误返回-1</returns>
        public static async Task<int> runSqlNonQueryAsync(string connectionString, string sql)
        {
            //int res = runSql(connectionString, sql);
            //return res;

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand(sql, connection);
            try
            {
                connection.Open();
                int res = await command.ExecuteNonQueryAsync();
                connection.Close();
                return res;

            }
            catch (SqlException e)
            {
                connection.Close();
                RunLog("命令：" + sql + "，错误信息：" + e.Message);
                return -1;
            }
        }


    }
}
