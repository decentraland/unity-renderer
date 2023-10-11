using DCLServices.CopyPaste.Analytics;
using DCLServices.PlacesAPIService;

namespace DCL.Controllers.HUD
{
    public class MinimapHUDControllerDesktop : MinimapHUDController
    {
        protected override MinimapHUDView CreateView() =>
            MinimapHUDViewDesktop.Create(this);

        public MinimapHUDControllerDesktop(
            MinimapMetadataController minimapMetadataController,
            IHomeLocationController locationController,
            Environment.Model environment,
            IPlacesAPIService placesAPIService,
            IPlacesAnalytics placesAnalytics,
            IClipboard clipboard,
            ICopyPasteAnalyticsService copyPasteAnalyticsService,
            DataStore_ContentModeration contentModerationDataStore,
            IWorldState worldState
        ) : base(minimapMetadataController, locationController, environment, placesAPIService, placesAnalytics, clipboard,
            copyPasteAnalyticsService, contentModerationDataStore, worldState) { }
    }
}
