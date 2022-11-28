using System;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Altom.AltDriver.Logging
{
    public class DriverLogManager
    {
        const string LOGSFILEPATH = "./AltTester.log";

        public static LogFactory Instance { get { return instance.Value; } }

        private static readonly Lazy<LogFactory> instance = new Lazy<LogFactory>(buildLogFactory);

        internal static void SetupAltDriverLogging(Dictionary<AltLogger, AltLogLevel> minLogLevels)
        {
            foreach (var key in minLogLevels.Keys)
            {
                SetMinLogLevel(key, minLogLevels[key]);
            }

            Instance.GetCurrentClassLogger().Info(AltLogLevel.Info.ToNLogLevel());
            AltLogLevel level;
            if (minLogLevels.TryGetValue(AltLogger.File, out level) && level != AltLogLevel.Off)
                Instance.GetCurrentClassLogger().Info("AltTester logs are saved at: " + LOGSFILEPATH);
        }

        /// <summary>
        /// Reconfigures the NLog logging level.
        /// </summary>
        /// <param name="minLogLevel">The <see cref="AltLogLevel" /> to be set.</param>
        public static void SetMinLogLevel(AltLogger loggerType, AltLogLevel minLogLevel)
        {

            foreach (var rule in Instance.Configuration.LoggingRules)
            {
                if (rule.Targets[0].Name == string.Format("AltDriver{0}Target", loggerType))
                {
                    if (minLogLevel == AltLogLevel.Off)
                    {
                        rule.SetLoggingLevels(LogLevel.Off, LogLevel.Off);
                    }
                    else
                    {
                        rule.SetLoggingLevels(minLogLevel.ToNLogLevel(), LogLevel.Fatal);
                    }
                }
            }

            Instance.ReconfigExistingLoggers();
        }

        public static void ResumeLogging()
        {
            Instance.ResumeLogging();
        }

        public static void SuspendLogging()
        {
            Instance.SuspendLogging();
        }

        public static bool IsLoggingEnabled()
        {
            return Instance.IsLoggingEnabled();
        }

        public static void StopLogging()
        {
            while (IsLoggingEnabled())
                SuspendLogging();
        }

        private static LogFactory buildLogFactory()
        {
            var config = new LoggingConfiguration();

#if UNITY_EDITOR || ALTTESTER
            var unityTarget = new UnityTarget("AltDriverUnityTarget")
            {
                Layout = Layout.FromString("${longdate}|Driver|${level:uppercase=true}|${message}"),
            };
            config.AddRuleForOneLevel(LogLevel.Off, unityTarget);
            config.LoggingRules[config.LoggingRules.Count - 1].RuleName = "AltServerUnityRule";
#else
            var consoleTarget = new ConsoleTarget("AltDriverConsoleTarget")
            {
                Layout = Layout.FromString("${longdate}|${level:uppercase=true}|${message}")
            };
            config.AddRuleForOneLevel(LogLevel.Off, consoleTarget);
            config.LoggingRules[config.LoggingRules.Count - 1].RuleName = "AltServerConsoleRule";
#endif

            var logfile = new FileTarget("AltDriverFileTarget")
            {
                FileName = LOGSFILEPATH,
                Layout = Layout.FromString("${longdate}|${level:uppercase=true}|${message}"),
                DeleteOldFileOnStartup = true, //overwrite existing log file.
                KeepFileOpen = true,
                ConcurrentWrites = false
            };
            config.AddRuleForOneLevel(LogLevel.Debug, logfile);
            config.LoggingRules[config.LoggingRules.Count - 1].RuleName = "AltServerFileRule";

            LogFactory logFactory = new LogFactory
            {
                Configuration = config,
                AutoShutdown = true
            };
            return logFactory;
        }
    }
}