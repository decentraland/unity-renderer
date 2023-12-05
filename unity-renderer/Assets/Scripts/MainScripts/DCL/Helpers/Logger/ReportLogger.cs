using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class ReportLogger : ILogHandler
    {
        private readonly ILogHandler unityLogHandler;
        private readonly string prefix;

        public ReportLogger(string prefix, ILogHandler unityLogHandler)
        {
            this.unityLogHandler = unityLogHandler;
            this.prefix = prefix;
        }

        [HideInCallstack]
        public void LogException(Exception exception, Object context)
        {
            Exception newException = new Exception(prefix + exception.Message, exception);
            unityLogHandler.LogException(newException, context);
        }

        [HideInCallstack]
        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            string newFormat = prefix + format;
            unityLogHandler.LogFormat(logType, context, newFormat, args);
        }
    }
}
