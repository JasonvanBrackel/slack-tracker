using System;

namespace Rancher.Community.Slack.Data
{
    public static class DataHelper
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(this long unixTime)
        {
            return Epoch.AddSeconds(unixTime);
        }

        public static DateTime FromSlackTimestamp(this string timestamp)
        {
            double unixTime = double.Parse(timestamp);
            return Epoch.AddSeconds(unixTime);
        }

        public static string ToSlackTimestamp(this DateTime dateTime)
        {
            var t =  dateTime.ToUniversalTime() - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return secondsSinceEpoch.ToString();
        }
    }
}