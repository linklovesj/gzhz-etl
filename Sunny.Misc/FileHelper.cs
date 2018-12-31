using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：FileHelper
    /// 功能：文件帮助类
    /// 详细：文件的读写等操作
    /// 创建者：DZT
    /// 创建日期：2014-9-18
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// 写日志信息到文件中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="msg"></param>
        public static void SaveLog(string filePath, string msg)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (StreamWriter tw = File.AppendText(filePath))
                    {
                        tw.Write(msg);
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
                        tw.Write(msg);
                        tw.Flush();
                        tw.Close();
                    }
                }  //END else

            }  //END Try
            catch (Exception ex)
            { } //END Catch 
        }
    }
}
