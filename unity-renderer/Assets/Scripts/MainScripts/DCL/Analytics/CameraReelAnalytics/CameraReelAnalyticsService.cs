using DCL;
using System;
using System.Collections.Generic;

namespace DCLServices.CameraReelService
{
    public class CameraReelAnalyticsService : ICameraReelAnalyticsService
    {
        private const string TAKE_PHOTO = "take_photo";
        private const string TWITTER_SHARE = "share_twitter";
        private const string OPEN_WEARABLE_MARKETPLACE = "open_wearable_in_marketplace";
        private const string JUMP_IN = "jump_in";

        private readonly IAnalytics analytics;

        public CameraReelAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void TakePhoto(string userId, string location, int visiblePeopleLength, string source)
        {
            var data = new Dictionary<string, string>
            {
                { "player", userId },
                { "location", location },
                { "visiblePeopleAmount", visiblePeopleLength.ToString() },
                { "source", source },
            };

            analytics.SendAnalytic(TAKE_PHOTO, data);
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

        public void DownloadPhoto(string source)
        {
            var data = new Dictionary<string, string>
            {
                {"source", source},
            };

            analytics.SendAnalytic("photo_download", data);
        }

        public void Dispose() { }

        public void Initialize() { }
    }

    public interface ICameraReelAnalyticsService : IService
    {
        void TakePhoto(string userId, string location, int visiblePeopleLength, string source);

        void ShareOnTwitter();

        void OpenWearableInMarketplace(string source);

        void JumpIn(string source);

        void DownloadPhoto(string source);
    }
}
