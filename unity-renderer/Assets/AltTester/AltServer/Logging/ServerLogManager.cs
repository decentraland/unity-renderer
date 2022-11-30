using System;
using System.Collections.Generic;
using Altom.AltDriver.Logging;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Altom.AltTester.Logging
{
    public class ServerLogManager
    {
        public static LogFactory Instance { get { return instance.Value; } }

        private static readonly Lazy<LogFactory> instance = new Lazy<LogFactory>(buildLogFactory);
        private static string logsFilePath = null;

        public static void SetupAltServerLogging(Dictionary<AltLogger, AltLogLevel> minLogLevels)
        {
            foreach (var key in minLogLevels.Keys)
            {
                SetMinLogLevel(key, minLogLevels[key]);
            }

            Instance.GetCurrentClassLogger().Info(AltLogLevel.Info.ToNLogLevel());
            AltLogLevel level;
            if (!string.IsNullOrEmpty(logsFilePath) && minLogLevels.TryGetValue(AltLogger.File, out level) && level != AltLogLevel.Off)
                Instance.GetCurrentClassLogger().Info("AltTester logs are saved at: " + logsFilePath);
        }


        /// <summary>
        /// Reconfigures the NLog logging level.
        /// </summary>
        /// <param name="minLogLevel">The <see cref="AltLogLevel" /> to be set.</param>
        public static void SetMinLogLevel(AltLogger loggerType, AltLogLevel minLogLevel)
        {
            LogLevel minLevel, maxLevel;
            if (minLogLevel == AltLogLevel.Off)
            {
                minLevel = LogLevel.Off;
                maxLevel = LogLevel.Off;
            }
            else
            {
                minLevel = minLogLevel.ToNLogLevel();
                maxLevel = LogLevel.Fatal;
            }
            bool found = false;
            foreach (var rule in Instance.Configuration.LoggingRules)
            {
                if (rule.Targets[0].Name == string.Format("AltServer{0}Target", loggerType))
                {
                    found = true;
                    rule.SetLoggingLevels(minLevel, maxLevel);
                    rule.RuleName = string.Format("AltServer{0}Rule", loggerType);
                }
            }
            if (!found && loggerType == AltLogger.File)
            {
                addFileLogger(minLevel, maxLevel);
            }

            Instance.ReconfigExistingLoggers();
        }

        private static void addFileLogger(LogLevel minLevel, LogLevel maxLevel)
        {
            logsFilePath = UnityEngine.Application.persistentDataPath + "/AltTester-Server.log";
            var logfile = new FileTarget("AltServerFileTarget")
            {
                FileName = logsFilePath,
                Layout = Layout.FromString("${longdate}|${level:uppercase=true}|${message}"),
                DeleteOldFileOnStartup = true, //overwrite existing log file.
                KeepFileOpen = true,
                ConcurrentWrites = false
            };
            Instance.Configuration.AddRule(minLevel, maxLevel, logfile);
            Instance.Configuration.LoggingRules[Instance.Configuration.LoggingRules.Count - 1].RuleName = "AltServerFileRule";
        }

        private static LogFactory buildLogFactory()
        {
            var config = new LoggingConfiguration();

#if UNITY_EDITOR || ALTTESTER
            var unitylog = new UnityTarget("AltServerUnityTarget")
            {
                Layout = Layout.FromString("${longdate}|Tester|${level:uppercase=true}|${message}"),
            };
            config.AddRuleForOneLevel(LogLevel.Off, unitylog);
            config.LoggingRules[config.LoggingRules.Count - 1].RuleName = "AltServerUnityRule";
#else
            var consoleTarget = new ConsoleTarget("AltDriverConsoleTarget")
            {
                Layout = Layout.FromString("${longdate}|${level:uppercase=true}|${message}"),
            };
            config.AddRuleForOneLevel(LogLevel.Off, consoleTarget);
            config.LoggingRules[config.LoggingRules.Count - 1].RuleName = "AltServerConsoleRule";
#endif

            LogFactory logFactory = new LogFactory
            {
                Configuration = config,
                AutoShutdown = true
            };
            return logFactory;
        }
    }
}