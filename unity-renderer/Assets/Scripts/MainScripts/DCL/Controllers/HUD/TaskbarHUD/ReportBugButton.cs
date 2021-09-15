using DCL;
using DCL.Interface;
using MainScripts.DCL.WebPlugin;
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
            
            var os = GetOSInfo();
            var cpu = GetCPUInfo();
            var ram = GetMemoryInfo();
            var gpu = GetGraphicsCardInfo();

            string url = $"{ReportBugURL}" +
                         $"&unity={unityVersion}" +
                         $"&kernel={kernelVersion}" +
                         $"&os={os}" +
                         $"&cpu={cpu}" +
                         $"&ram={ram}" +
                         $"&gpu={gpu}" +
                         $"&nametag={nametag}" +
                         $"&realm={realm}";

            WebInterface.OpenURL(url);
        }
        private string GetOSInfo()
        {
#if UNITY_WEBGL
            return WebGLPlugin.GetUserAgent();
#else
            return SystemInfo.operatingSystem;
#endif
        }

        private string GetCPUInfo()
        {
#if UNITY_WEBGL
            return "";
#else
            return SystemInfo.processorType;
#endif
        }

        private string GetMemoryInfo()
        {
            return $"{SystemInfo.systemMemorySize}MB";
        }

        private string GetGraphicsCardInfo()
        {
            return $"{SystemInfo.graphicsDeviceName} " +
                   $"{SystemInfo.graphicsMemorySize}MB";
        }
    }
}