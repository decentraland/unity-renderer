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
            var realm = DataStore.i.realm.playerRealm.Get()?.serverName;
            var unityVersion = kernelConfig.rendererVersion;
            var kernelVersion = kernelConfig.kernelVersion;

            var os = UnityWebRequest.EscapeURL(GetOSInfo());
            var cpu = UnityWebRequest.EscapeURL(GetCPUInfo());
            var ram = UnityWebRequest.EscapeURL(GetMemoryInfo());
            var gpu = UnityWebRequest.EscapeURL(GetGraphicsCardInfo());

            string url = $"{ReportBugURL}" +
                         $"&unity={unityVersion}" +
                         $"&kernel={kernelVersion}" +
                         $"&os={os}" +
                         $"&cpu={cpu}" +
                         $"&ram={ram}" +
                         $"&gpu={gpu}" +
                         $"&nametag={nametag}" +
                         $"&realm={realm}" +
                         $"&labels={GetLabels()}";

            WebInterface.OpenURL(url);
        }

        private string GetLabels()
        {
#if UNITY_WEBGL
            return "explorer,new";
#else
            return "explorer-desktop,new";
#endif
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
#if UNITY_WEBGL
            return "";
#else
            return $"{SystemInfo.systemMemorySize}MB";
#endif
        }

        private string GetGraphicsCardInfo()
        {
            return $"{SystemInfo.graphicsDeviceName} " +
                   $"{SystemInfo.graphicsMemorySize}MB";
        }
    }
}