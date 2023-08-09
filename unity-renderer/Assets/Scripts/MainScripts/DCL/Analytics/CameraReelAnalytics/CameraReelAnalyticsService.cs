using DCL;
using System;
using System.Collections.Generic;

namespace DCLServices.CameraReelService
{
    public class CameraReelAnalyticsService : ICameraReelAnalyticsService
    {
        private const string PLAYER_UPLOAD_SCREENSHOT = "player_upload_screenshot";
        private const string TWITTER_SHARE = "player_share_twitter";
        private const string OPEN_WEARABLE_MARKETPLACE = "open_wearable_in_marketplace";
        private const string JUMP_IN = "player_jump_in";

        private readonly IAnalytics analytics;

        public CameraReelAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void SendScreenshotUploaded(string metadataUserAddress, string metadataRealm, string metadataScene, string metadataDateTime, int visiblePeopleLength)
        {
            var data = new Dictionary<string, string>
            {
                { "userAddress", metadataUserAddress },
                { "realm", metadataRealm },
                { "scene", metadataScene },
                { "localDateTime", metadataDateTime },
                { "visiblePeopleAmount", visiblePeopleLength.ToString() },
            };

            analytics.SendAnalytic(PLAYER_UPLOAD_SCREENSHOT, data);
        }

        public void ShareOnTwitter()
        {
            var data = new Dictionary<string, string>();

            analytics.SendAnalytic(TWITTER_SHARE, data);
        }

        public void OpenWearableInMarketplace(string source)
        {
            var data = new Dictionary<string, string>
            {
                {"source", source},
            };

            analytics.SendAnalytic(OPEN_WEARABLE_MARKETPLACE, data);
        }

        public void JumpIn(string source)
        {
            var data = new Dictionary<string, string>
            {
                {"source", source},
            };

            analytics.SendAnalytic(JUMP_IN, data);
        }

        public void Dispose() { }

        public void Initialize() { }
    }

    public interface ICameraReelAnalyticsService : IService
    {
        void SendScreenshotUploaded(string metadataUserAddress, string metadataRealm, string metadataScene, string metadataDateTime, int visiblePeopleLength);

        void ShareOnTwitter();

        void OpenWearableInMarketplace(string source);

        void JumpIn(string source);
    }
}
