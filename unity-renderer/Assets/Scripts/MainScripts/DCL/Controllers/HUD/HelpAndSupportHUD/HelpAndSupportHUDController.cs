using DCL.Interface;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDController : IHUD
    {
        private const string PATH = "HelpAndSupportHUD";
        private const string VIEW_OBJECT_NAME = "_HelpAndSupportHUD";

        private const string CONTACT_SUPPORT_URL = "https://intercom.decentraland.org";
        private const string JOIN_DISCORD_URL = "https://dcl.gg/discord";
        private const string FAQ_URL = "https://docs.decentraland.org/decentraland/faq/";
        public HelpAndSupportHUDView view { private set; get; }

        public HelpAndSupportHUDController()
        {
            view = HelpAndSupportHUDView.Create(
                PATH,
                VIEW_OBJECT_NAME,
#if UNITY_WEBGL
                () => OpenIntercom(),
#else
                () => OpenURL(CONTACT_SUPPORT_URL),
#endif
                () => OpenURL(JOIN_DISCORD_URL),
                () => OpenURL(FAQ_URL)
                );
        }

        public void SetVisibility(bool visible) { view.SetVisibility(visible); }

        private void OpenURL(string url)
        {
            WebInterface.OpenURL(url);
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenIntercom();
        
#endif

        public void Dispose()
        {
            if (view != null)
                UnityEngine.Object.Destroy(view.gameObject);
        }
    }
}
