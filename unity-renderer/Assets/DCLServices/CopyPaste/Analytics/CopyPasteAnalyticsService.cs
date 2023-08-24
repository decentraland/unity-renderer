using System.Collections.Generic;

namespace DCLServices.CopyPaste.Analytics
{
    public class CopyPasteAnalyticsService : ICopyPasteAnalyticsService
    {
        private readonly IAnalytics analytics;

        public CopyPasteAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void Dispose() { }

        public void Initialize() { }

        public void Copy(string userId, string element)
        {
            analytics.SendAnalytic("copy_element", new Dictionary<string, string>
            {
                { "wallet_id", userId },
                { "copied_element", element },
            });
        }
    }
}
