using NodaTime;
using System;

namespace DotNetCoreWebApi{
    public static class TimeUtil
    {
        public static DateTime GetCstDateTime()
        {
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var shanghaiZone = DateTimeZoneProviders.Tzdb["Asia/Shanghai"];
            return now.InZone(shanghaiZone).ToDateTimeUnspecified();
        }

    }
    public static class DateTimeExtentions
    {
        public static DateTime ToCstTime(this DateTime time)
        {
            return TimeUtil.GetCstDateTime();
        }
    }
}