using System;
using System.Collections.Generic;
using System.Text;

namespace ArkTrading
{
    public static class RowKeyHelper
    {
        // RowKey = string.Concat(string.Format("{0:D19}", DateTimeOffset.MaxValue.Ticks - DateTimeOffset.UtcNow.Ticks), "-" , Guid.NewGuid().ToString("N"));
        // DateTime dt = new DateTime(DateTime.MaxValue.Ticks - Int64.Parse(invertedTicks));

        public static string ToRowKey(DateTime dateTime)
        {
            return string.Concat(ToRowKeyPrefix(dateTime), "-", Guid.NewGuid().ToString("N"));
        }
        public static string ToRowKeyPrefix(DateTime dateTime)
        {
            return string.Format("{0:D19}", DateTimeOffset.MaxValue.Ticks - dateTime.Ticks);
        }
        public static DateTime FromRowKeyPrefix(string invertedTicks)
        {
            return new DateTime(DateTime.MaxValue.Ticks - Int64.Parse(invertedTicks));
        }

    }
}
