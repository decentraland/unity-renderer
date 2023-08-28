using System.Collections.Generic;

namespace DCLServices.PortableExperiences.Analytics
{
    public class PortableExperiencesAnalyticsService : IPortableExperiencesAnalyticsService
    {
        private readonly IAnalytics analytics;
        private readonly IUserProfileBridge userProfileBridge;

        private string ownUserId => userProfileBridge.GetOwn().userId;

        public PortableExperiencesAnalyticsService(IAnalytics analytics,
            IUserProfileBridge userProfileBridge)
        {
            this.analytics = analytics;
            this.userProfileBridge = userProfileBridge;
        }

        public void Dispose() { }

        public void Initialize() { }

        public void Spawn(string pexId)
        {
            analytics.SendAnalytic("spawn_portable_experience", new Dictionary<string, string>
            {
                { "wallet_id", ownUserId },
                { "pex_urn", pexId },
            });
        }

        public void Accept(string pexId, bool dontAskAgain, string source)
        {
            analytics.SendAnalytic("player_accept_portable_experience", new Dictionary<string, string>
            {
                { "wallet_id", ownUserId },
                { "pex_urn", pexId },
                { "dont_ask_again_flag", dontAskAgain.ToString().ToLower() },
                { "triggered_from", source },
            });
        }

        public void Reject(string pexId, bool dontAskAgain, string source)
        {
            analytics.SendAnalytic("player_reject_portable_experience", new Dictionary<string, string>
            {
                { "wallet_id", ownUserId },
                { "pex_urn", pexId },
                { "dont_ask_again_flag", dontAskAgain.ToString().ToLower() },
                { "triggered_from", source },
            });
        }
    }
}
