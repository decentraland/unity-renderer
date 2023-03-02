using DCL.Interface;
using UnityEngine;

namespace DCL.LogReport
{
    public class LogReportKernelInterface
    {
        [System.Serializable]
        public class LogReportPayload
        {
            public string type;
            public string message;
        }

        private static LogReportPayload logReportPayload = new LogReportPayload();

        public static void ReportLog(LogType type, string message)
        {
            switch (type)
            {
                case LogType.Error:
                    ReportLog("error", message);
                    break;
                case LogType.Assert:
                    ReportLog("warning", message);
                    break;
                case LogType.Exception:
                    ReportLog("error", message);
                    break;
                case LogType.Warning:
                    ReportLog("warning", message);
                    break;
                default:
                    ReportLog("log", message);
                    break;
            }
        }

        public static void ReportLog(string type, string message)
        {
            logReportPayload.type = type;
            logReportPayload.message = message;
            WebInterface.SendMessage("ReportLog", logReportPayload);
        }
    }
}
