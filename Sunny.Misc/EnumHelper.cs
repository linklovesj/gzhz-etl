using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：EnumHelper
    /// 功能：枚举帮助类
    /// 详细：枚举转换等操作
    /// 创建者：LHL
    /// 创建日期：2014-9-28
    /// 修改日期：
    /// 说明：
    /// </summary>
    public class EnumHelper
    {
        /// <summary>
        /// 枚举缓存池
        /// </summary>
        private static Dictionary<string, Dictionary<int, string>> m_dicEnumListCache = new Dictionary<string, Dictionary<int, string>>();
        private static Dictionary<string, Dictionary<string, string>> m_dicEnumListCache_Name = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 将枚举类型转换成字典(键值对：Int-Description)
        /// </summary>
        /// <param name="eumType">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<int, string> EnumToDictionary(Type eumType)
        {
            string strKeyName = eumType.FullName;

            if (!m_dicEnumListCache.ContainsKey(strKeyName))
            {
                Dictionary<int, string> dicList = new Dictionary<int, string>();
                foreach (int i in Enum.GetValues(eumType))
                {
                    string strName = Enum.GetName(eumType, i);
                    string strDescription = string.Empty;
                    object[] atts = eumType.GetField(strName).GetCustomAttributes(typeof(DescriptionAttribute), false);
                    strDescription = atts.Length > 0 ? (atts[0] as DescriptionAttribute).Description : strDescription;

                    dicList.Add(i, !string.IsNullOrEmpty(strDescription) ? strDescription : strName);
                }

                object objSync = new object();
                if (!m_dicEnumListCache.ContainsKey(strKeyName))
                {
                    lock (objSync)
                    {
                        if (!m_dicEnumListCache.ContainsKey(strKeyName))
                        {
                            m_dicEnumListCache.Add(strKeyName, dicList);
                        }
                    }
                }
            }

            return m_dicEnumListCache[strKeyName];
        }

        /// <summary>
        /// 将枚举类型转换成字典(键值对：Name-Description)
        /// </summary>
        /// <param name="eumType">枚举类型</param>
        /// <returns></returns>
        public static Dictionary<string, string> EnumToDictionary_Name(Type eumType)
        {
            string strKeyName = eumType.FullName;

            if (!m_dicEnumListCache_Name.ContainsKey(strKeyName))
            {
                Dictionary<string, string> dicList = new Dictionary<string, string>();
                foreach (int i in Enum.GetValues(eumType))
                {
                    string strName = Enum.GetName(eumType, i);
                    string strDescription = string.Empty;
                    object[] atts = eumType.GetField(strName).GetCustomAttributes(typeof(DescriptionAttribute), false);
                    strDescription = atts.Length > 0 ? (atts[0] as DescriptionAttribute).Description : strDescription;

                    dicList.Add(strName, !string.IsNullOrEmpty(strDescription) ? strDescription : strName);
                }

                object objSync = new object();
                if (!m_dicEnumListCache_Name.ContainsKey(strKeyName))
                {
                    lock (objSync)
                    {
                        if (!m_dicEnumListCache_Name.ContainsKey(strKeyName))
                        {
                            m_dicEnumListCache_Name.Add(strKeyName, dicList);
                        }
                    }
                }
            }

            return m_dicEnumListCache_Name[strKeyName];
        }

        /// <summary>
        /// 通过ID获取枚举中该ID对应的描述
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eumType">枚举类型</param>
        /// <returns></returns>
        public static string GetDescriptionById(int id, Type eumType)
        {
            string strKeyName = eumType.FullName;
            if (!m_dicEnumListCache.ContainsKey(strKeyName))
            {
                EnumToDictionary(eumType);
            }

            if (m_dicEnumListCache[strKeyName].ContainsKey(id))
                return m_dicEnumListCache[strKeyName][id];
            else
                return string.Empty;
        }

        /// <summary>
        /// 通过Name获取枚举中该Name对应的描述
        /// </summary>
        /// <param name="id"></param>
        /// <param name="eumType">枚举类型</param>
        /// <returns></returns>
        public static string GetDescriptionByName(string name, Type eumType)
        {
            string strKeyName = eumType.FullName;
            if (!m_dicEnumListCache_Name.ContainsKey(strKeyName))
            {
                EnumToDictionary_Name(eumType);
            }

            if (m_dicEnumListCache_Name[strKeyName].ContainsKey(name))
                return m_dicEnumListCache_Name[strKeyName][name];
            else
                return string.Empty;
        }

        /// <summary>
        /// 通过枚举的描述查找对应的键值
        /// </summary>
        /// <param name="description"></param>
        /// <param name="eumType"></param>
        /// <returns></returns>
        public static int GetIDByDescription(string description, Type eumType)
        {
            string strKeyName = eumType.FullName;
            if (!m_dicEnumListCache.ContainsKey(strKeyName))
            {
                EnumToDictionary(eumType);
            }
            int iId = 0;
            foreach (int item in m_dicEnumListCache[strKeyName].Keys)
            {
                if (m_dicEnumListCache[strKeyName][item] == description)
                {
                    iId = item;
                    break;
                }
            }
            return iId;
        }
    }
}
