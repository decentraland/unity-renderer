using System.Collections.Generic;

namespace DCLServices.CopyPaste.Analytics
{
    public class CopyPasteAnalyticsService : ICopyPasteAnalyticsService
    {
        private readonly IAnalytics analytics;
        private readonly IUserProfileBridge userProfileBridge;

        public CopyPasteAnalyticsService(IAnalytics analytics,
            IUserProfileBridge userProfileBridge)
        {
            this.analytics = analytics;
            this.userProfileBridge = userProfileBridge;
        }

        public void Dispose() { }

        public void Initialize() { }

        public void Copy(string element)
        {
            analytics.SendAnalytic("copy_element", new Dictionary<string, string>
            {
                { "wallet_id", userProfileBridge.GetOwn().userId },
                { "copied_element", element },
            });
        }
    }
}
