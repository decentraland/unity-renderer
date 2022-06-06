using System;

public static class TimestampExtensions
{
    public static DateTime ToDateTimeFromUnixTimestamp(this ulong milliseconds)
    {
        var result = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return result.AddMilliseconds(milliseconds);
    }
}