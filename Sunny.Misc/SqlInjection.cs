using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：SqlInjection
    /// 功能：SQL 防注入
    /// 详细：简单的SQL防治注入
    /// 创建者：DZT
    /// 创建日期：2014-9-22
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class SqlInjection
    {
        #region 关键字过滤
        /// <summary>
        /// 关键字过滤
        /// </summary>
        /// <param name="originalString">原始串</param>
        /// <returns>返回True就是找到了可能sql注入的关键字</returns>
        public static bool HasInjection(string originalString)
        {
            //参考：technet.microsoft.com/zh-cn/library/ms161953.aspx
            if (originalString.IndexOf(";") != -1 || originalString.IndexOf("'") != -1 || originalString.IndexOf("--") != -1 || originalString.IndexOf("/*") != -1 || originalString.IndexOf("*/") != -1 || originalString.IndexOf("xp_cmdshell") != -1)
                return true;
            else
                return false;
        }

        #endregion

        #region 将文本转换成适合在Sql语句里使用的字符串。
        /// <summary>
        /// 将文本转换成适合在Sql语句里使用的字符串。
        /// </summary>
        /// <param name="pStr">SQL语句</param>
        /// <returns>转换后文本</returns>	
        public static String GetQuotedString(String pStr)
        {
            return ("'" + pStr.Replace("'", "''") + "'");
        }

        /// <summary>
        /// 将文本转换成适合在Sql语句里使用的字符串。
        /// </summary>
        /// <param name="pStr">SQL语句</param>
        /// <returns>转换后文本</returns>	
        public static String GetQuotedStringN(String pStr)
        {
            return ("N'" + pStr.Replace("'", "''") + "'");
        }

        /// <summary>
        /// 将文本转换成适合在Sql语句里使用的字符串。
        /// </summary>
        /// <param name="pStr">SQL语句</param>
        /// <returns>转换后文本</returns>	
        public static String GetString(String pStr)
        {
            return (pStr.Replace("'", "''"));
        }

        /// <summary>
        /// 将文本转换成适合在Sql语句里使用的字符串。
        /// </summary>
        /// <param name="pStr">SQL语句</param>
        /// <returns>转换后文本</returns>	
        public static String GetStringN(String pStr)
        {
            return ("N" + pStr.Replace("'", "''"));
        }

        #endregion
    }
}
