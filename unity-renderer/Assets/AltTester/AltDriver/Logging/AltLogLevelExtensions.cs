using NLog;

namespace Altom.AltDriver.Logging
{
    public static class AltLogLevelExtensions
    {
        public static LogLevel ToNLogLevel(this AltLogLevel logLevel)
        {
            return LogLevel.FromOrdinal((int)logLevel);
        }
    }
}