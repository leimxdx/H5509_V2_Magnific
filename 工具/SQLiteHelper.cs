using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using static GTN.mc;
using System.IO;

namespace H5509_V2
{
    public class SQLiteHelper
    {

        public static string ConnString = $"Data Source={Directory.GetCurrentDirectory() + "\\配置\\SystemData"};Polling=true;FailIfMissing=false";



        public string GetConnString(int mdb)
        {
            switch (mdb)
            {
                case 0: return ConnString;
                default: return "";
            }
        }

        /// <summary>
        /// 执行insert、update、delete类型的SQL语句
        /// </summary>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="paramArray">参数数组</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string constring, string cmdText, SQLiteParameter[] paramArray = null)
        {
            SQLiteConnection conn = new SQLiteConnection(constring);
            SQLiteCommand cmd = new SQLiteCommand(cmdText, conn);
            if (paramArray != null)
            {
                cmd.Parameters.AddRange(paramArray);
            }
            try
            {
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string errorMsg = $"{DateTime.Now}  : 执行 public static int ExecuteNonQuery(string cmdText, SqlParameter[] paramArray = null)方法发生异常：{ex.Message}";
                //在这个地方写入日志...

                throw new Exception("执行public static int ExecuteNonQuery(string cmdText, SqlParameter[] paramArray = null)方法发生异常：" + ex.Message);
            }
            finally   //以上不管是否发生异常，都会执行的代码
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 执行insert、update、delete类型的SQL语句
        /// </summary>
        /// <param name="cmdText">SQL语句或存储过程名称</param>
        /// <param name="paramArray">参数数组</param>
        /// <returns>受影响的行数</returns>
        public static int ExecuteNonQuery(string cmdText, SQLiteParameter[] paramArray = null)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = new SQLiteCommand(cmdText, conn);
            if (paramArray != null)
            {
                cmd.Parameters.AddRange(paramArray);
            }
            try
            {
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                string errorMsg = $"{DateTime.Now}  : 执行 public static int ExecuteNonQuery(string cmdText, SqlParameter[] paramArray = null)方法发生异常：{ex.Message}";
                //在这个地方写入日志...

                throw new Exception("执行public static int ExecuteNonQuery(string cmdText, SqlParameter[] paramArray = null)方法发生异常：" + ex.Message);
            }
            finally   //以上不管是否发生异常，都会执行的代码
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 返回单一结果的查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string cmdText, SQLiteParameter[] paramArray = null)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = new SQLiteCommand(cmdText, conn);
            if (paramArray != null)
            {
                cmd.Parameters.AddRange(paramArray);
            }
            try
            {
                conn.Open();
                return cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                //在这个地方写入日志...

                throw new Exception("执行 public object ExecuteScalar(string cmdText, SqlParameter[] paramArray = null方法发生异常：" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 执行返回一个只读结果集的查询
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static SQLiteDataReader ExecuteReader(string cmdText, SQLiteParameter[] paramArray = null)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = new SQLiteCommand(cmdText, conn);
            if (paramArray != null)
            {
                cmd.Parameters.AddRange(paramArray);
            }
            try
            {
                conn.Open();
                return cmd.ExecuteReader(CommandBehavior.CloseConnection); //必须添加这个枚举
            }
            catch (Exception ex)
            {
                //在这个地方写入日志...

                throw new Exception("执行 public object SqlDataReader(string cmdText, SqlParameter[] paramArray = null)方法发生异常：" + ex.Message);
            }
        }
        /// <summary>
        /// 返回包含一张数据表的数据集的查询
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <param name="tableName">数据表的名称</param>
        /// <returns></returns>
        public static DataSet GetDataSet(string sql, SQLiteParameter[] paramArray = null, string tableName = null)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            if (paramArray != null)
            {
                cmd.Parameters.AddRange(paramArray);
            }
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
            try
            {
                conn.Open();
                if (tableName == null)
                    da.Fill(ds);
                else
                    da.Fill(ds, tableName);
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("执行 public DataSet GetDataSet(string sql, string tableName = null)方法发生异常：" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }
        /// <summary>
        /// 执行查询，返回一个或多个表的DataSet
        /// </summary>
        /// <param name="dicTableAndSql"></param>
        /// <returns></returns>
        public static DataSet GetDataSet(Dictionary<string, string> dicTableAndSql)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = conn;
            SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
            try
            {
                conn.Open();
                foreach (string tbName in dicTableAndSql.Keys)
                {
                    cmd.CommandText = dicTableAndSql[tbName];
                    da.Fill(ds, tbName);
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("执行 public DataSet GetDataSet(Dictionary<string,string> dicTableAndSql)方法发生异常：" + ex.Message);
            }
            finally
            {
                conn.Close();
            }
        }

        /// <summary>
        /// 查询结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static SQLiteDataReader GetDataReader(string sql)
        {
            SQLiteConnection conn = new SQLiteConnection(ConnString);
            SQLiteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;  // = "select * from 表名";  mdb文件里的表名
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();  //打开数据库连接
                }
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                if (conn.State == ConnectionState.Open) conn.Close();
                //throw new Exception("查询结果集异常！详细信息：" + ex.ToString());
                throw new Exception(ex.Message);
            }
        }
    }
}
