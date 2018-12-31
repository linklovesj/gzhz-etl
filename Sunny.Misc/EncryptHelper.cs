using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：EncryptHelper
    /// 功能：加密、解密帮助类
    /// 详细：加密、解密帮助类
    /// 创建者：DZT
    /// 创建日期：2014-9-18
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class EncryptHelper
    {
        private static string _strKey = "ANHUIRIVERFORECAST";
        #region ========DES 加密========
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string DESEncrypt(string Text)
        {
            return DESEncrypt(Text, _strKey);
        }
        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string DESEncrypt(string Text,string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            Byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            //加密密匙转化为byte数组
            byte[] key = Encoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.Key = key; 
            des.IV = key;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //创建其支持存储区为内存的流
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            //向可变字符串追加转换成十六进制数字符串的加密后byte数组。
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString(); 
        }
        #endregion

        #region ========DES 解密========
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string DESDecrypt(string Text)
        {
            return DESDecrypt(Text, _strKey);
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string DESDecrypt(string Text,string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            //将被解密的字符串每两个字符以十六进制解析为byte类型，组成byte数组
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            byte[] key = Encoding.ASCII.GetBytes(System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(sKey, "md5").Substring(0, 8));
            des.Key = key;
            des.IV = key;
            //创建其支持存储区为内存的流
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            //定义将数据流链接到加密转换的流
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
            //  return ASCIIEncoding.UTF8.GetString((ms.ToArray()));
        }
        #endregion

        #region MD5算法加密字符串( 32位 )

        #region 重载1
        /// <summary>
        /// MD5算法加密字符串( 32位 )
        /// </summary>
        /// <param name="text">要加密的字符串</param>    
        /// <param name="encoding">字符编码</param>    
        public static string MD5(string text, Encoding encoding)
        {
            //如果字符串为空，则返回
            if (ValidationHelper.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            try
            {
                text += _strKey;
                //创建MD5密码服务提供程序
                MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

                //计算传入的字节数组的哈希值
                byte[] hashCode = md5.ComputeHash(encoding.GetBytes(text));

                //释放资源
                md5.Clear();

                //返回MD5值的字符串表示
                string temp = "";
                for (int i = 0, len = hashCode.Length; i < len; i++)
                {
                    temp += hashCode[i].ToString("x").PadLeft(2, '0');
                }
                return temp;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region 重载2
        /// <summary>
        /// MD5算法加密字符串( 32位 )
        /// </summary>
        /// <param name="text">要加密的字符串</param>
        public static string MD5(string text)
        {
            return MD5(text, Encoding.UTF8);
        }
        #endregion
        #endregion
    }
}

