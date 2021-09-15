using DCL;
using DCL.Interface;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace MainScripts.DCL.Controllers.HUD.TaskbarHUD
{
    public class ReportBugButton : MonoBehaviour
    {
        private const string ReportBugURL = "https://github.com/decentraland/issues/issues/new?assignees=&labels=new%2Cexplorer&template=bug_report.yml";

        [SerializeField] private Button button;

        private void Awake() { button.onClick.AddListener(ReportBug); }

        private void ReportBug()
        {
            var userProfile = UserProfile.GetOwnUserProfile();
            var nametag = UnityWebRequest.EscapeURL(userProfile.userName);
            var kernelConfig = KernelConfig.i.Get();
            var realm = DataStore.i.playerRealm.Get()?.serverName;
            var unityVersion = kernelConfig.rendererVersion;
            var kernelVersion = kernelConfig.kernelVersion;
            var systemInfo = $"{SystemInfo.processorType}, " +
                             $"{SystemInfo.systemMemorySize}MB RAM, " +
                             $"{SystemInfo.graphicsDeviceName} " +
                             $"{SystemInfo.graphicsMemorySize}MB";

            string url = $"{ReportBugURL}" +
                         $"&unity={unityVersion}" +
                         $"&kernel={kernelVersion}" +
                         $"&nametag={nametag}" +
                         $"&sysinfo={systemInfo}" +
                         $"&realm={realm}";

            WebInterface.OpenURL(url);
        }
    }
}