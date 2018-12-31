using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：LogHelper
    /// 功能：日志帮助类
    /// 详细：读写日志
    /// 创建者：DZT
    /// 创建日期：2014-9-22
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class LogHelper
    {
        #region 私有变量
        /// <summary>
        /// 链接字符串
        /// </summary>
        private static string m_conn = ""; 
        #endregion

        #region 写日志 +bool Write(Log model)
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="model">日志信息</param>
        /// <returns></returns>
        public static bool Write(Log model)
        {
            return WriteToDB(model) ;
        } 
        #endregion

        #region 写日志 +bool Write(string info)
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="info">日志信息</param>
        /// <returns></returns>
        public static bool Write(string info)
        {
            Log model = new Log();
            model.LOGCODE = "0";
            model.LOGDESCRIPTION = info;
            model.LOGID = Guid.NewGuid().ToString();
            model.LOGMESSAGE = info;
            model.LOGTIME = DateTime.Now;
            model.LOGTYPE =(int) LogType.INFO;
            model.UID = "";
            model.UIP = "127.0.0.1";
            Write(model);

            return true;
        } 
        #endregion


        #region 获取日志描述 +string GetLogDescription(string code)
        /// <summary>
        /// 获取日志描述
        /// </summary>
        /// <param name="code">日志信息编码</param>
        /// <returns></returns>
        public static string GetLogDescription(string code)
        {
            return "";
        }        
        #endregion

        #region 把日志保存到数据库 -static bool WriteToDB(Log model)
        /// <summary>
        /// 把日志保存到数据库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static bool WriteToDB(Log model)
        {            
            try
            {
                if (m_conn == "")
                    m_conn = System.Configuration.ConfigurationManager.ConnectionStrings["Sunny_Forecast"].ConnectionString;
                using (System.Data.SqlClient.SqlConnection conn = new SqlConnection(m_conn))
                {
                    string strSql = "INSERT INTO SY_LOG_B(LOGID,UID,LOGTYPE,LOGTIME,LOGCODE,LOGMESSAGE,LOGDESCRIPTION,UIP) VALUES (@LOGID,@UID,@LOGTYPE,@LOGTIME,@LOGCODE,@LOGMESSAGE,@LOGDESCRIPTION,@UIP)";
                    SqlParameter[] paras = {
                        new SqlParameter("@LOGID", SqlDbType.Char,36) ,            
                        new SqlParameter("@UID", SqlDbType.Char,36) ,            
                        new SqlParameter("@LOGTYPE", SqlDbType.TinyInt,1) ,            
                        new SqlParameter("@LOGTIME", SqlDbType.DateTime) ,            
                        new SqlParameter("@LOGCODE", SqlDbType.VarChar,50) ,            
                        new SqlParameter("@LOGMESSAGE", SqlDbType.NVarChar,200) ,            
                        new SqlParameter("@LOGDESCRIPTION", SqlDbType.Text) ,            
                        new SqlParameter("@UIP", SqlDbType.VarChar,16)             
                
                     };

                    paras[0].Value = model.LOGID;
                    paras[1].Value = model.UID;
                    paras[2].Value = model.LOGTYPE;
                    paras[3].Value = DateTime.Now;
                    paras[4].Value = model.LOGCODE;
                    paras[5].Value = model.LOGMESSAGE;
                    paras[6].Value = model.LOGDESCRIPTION;
                    paras[7].Value = model.UIP;

                    System.Data.SqlClient.SqlCommand cmd = new SqlCommand(strSql, conn);
                    foreach (IDataParameter parm in paras)
                        cmd.Parameters.Add(parm);
                    conn.Open();
                    int count = cmd.ExecuteNonQuery();
                    if (count > 0)
                        return true;
                }
                return false;
            }
            catch(Exception e)
            {
                return false;
            }
        } 
        #endregion

        #region 把日志保存到文件 +static bool WriteToFile(Log model)
        /// <summary>
        /// 把日志保存到文件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static bool WriteToFile(Log model)
        {
            return true;
        }
        #endregion

        /// <summary>
        /// 写日志信息到文件中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="Msg"></param>
        public static void SaveLog(string filePath, string Msg)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamWriter tw = File.AppendText(filePath))
                    {
                        tw.WriteLine(DateTime.Now.ToString() + "> " + Msg);
                    }  //END using

                }  //END if
                else
                {
                    using (FileStream fs = File.Create(filePath))
                    {
                        fs.Close();
                    }
                    using (TextWriter tw = new StreamWriter(filePath))
                    {
                        tw.WriteLine(DateTime.Now.ToString() + "> " + Msg);
                        tw.Flush();
                        tw.Close();
                    }
                }  //END else

            }  //END Try
            catch (Exception ex)
            { } //END Catch   

        }
        
    }
    /// <summary>
    /// 类名：Log
    /// 功能：实体类
    /// 详细：日志实体 
    /// 创建者：DZT
    /// 创建日期：2014-9-22
    /// 修改日期：
    /// 说明：
    /// </summary>
    [Serializable]
    public class Log
    {
        /// <summary>
        /// ID
        /// </summary>	
        public string LOGID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>	
        public string UID { get; set; }

        /// <summary>
        /// 日志类型：0：错误日志；1：操作信息；
        /// </summary>	
        public int LOGTYPE { get; set; }

        /// <summary>
        /// 时间
        /// </summary>	
        public DateTime LOGTIME { get; set; }

        /// <summary>
        /// 日志代码
        /// </summary>	
        public string LOGCODE { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>	
        public string LOGMESSAGE { get; set; }

        /// <summary>
        /// 日志描述
        /// </summary>	
        public string LOGDESCRIPTION { get; set; }

        /// <summary>
        /// 用户IP地址
        /// </summary>	
        public string UIP { get; set; }

    }
    /// <summary>
    /// 类名：LogCode
    /// 功能：日志代码
    /// 详细：日志代码 
    /// 创建者：DZT
    /// 创建日期：2014-9-22
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class LogCode
    {
        /// <summary>
        /// 代码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        public string description { get; set; }
    }
    /// <summary>
    /// 日志级别类型
    /// </summary>
    public enum LogType
    {
        FATAL,
        RROR,
        WARN,
        INFO,
        DEBUG
    }

    
}
