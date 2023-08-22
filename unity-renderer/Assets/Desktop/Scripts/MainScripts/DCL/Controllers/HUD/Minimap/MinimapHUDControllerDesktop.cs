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
            IClipboard clipboard
            )
            : base(minimapMetadataController, locationController, environment, placesAPIService, placesAnalytics, clipboard) { }
    }
}
