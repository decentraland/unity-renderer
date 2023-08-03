using DCL;
using System;
using Object = UnityEngine.Object;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreencaptureCameraHUDController : IDisposable
    {
        private readonly ScreencaptureCameraHUDView view;
        private readonly ScreencaptureCamera screencaptureCamera;

        public ScreencaptureCameraHUDController(ScreencaptureCameraHUDView view, ScreencaptureCamera screencaptureCamera)
        {
            this.view = view;
            this.screencaptureCamera = screencaptureCamera;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += screencaptureCamera.DisableScreenshotCameraMode;
            view.TakeScreenshotButtonClicked += screencaptureCamera.CaptureScreenshot;
            view.ShortcutButtonClicked += view.ToggleShortcutsHelpPanel;
            view.CameraReelButtonClicked += OpenCameraReelGallery;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= screencaptureCamera.DisableScreenshotCameraMode;
            view.TakeScreenshotButtonClicked -= screencaptureCamera.CaptureScreenshot;
            view.ShortcutButtonClicked -= view.ToggleShortcutsHelpPanel;
            view.CameraReelButtonClicked -= OpenCameraReelGallery;

            Object.Destroy(view);
        }

        private void OpenCameraReelGallery()
        {
            screencaptureCamera.DisableScreenshotCameraMode();
            DataStore.i.HUDs.cameraReelSectionVisible.Set(true);
        }
    }
}
