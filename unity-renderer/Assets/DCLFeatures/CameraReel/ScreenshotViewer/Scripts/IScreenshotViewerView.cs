using System;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public interface IScreenshotViewerView
    {
        event Action CloseButtonClicked;
        event Action PrevScreenshotClicked;
        event Action NextScreenshotClicked;

        void Dispose();
        void Hide();
        void Show();
        void SetScreenshotImage(string url);
        void ToggleInfoSidePanel();
    }
}
