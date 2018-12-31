using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Reflection;

namespace Sunny.Misc
{
    public static  class SqlHelper5
    {


         #region 变量

        private static OracleConnection _con = null;
        public static string _constr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            //= ConfigurationManager.ConnectionStrings["orcl"].ToString();
        
        #endregion  
 
 
        #region 属性  
  
        public static string constr
        {
            get
            {
                if (_constr == null||_constr.Equals(String.Empty))
                {
                    string server = System.Configuration.ConfigurationManager.AppSettings["server"];
                    string service_name = System.Configuration.ConfigurationManager.AppSettings["service_name"];
                    string pwd = System.Configuration.ConfigurationManager.AppSettings["pwd"];
                    string uid = System.Configuration.ConfigurationManager.AppSettings["uid"];
                    string connString = string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT=1521))(CONNECT_DATA=(SERVICE_NAME={1})));Persist Security Info=True;User ID={2};Password={3};"
                        ,server,service_name,uid,pwd);
                    _constr = connString;
                }
                return _constr;
            }
            set
            {
                _constr = value;
            }
        }

        /// <summary>  
        /// 获取或设置数据库连接对象  
        /// </summary>  
        public static OracleConnection Con  
        {
            get  
            {
                   
                if (SqlHelper5._con == null)  
                {
                    SqlHelper5._con = new OracleConnection();  
                }
                if (SqlHelper5._con.ConnectionString == null || SqlHelper5._con.ConnectionString.Equals(string.Empty))
                {
                    SqlHelper5._con.ConnectionString = SqlHelper5.constr;
                }
                return SqlHelper5._con;  
            }  
            set  
            {  
                SqlHelper5._con = value;  
            }  
        }  
        #endregion  
 
        #region 方法  
 
        /// <summary>    
        /// 执行并返回第一行第一列的数据库操作  
        /// </summary>  
        /// <param name="commandText">Sql语句或存储过程名</param>  
        /// <param name="commandType">Sql命令类型</param>  
        /// <param name="param">Oracle命令参数数组</param>  
        /// <returns>第一行第一列的记录</returns>  
        public static int ExecuteScalar(string commandText, CommandType commandType, params OracleParameter[] param)  
        {   
            int result = 0;  
            try{
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))  
                {  
                    try  
                    {  
                        cmd.CommandType = commandType;  
                        if (param!=null)  
                        {  
                            cmd.Parameters.AddRange(param);  
                        }  
                        SqlHelper5.Con.Open();
                        string x = cmd.CommandText;
                        result = Convert.ToInt32(cmd.ExecuteScalar());  
                    }
                    catch
                    {
                        result = -1;
                    }  
                }  
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return result;
        }
  
        /// <summary>  
        /// 执行不查询的数据库操作  
        /// </summary>  
        /// <param name="commandText">Oracle语句或存储过程名</param>  
        /// <param name="commandType">Oracle命令类型</param>  
        /// <param name="param">Oracle命令参数数组</param>  
        /// <returns>受影响的行数</returns>  
        public static int ExecuteNonQuery(string commandText, CommandType commandType, params OracleParameter[] param)  
        {  
            int result = 0;
            try
            {
               
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))  
                {  
                    try  
                    {  
                        cmd.CommandType = commandType;  
                        if (param!=null)  
                        {  
                            cmd.Parameters.AddRange(param);  
                        }
                        if (SqlHelper5.Con.State!=ConnectionState.Open)
                        {
                           SqlHelper5.Con.Open();  
                        }
                         
                        result = cmd.ExecuteNonQuery();  
                    }  
                    catch
                    {
                        throw;
                    }  
                }
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return result;  
        }

        /// <summary>
        /// 获取数据表
        /// </summary>
        /// <param name="commandText">select命令</param>
        /// <param name="param">参数表</param>
        /// <returns></returns>
        public static DataTable GetDataTable(string commandText,params OracleParameter[] param)
        {
            DataTable result = new DataTable();
            try
            {
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))
                {
                    if (param!=null)
                    {
                        cmd.Parameters.AddRange(param);
                    }
                    try {                        
                        OracleDataAdapter adapter = new OracleDataAdapter(cmd);
                        adapter.Fill(result);
                    }
                    catch
                    {
                        result = null;
                    }
                }
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return result;
        }

        public static int GetNextValueInSequence(string sequenceName)
        {
            if (ExecuteScalar("select count(*) from user_objects where OBJECT_NAME=:seqName", CommandType.Text, new OracleParameter(":seqName",sequenceName)) > 0)
            {
                return ExecuteScalar("select " + sequenceName + ".nextval from dual", CommandType.Text);
            }
            else
            {
                return -1;
            }


        }

        /// <summary>
        /// 事务模式执行多行非查询语句
        /// </summary>
        /// <param name="commandText">sql语句</param>
        /// <param name="param">参数</param>
        /// <returns>受影响行数</returns>
        public static int ExecuteNonQueryTransaction(string commandText, List<OracleParameter[]> param)
        {
            int result = 0;
            try
            {
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))
                {
                    SqlHelper5.Con.Open();
                    cmd.Transaction = cmd.Connection.BeginTransaction();
                    try
                    {
                        foreach (OracleParameter[] par in param)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddRange(par);
                            result += cmd.ExecuteNonQuery();
                        }
                        cmd.Transaction.Commit();
                    }
                    catch
                    {
                        result = -1;
                        try
                        {
                            cmd.Transaction.Rollback();
                        }
                        catch
                        {
                            result = -2;
                        }
                    }
                }
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return result;
        }


        /// <summary>   
        /// 执行返回一条记录的泛型对象
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>  
        /// <param name="reader">只进只读对象</param>  
        /// <returns>泛型对象</returns>  
        private static T ExecuteDataReader<T>(IDataReader reader)  
        {    
            T obj = default(T);  
            try  
            {  
                Type type = typeof(T);  
                obj = (T)Activator.CreateInstance(type);//从当前程序集里面通过反射的方式创建指定类型的对象
                //obj = (T)Assembly.Load(SqlHelper5._assemblyName).CreateInstance(SqlHelper5._assemblyName + "." + type.Name);//从另一个程序集里面通过反射的方式创建指定类型的对象
                PropertyInfo[] propertyInfos = type.GetProperties();//获取指定类型里面的所有属性  
                foreach (PropertyInfo propertyInfo in propertyInfos)  
                {  
                    for (int i = 0; i < reader.FieldCount; i++)  
                    {  
                        string fieldName = reader.GetName(i);  
                        if (fieldName.ToLower() == propertyInfo.Name.ToLower())  
                        {  
                            object val = reader[propertyInfo.Name];//读取表中某一条记录里面的某一列  
                            if (val != null && val != DBNull.Value)  
                            {
                                Type valType = val.GetType();

                                if (valType == typeof(float) || valType == typeof(double) || valType== typeof(decimal))
                                {
                                    propertyInfo.SetValue(obj, Convert.ToDouble(val), null);
                                }
                                else if (valType == typeof(int))  
                                {  
                                    propertyInfo.SetValue(obj, Convert.ToInt32(val), null);  
                                }  
                                else if (valType == typeof(DateTime))  
                                {  
                                    propertyInfo.SetValue(obj, Convert.ToDateTime(val), null);  
                                }  
                                else if (valType == typeof(string))  
                                {  
                                    propertyInfo.SetValue(obj, Convert.ToString(val), null);  
                                }  
                            }  
                            break;  
                        }  
                    }  
                }  
            }  
            catch
            {  
                throw;  
            }  
            return obj;  
        }  
 
        /// <summary>    
        /// 执行返回一条记录的泛型对象  
        /// </summary>  
        /// <typeparam name="T">泛型类型</typeparam>  
        /// <param name="commandText">Oracle语句或存储过程名</param>  
        /// <param name="commandType">Oracle命令类型</param>  
        /// <param name="param">Oracle命令参数数组</param>  
        /// <returns>实体对象</returns>  
        public static T ExecuteEntity<T>(string commandText, CommandType commandType, params OracleParameter[] param)  
        {  
            T obj = default(T);  
            try
            {  
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))  
                {  
                    cmd.CommandType = commandType;  
                    cmd.Parameters.AddRange(param);  
                    SqlHelper5.Con.Open();  
                    OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);  
                    while (reader.Read())  
                    {  
                        obj = SqlHelper5.ExecuteDataReader<T>(reader);  
                    }  
                }  
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return obj;  
        }  
 
        /// <summary>  
        /// 执行返回多条记录的泛型集合对象  
        /// </summary>  
        /// <typeparam name="T">泛型类型</typeparam>  
        /// <param name="commandText">Oracle语句或存储过程名</param>  
        /// <param name="commandType">Oracle命令类型</param>  
        /// <param name="param">Oracle命令参数数组</param>  
        /// <returns>泛型集合对象</returns>  
        public static List<T> ExecuteList<T>(string commandText, CommandType commandType, params OracleParameter[] param)  
        {  
            List<T> list = new List<T>();  
            try
            {  
                using (OracleCommand cmd = new OracleCommand(commandText, SqlHelper5.Con))  
                {  
                    try  
                    {  
                        cmd.CommandType = commandType;
                        
                        if (param != null)  
                        {  
                            cmd.Parameters.AddRange(param);  
                        }  
                        SqlHelper5.Con.Open();  
  
                        OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);  
                        
                        while (reader.Read())  
                        {  
                            T obj = SqlHelper5.ExecuteDataReader<T>(reader);  
                            list.Add(obj);  
                        }  
                    }  
                    catch (Exception ex)  
                    {  
                        list = null;  
                    }  
                }  
            }
            finally
            {
                if (SqlHelper5.Con.State != ConnectionState.Closed)
                {
                    SqlHelper5.Con.Close();
                }
            }
            return list;
        }

        #endregion
    //}
    }
}
