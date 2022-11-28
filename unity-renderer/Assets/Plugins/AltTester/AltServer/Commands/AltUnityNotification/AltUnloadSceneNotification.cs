using System.Reflection;
using Altom.AltDriver.Notifications;
using Altom.AltTester.Communication;
using UnityEngine.SceneManagement;

namespace Altom.AltTester.Notification
{
    public class AltUnloadSceneNotification : BaseNotification
    {
        public AltUnloadSceneNotification(ICommandHandler commandHandler, bool isOn) : base(commandHandler)
        {
            SceneManager.sceneUnloaded -= onSceneUnloaded;

            if (isOn)
            {
                SceneManager.sceneUnloaded += onSceneUnloaded;
            }
        }

        static void onSceneUnloaded(Scene scene)
        {
            SendNotification(scene.name, "unloadSceneNotification");
        }
    }
}