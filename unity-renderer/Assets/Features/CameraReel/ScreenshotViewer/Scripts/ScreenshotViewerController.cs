namespace Features.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerController
    {
        private readonly ScreenshotViewerHUDView view;
        private readonly CameraReelModel model;

        public ScreenshotViewerController(ScreenshotViewerHUDView view, CameraReelModel model)
        {
            this.view = view;
            this.model = model;
        }

        public void Initialize()
        {

        }
    }
}
