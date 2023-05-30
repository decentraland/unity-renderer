using DCL.Interface;
using Analytics;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

namespace DCL.HelpAndSupportHUD
{
    public class HelpAndSupportHUDController : IHUD
    {
        internal const string CONTACT_SUPPORT_URL = "https://intercom.decentraland.org";
        internal const string JOIN_DISCORD_URL = "https://dcl.gg/discord";
        internal const string FAQ_URL = "https://docs.decentraland.org/decentraland/faq/";
        public IHelpAndSupportHUDView view {  get; }

        private ISupportAnalytics analytics;

        public HelpAndSupportHUDController(IHelpAndSupportHUDView view, ISupportAnalytics analytics)
        {
            this.view = view;
            this.analytics = analytics;
            view.Initialize();

            view.OnDiscordButtonPressed += OpenDiscord;
            view.OnFaqButtonPressed += OpenFaqs;
            view.OnSupportButtonPressed += OpenSupport;
        }

        public void SetVisibility(bool visible)
        {
            view.SetVisibility(visible);
        }

        private void OpenDiscord()
        {
            OpenURL(JOIN_DISCORD_URL);
        }

        private void OpenFaqs()
        {
            OpenURL(FAQ_URL);
        }

        private void OpenSupport()
        {
            analytics.SendOpenSupport(OpenSupportSource.ExploreHUD);
            OpenIntercom();
        }

        internal void OpenURL(string url)
        {
            WebInterface.OpenURL(url);
        }

        public void Dispose()
        {
            view.OnDiscordButtonPressed -= OpenDiscord;
            view.OnFaqButtonPressed -= OpenFaqs;
            view.OnSupportButtonPressed -= OpenSupport;

            view.Dispose();
        }


#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenIntercom();
#else
        private void OpenIntercom()
        {
            OpenURL(CONTACT_SUPPORT_URL);
        }
#endif
    }
}
