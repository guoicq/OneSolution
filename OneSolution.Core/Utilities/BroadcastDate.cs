using System;
using System.Runtime.InteropServices;

namespace OneSolution.Core.Utilities
{
    [AllowConversionFrom(typeof(DateTime))]
    [ConvertTo(typeof(DateTime))]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BroadcastDate : IComparable, IComparable<BroadcastDate>, IEquatable<BroadcastDate>, IFormattable, IConvertible
    {
        private static DateTime epoch = new DateTime(1990, 1, 1);
        private const int epochWeekDay = 0; // Jan 1, 1990 was a Monday
        internal int secondsFromEpoch;

        public BroadcastDate(DateTime date)
        {
            this.secondsFromEpoch = (int)(date - epoch).TotalSeconds;
        }

        internal BroadcastDate(int secondsFromEpoch)
        {
            this.secondsFromEpoch = secondsFromEpoch;
        }

        public BroadcastDate(int year, int month, int day)
            : this(new DateTime(year, month, day))
        {
        }
        public static BroadcastDate Null => new BroadcastDate { secondsFromEpoch = 0 };
        public static BroadcastDate MinValue => new BroadcastDate { secondsFromEpoch = 1 };
        public static BroadcastDate MaxValue => new BroadcastDate { secondsFromEpoch = int.MaxValue };

        public DateTime AsDate
        {
            get { return epoch.AddSeconds(secondsFromEpoch); }
        }

        public bool IsNull => secondsFromEpoch == 0;

        public static implicit operator BroadcastDate(DateTime date)
        {
            return new BroadcastDate(date);
        }

        public static implicit operator DateTime(BroadcastDate date)
        {
            return date.AsDate;
        }

        public override string ToString()
        {
            return AsDate.ToString();
        }
        public string AsTimeString => string.Format("{0:00}:{1:00}:{2:00}", BroadcastHour, HourMinute, secondsFromEpoch % 60);

        public string AsEndTimeString => string.Format("{0:00}:{1:00}:{2:00}", BroadcastHourEndTime, HourMinute, secondsFromEpoch % 60);

        public override int GetHashCode()
        {
            return secondsFromEpoch.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals((BroadcastDate)obj);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((BroadcastDate)obj);
        }

        public int CompareTo(BroadcastDate other)
        {
            return this.secondsFromEpoch.CompareTo(other.secondsFromEpoch);
        }

        public bool Equals(BroadcastDate other)
        {
            return this.secondsFromEpoch == other.secondsFromEpoch;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return AsDate.ToString(format, formatProvider);
        }
        public BroadcastDate Date
        {
            get {
                return new BroadcastDate(secondsFromEpoch - secondsFromEpoch % (3600 * 24));
            }
        }
        public BroadcastDate AdjustedDate
        {
            get {
                int second = secondsFromEpoch % (3600 * 24);
                if (second < 7200)
                    return new BroadcastDate(secondsFromEpoch - second - 3600 * 24);
                else
                    return new BroadcastDate(secondsFromEpoch - second);
            }
        }
        public int DayOfWeek
        {
            get {
                return (secondsFromEpoch / (3600 * 24) + epochWeekDay) % 7 + 1;
            }
        }
        public int WeekFromEpoch
        {
            get {
                return (secondsFromEpoch / (3600 * 24) + epochWeekDay) / 7;
            }
        }
        public int BroadcastWeek
        {

            get {
                return this.WeekFromEpoch - this.BroadcastYearStart.WeekFromEpoch + 1;
            }
        }

        public BroadcastDate BroadcastMonthStart
        {
            get {
                var firstDayOfCalendarMonth = new BroadcastDate(new DateTime(this.AsDate.Year, this.BroadcastMonth, 1));
                return firstDayOfCalendarMonth.AddDays(-1 * (firstDayOfCalendarMonth.DayOfWeek - 1));
            }
        }

        public BroadcastDate BroadcastWeekStart
        {
            get {
                return this.Date.AddDays(-1 * (DayOfWeek - 1));
            }
        }

        public int BroadcastMonth
        {
            get {
                var thisMonthNum = this.AsDate.Month;
                var nextMonthStartDate =
                    thisMonthNum == 12
                        ? new BroadcastDate(this.AsDate.Year + 1, 1, 1)
                        : new BroadcastDate(this.AsDate.Year, thisMonthNum + 1, 1);

                if (nextMonthStartDate.WeekFromEpoch == this.WeekFromEpoch)
                    return nextMonthStartDate.AsDate.Month;
                else
                    return thisMonthNum;
            }
        }

        public BroadcastDate BroadcastYearStart
        {
            get {
                var broadcastYearStart = new BroadcastDate(this.AsDate.Year, 9, 1);

                if (this.WeekFromEpoch < broadcastYearStart.WeekFromEpoch)
                    broadcastYearStart = new BroadcastDate(this.AsDate.Year - 1, 9, 1);

                broadcastYearStart = broadcastYearStart.AddDays(-1 * (broadcastYearStart.DayOfWeek - 1));

                return broadcastYearStart;
            }
        }
        public int DayMinute
        {
            get {
                return DaySecond / 60;
            }
        }
        public int DaySecond
        {
            get {
                return secondsFromEpoch % (3600 * 24);
            }
        }
        public int BroadcastHour
        {
            get {
                int hr = DaySecond / 3600;
                if (hr < 2) hr += 24;
                return hr;
            }
        }
        public int BroadcastHourEndTime
        {
            get {
                int hr = DaySecond / 3600;
                if (hr < 2 || hr == 2 && HourMinute == 0) hr += 24;
                return hr;
            }
        }
        public int DayHour
        {
            get {
                return DaySecond / 3600;
            }
        }
        public int HourMinute
        {
            get {
                return (DaySecond - DayHour * 3600) / 60;
            }
        }

        public BroadcastDate AddDays(int days)
        {
            return new BroadcastDate(secondsFromEpoch + days * 3600 * 24);
        }

        public BroadcastDate AddMinutes(int minutes)
        {
            return new BroadcastDate(secondsFromEpoch + minutes * 60);
        }

        /// <summary>
        /// Gives the current date relative to the broadcast calendar from the requisite number of offset years
        /// Ex: Tuesday of third broadcast week gets pushed to the Tuesday of the third broadcast week of the specified offset year
        /// </summary>
        public BroadcastDate AddBroadcastYears(int years)
        {
            var offsetDaysFromBroadcastYearStart =
                this.AsDate.Subtract(this.BroadcastYearStart.AsDate);
            var targetYearStart = new BroadcastDate(
                new DateTime(this.BroadcastYearStart.AsDate.Year + years, 9, 1))
                .BroadcastYearStart;
            return targetYearStart.AsDate.Add(offsetDaysFromBroadcastYearStart);
        }

        public static bool operator <(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch < b.secondsFromEpoch;
        }

        public static bool operator >(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch > b.secondsFromEpoch;
        }

        public static bool operator <=(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch <= b.secondsFromEpoch;
        }

        public static bool operator >=(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch >= b.secondsFromEpoch;
        }

        public static bool operator ==(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch == b.secondsFromEpoch;
        }

        public static bool operator !=(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch != b.secondsFromEpoch;
        }

        public static bool operator ==(BroadcastDate a, DateTime b)
        {
            return a.secondsFromEpoch == ((BroadcastDate)b).secondsFromEpoch;
        }

        public static bool operator !=(BroadcastDate a, DateTime b)
        {
            return a.secondsFromEpoch != ((BroadcastDate)b).secondsFromEpoch;
        }

        public static int operator -(BroadcastDate a, BroadcastDate b)
        {
            return a.secondsFromEpoch - b.secondsFromEpoch;
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            return TypeCode.DateTime;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return this.AsDate;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(DateTime))
                return AsDate;
            else
                throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    [AllowConversionFrom(typeof(DateTime))]
    [ConvertTo(typeof(DateTime))]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BroadcastDateShort : IComparable, IComparable<BroadcastDateShort>, IEquatable<BroadcastDateShort>, IFormattable, IConvertible
    {
        private static DateTime epoch = new DateTime(1990, 1, 1);
        private const int epochWeekDay = 0; // Jan 1, 1990 was a Monday
        private short daysFromEpoch;


        public BroadcastDateShort(DateTime date)
        {
            this.daysFromEpoch = (short)(date - epoch).TotalDays;
        }

        public BroadcastDateShort(BroadcastDate date)
        {
            this.daysFromEpoch = (short)(date.secondsFromEpoch / (3600 * 24));
        }

        public BroadcastDateShort(int year, int month, int day)
            : this(new DateTime(year, month, day))
        {
        }
        public static BroadcastDateShort Null => new BroadcastDateShort { daysFromEpoch = 0 };

        public DateTime AsDate
        {

            get { return epoch.AddDays(daysFromEpoch); }
        }

        public static bool operator ==(BroadcastDateShort a, BroadcastDate b)
        {
            return a.daysFromEpoch == (short)(b.secondsFromEpoch / (3600 * 24));
        }

        public static bool operator !=(BroadcastDateShort a, BroadcastDate b)
        {
            return !(a == b);
        }

        public static implicit operator BroadcastDateShort(DateTime date)
        {
            return new BroadcastDateShort(date);
        }

        public static implicit operator DateTime(BroadcastDateShort date)
        {
            return date.AsDate;
        }

        public static implicit operator BroadcastDateShort(BroadcastDate date)
        {
            return new BroadcastDateShort(date);
        }

        public static implicit operator BroadcastDate(BroadcastDateShort date)
        {
            return new BroadcastDate(date.daysFromEpoch * 3600 * 24);
        }

        public override string ToString()
        {
            return AsDate.ToString();
        }

        public override int GetHashCode()
        {
            return daysFromEpoch.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals((BroadcastDateShort)obj);
        }

        public int CompareTo(object obj)
        {
            return this.CompareTo((BroadcastDateShort)obj);
        }

        public int CompareTo(BroadcastDateShort other)
        {
            return this.daysFromEpoch.CompareTo(other.daysFromEpoch);
        }

        public bool Equals(BroadcastDateShort other)
        {
            return this.daysFromEpoch == other.daysFromEpoch;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return AsDate.ToString(format, formatProvider);
        }
        public int DayOfWeek
        {

            get {
                return (daysFromEpoch + epochWeekDay) % 7 + 1;
            }
        }

        public BroadcastDateShort AddDays(short days)
        {
            return new BroadcastDateShort { daysFromEpoch = (short)(daysFromEpoch + days) };
        }

        public static bool operator <(BroadcastDateShort a, BroadcastDateShort b)
        {
            return a.daysFromEpoch < b.daysFromEpoch;
        }

        public static bool operator >(BroadcastDateShort a, BroadcastDateShort b)
        {
            return a.daysFromEpoch > b.daysFromEpoch;
        }

        public static bool operator ==(BroadcastDateShort a, BroadcastDateShort b)
        {
            return a.daysFromEpoch == b.daysFromEpoch;
        }

        public static bool operator !=(BroadcastDateShort a, BroadcastDateShort b)
        {
            return a.daysFromEpoch != b.daysFromEpoch;
        }

        #region IConvertible
        public TypeCode GetTypeCode()
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return this.AsDate;
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
