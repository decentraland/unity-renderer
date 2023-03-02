namespace DCL.Controllers.HUD
{
    public class MinimapHUDControllerDesktop : MinimapHUDController
    {
        protected override MinimapHUDView CreateView() =>
            MinimapHUDViewDesktop.Create(this);

        public MinimapHUDControllerDesktop(MinimapMetadataController minimapMetadataController, IHomeLocationController locationController, global::DCL.Environment.Model environment)
            : base(minimapMetadataController, locationController, environment) { }

        public MinimapHUDControllerDesktop(MinimapHUDModel model, MinimapMetadataController minimapMetadataController, IHomeLocationController locationController, global::DCL.Environment.Model environment)
            : base(model, minimapMetadataController, locationController, environment) { }
    }
}
