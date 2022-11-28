using System;
using Altom.AltDriver.Logging;

namespace Altom.AltDriver.Notifications
{
    public class BaseNotificationCallBacks : INotificationCallbacks
    {
        private static readonly NLog.Logger logger = DriverLogManager.Instance.GetCurrentClassLogger();
        public void SceneLoadedCallback(AltLoadSceneNotificationResultParams altLoadSceneNotificationResultParams)
        {
            logger.Log(NLog.LogLevel.Info, String.Format("Scene {0} was loaded {1}", altLoadSceneNotificationResultParams.sceneName, altLoadSceneNotificationResultParams.loadSceneMode.ToString()));
        }
        public void SceneUnloadedCallback(string sceneName)
        {
            logger.Log(NLog.LogLevel.Info, String.Format("Scene {0} was unloaded", sceneName));
        }
        public void LogCallback(AltLogNotificationResultParams altLogNotificationResultParams)
        {
            logger.Log(NLog.LogLevel.Info, String.Format("Log of type {0} with message {1} was received", altLogNotificationResultParams.level, altLogNotificationResultParams.message));
        }
        public void ApplicationPausedCallback(bool applicationPaused)
        {
            logger.Log(NLog.LogLevel.Info, String.Format("Application paused: {0}", applicationPaused));
        }
    }
}