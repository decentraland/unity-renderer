using DCL;
using System.Collections.Generic;

namespace DCLServices.CameraReelService
{
    public class CameraReelAnalyticsService: ICameraReelAnalyticsService
    {
        private const string PLAYER_UPLOAD_SCREENSHOT = "player_upload_screenshot";

        private readonly IAnalytics analytics;

        public CameraReelAnalyticsService(IAnalytics analytics)
        {
            this.analytics = analytics;
        }

        public void SendScreenshotUploaded(string metadataUserAddress, string metadataRealm, string metadataScene, string metadataDateTime, int visiblePeopleLength)
        {
            var data = new Dictionary<string, string>
            {
                {"userAddress", metadataUserAddress},
                {"realm", metadataRealm},
                {"scene", metadataScene},
                {"localDateTime", metadataDateTime},
                {"visiblePeopleAmount", visiblePeopleLength.ToString()},
            };

            analytics.SendAnalytic(PLAYER_UPLOAD_SCREENSHOT, data);
        }

        public void Dispose()
        { }

        public void Initialize()
        { }
    }

    public interface ICameraReelAnalyticsService : IService
    {
        void SendScreenshotUploaded(string metadataUserAddress, string metadataRealm, string metadataScene, string metadataDateTime, int visiblePeopleLength);
    }
}
