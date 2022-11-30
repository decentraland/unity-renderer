
namespace Altom.AltDriver.Notifications
{
    public interface INotificationCallbacks
    {
        void SceneLoadedCallback(AltLoadSceneNotificationResultParams altLoadSceneNotificationResultParams);
        void SceneUnloadedCallback(string sceneName);
        void LogCallback(AltLogNotificationResultParams altLogNotificationResultParams);
        void ApplicationPausedCallback(bool applicationPaused);
    }
}