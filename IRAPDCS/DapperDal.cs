using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace IRAPDCS
{
    public class DapperDal
    {
        string connectionString = "";
        public DapperDal(string conStr)
        {
            connectionString = conStr;
        }

        //public dynamic Query(string sql, object args = null, CommandType commandType = CommandType.Text)
        //{
        //    dynamic result;
        //    using (IDbConnection conn = new SqlConnection(connectionString))
        //    {
        //        result  = conn.Query(sql, args, commandType: commandType).FirstOrDefault();
        //    }
        //    return result;
        //}


        /// <summary> 执行SQL命令，并返回查询结果。
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="args">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回SQL语句的查询结果。</returns>
        public List<dynamic> Query(string sql, object args = null, CommandType commandType = CommandType.Text)
        {
            List<dynamic> result;
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                result = conn.Query(sql, args, commandType: commandType).ToList();
            }
            return result;
        }


        /// <summary>
        /// 执行SQL命令，并返回查询结果。
        /// </summary>
        /// <typeparam name="T">查询结果泛型参数</typeparam>
        /// <param name="sql">SQL命令</param>
        /// <param name="args">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回SQL语句的查询结果。</returns>
        public List<T> Query<T>(string sql, object args = null, CommandType commandType = CommandType.Text)
        {
            List<T> result;
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                result = conn.Query<T>(sql, args, commandType: commandType).ToList();
            }
            return result;
        }


        /// <summary>
        /// 执行SQL命令，并返回多个查询结果。
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="args">命令参数</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回SQL语句的查询结果。</returns>
        public List<List<dynamic>> QueryMultiple(string sql, object args = null, CommandType commandType = CommandType.Text)
        {

            List<List<dynamic>> result = new List<List<dynamic>>();
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                var temp = conn.QueryMultiple(sql, args, commandType: commandType);
                while (!temp.IsConsumed)
                {
                    result.Add(temp.Read().ToList());
                }
            }
            return result;
        }

        public DataTable ExecuteReader(string sql, object args = null, CommandType commandType = CommandType.Text)
        {
            DataTable dt = new DataTable();
            using (IDbConnection conn = new SqlConnection(connectionString))
            {
                IDataReader reader = conn.ExecuteReader(sql, args, commandType: commandType);
                dt.Load(reader);
                var t = dt.Rows.Count;
            }
            return dt;
        }

    }
}
