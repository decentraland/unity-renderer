
namespace Altom.AltDriver.Notifications
{
    public class AltLoadSceneNotificationResultParams
    {
        public string sceneName;
        public AltLoadSceneMode loadSceneMode;

        public AltLoadSceneNotificationResultParams(string sceneName, AltLoadSceneMode loadSceneMode)
        {
            this.sceneName = sceneName;
            this.loadSceneMode = loadSceneMode;
        }
    }
}