using System.Collections.Generic;
using UnityEngine;

namespace SupportAnalytics
{
    public class SupportAnalytics : ISupportAnalytics
    {
        private const string PLAYER_OPENED_SUPPORT = "player_opened_support";

        public static SupportAnalytics i { get; private set; }

        private readonly IAnalytics analytics;

        public SupportAnalytics(IAnalytics analytics)
        {
            this.analytics = analytics;
            i ??= this;
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
