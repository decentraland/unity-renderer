using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using DCLServices.CameraReelService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraReelSectionView : MonoBehaviour
{
    [SerializeField] private Slider storageBar;
    [SerializeField] private TMP_Text storageText;

    [SerializeField] private CameraReelGalleryView galleryView;
    [SerializeField] private ScreenshotViewerHUDView screenshotViewerPrefab;

    private ScreenshotViewerHUDView screenshotViewer;

    private void Awake()
    {
        storageBar.gameObject.SetActive(false);
        storageText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        galleryView.ScreenshotsStorageUpdated += UpdateStorageBar;
        galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
    }

    private void OnDisable()
    {
        galleryView.ScreenshotsStorageUpdated -= UpdateStorageBar;
        galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;
    }

    private void OnDestroy()
    {
        if (screenshotViewer != null)
        {
            screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
            screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
        }
    }

    private void UpdateStorageBar((int current, int max) storage)
    {
        storageText.text = $"Storage: {storage.current}/{storage.max} photo taken";
        storageText.gameObject.SetActive(true);

        storageBar.maxValue = storage.max;
        storageBar.value = storage.current;
        storageBar.gameObject.SetActive(true);
    }

    private void ShowScreenshotWithMetadata(CameraReelResponse reel)
    {
        if (screenshotViewer == null)
        {
            screenshotViewer = Instantiate(screenshotViewerPrefab, null);
            screenshotViewer.PrevScreenshotClicked += ShowPrevScreenshot;
            screenshotViewer.NextScreenshotClicked += ShowNextScreenshot;
        }

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
