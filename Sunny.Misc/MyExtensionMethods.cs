using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sunny.Misc
{
    /// <summary>
    /// 类名：扩展方法类
    /// 功能：实现需要的扩展方法 
    /// 详细：
    /// 创建者：LHL
    /// 创建日期：2014-9-30
    /// 修改日期：
    /// 说明：
    /// </summary>
    public static class MyExtensionMethods
    {
        /// <summary>
        /// 泛型接口IList的扩展方法
        /// 实现泛型的深度拷贝
        /// </summary>
        /// <typeparam name="T">实体对象</typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IList<T> Clone<T>(this IList<T> source)
                                        where T : ICloneable
        {
            IList<T> newList = new List<T>(source.Count);
            foreach (var item in source)
            {
                newList.Add((T)((ICloneable)item.Clone()));
            }
            return newList;
        }
    }
}
