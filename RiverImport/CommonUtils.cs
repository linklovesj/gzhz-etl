using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RiverImport
{
    public class CommonUtils
    {
        public static int GetWeekOfYear(DateTime dt)
        {
            //一.找到第一周的最后一天（先获取1月1日是星期几，从而得知第一周周末是几）
            int firstWeekend = 7 - Convert.ToInt32(DateTime.Parse(dt.Year + "-1-1").DayOfWeek);

            //二.获取今天是一年当中的第几天
            int currentDay = dt.DayOfYear;

            //三.（今天 减去 第一周周末）/7 等于 距第一周有多少周 再加上第一周的1 就是今天是今年的第几周了
            //    刚好考虑了惟一的特殊情况就是，今天刚好在第一周内，那么距第一周就是0 再加上第一周的1 最后还是1
            return Convert.ToInt32(Math.Ceiling((currentDay - firstWeekend) / 7.0)) + 1;
        }
    }
}
