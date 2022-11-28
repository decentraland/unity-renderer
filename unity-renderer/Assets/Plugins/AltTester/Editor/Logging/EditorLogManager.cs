using System;
using Altom.AltDriver.Logging;
using NLog;
using NLog.Layouts;

namespace Altom.AltTesterEditor.Logging
{
    public class EditorLogManager
    {
        public static LogFactory Instance { get { return instance.Value; } }
        private static readonly Lazy<LogFactory> instance = new Lazy<LogFactory>(buildLogFactory);

        private static LogFactory buildLogFactory()
        {
            var config = new NLog.Config.LoggingConfiguration();
            var unitylog = new UnityTarget("AltEditorUnityTarget")
            {
                Layout = Layout.FromString("${longdate}|Editor|${level:uppercase=true}|${message}"),
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, unitylog);

            LogFactory logFactory = new LogFactory
            {
                Configuration = config,
                AutoShutdown = true
            };

            return logFactory;
        }
    }
}