using System;
using UnityEngine;

namespace DCL.LogReport
{
    public class LogReportPlugin : IPlugin
    {
        public LogReportPlugin()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionCallback;
            Application.logMessageReceived += LogCallback;
        }

        private void UnhandledExceptionCallback(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
                LogReportKernelInterface.ReportLog("error", exception.Message);
        }

        private void LogCallback(string condition, string stackTrace, LogType type)
        {
            System.Diagnostics.StackTrace sTrace = new System.Diagnostics.StackTrace();
            string message = $"{condition}\n{sTrace.ToString()}";

            LogReportKernelInterface.ReportLog(type, message);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.UnhandledException -= UnhandledExceptionCallback;
            Application.logMessageReceived -= LogCallback;
        }
    }
}
