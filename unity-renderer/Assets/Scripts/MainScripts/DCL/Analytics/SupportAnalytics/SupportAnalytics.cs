using System.Collections.Generic;
using UnityEngine;

namespace Analytics
{
    public class SupportAnalytics : ISupportAnalytics
    {
        private const string PLAYER_OPENED_SUPPORT = "player_opened_support";

        private readonly IAnalytics analytics;

        public SupportAnalytics(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void SendOpenSupport(OpenSupportSource openSupportSource)
        {
            analytics.SendAnalytic(PLAYER_OPENED_SUPPORT, new Dictionary<string, string>()
            {
                { "source", openSupportSource.ToString() }
            });
        }
    }
}
