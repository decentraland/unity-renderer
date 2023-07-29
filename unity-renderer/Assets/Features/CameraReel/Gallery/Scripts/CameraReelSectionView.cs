using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using DCLServices.CameraReelService;
using UnityEngine;
using UnityEngine.UI;

public class CameraReelSectionView : MonoBehaviour
{
    [SerializeField] private Slider storageBar;

    [SerializeField] private CameraReelGalleryView galleryView;
    [SerializeField] private ScreenshotViewerHUDView screenshotViewer;

    private void Awake()
    {
        storageBar.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        galleryView.ScreenshotsLoaded += UpdateStorageBar;
        galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        screenshotViewer.PrevScreenshotClicked += ShowPrevScreenshot;
        screenshotViewer.NextScreenshotClicked += ShowNextScreenshot;
    }

    private void OnDisable()
    {
        galleryView.ScreenshotsLoaded -= UpdateStorageBar;
        galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;
        screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
        screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
    }

    private void UpdateStorageBar((int current, int max) storage)
    {
        storageBar.value = storage.current;
        storageBar.maxValue = storage.max;;
    }

    private void ShowScreenshotWithMetadata(CameraReelResponse reel)
    {
        screenshotViewer.Show(reel);
    }

    private void ShowNextScreenshot(CameraReelResponse current)
    {
        CameraReelResponse next = galleryView.GetNextScreenshot(current);

        if (next != null)
            ShowScreenshotWithMetadata(next);
    }

    private void ShowPrevScreenshot(CameraReelResponse current)
    {
        CameraReelResponse prev = galleryView.GetPreviousScreenshot(current);

        if (prev != null)
            ShowScreenshotWithMetadata(prev);
    }
}
