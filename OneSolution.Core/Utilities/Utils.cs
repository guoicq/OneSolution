using System;
using System.Collections.Generic;
using System.Text;

namespace OneSolution.Core.Utilities
{
    public static class Utils
    {
        public static T Max<T>(T a, T b)
            where T : IComparable
        {
            return a.CompareTo(b) >= 0 ? a : b;
        }

        public static T Min<T>(T a, T b)
            where T : IComparable
        {
            return a.CompareTo(b) <= 0 ? a : b;
        }

        public static DateTime ParseConfigDate(string strDate, DateTime? defaultValue = null)
        {
            if (strDate != null && strDate.Trim() != "")
                return DateTime.ParseExact(strDate, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            else if (defaultValue != null)
                return defaultValue.Value;
            else
                throw new ArgumentNullException(strDate);
        }
    }
}
