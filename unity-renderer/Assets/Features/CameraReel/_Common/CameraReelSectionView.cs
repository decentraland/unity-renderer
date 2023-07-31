using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using DCL;
using DCLServices.CameraReelService;
using Features.CameraReel;
using UnityEngine;

public class CameraReelSectionView : MonoBehaviour
{
    [field: SerializeField] public CameraReelGalleryStorageView GalleryStorageView { get; private set;}
    [field: SerializeField] public CameraReelGalleryView GalleryView { get; private set; }

    [SerializeField] private ScreenshotViewerHUDView screenshotViewerPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject loadingSpinner;

    private ScreenshotViewerHUDView screenshotViewer;
    private Canvas galleryCanvas;

    private void Awake()
    {
        galleryCanvas = GalleryView.GetComponent<Canvas>();
        galleryCanvas.enabled = false;
    }

    private void OnEnable()
    {
        DataStore.i.HUDs.cameraReelVisible.OnChange += SwitchVisibility;
        GalleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
    }

    private void OnDisable()
    {
        DataStore.i.HUDs.cameraReelVisible.OnChange -= SwitchVisibility;
        GalleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;
    }

    private void OnDestroy()
    {
        if (screenshotViewer != null)
        {
            screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
            screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
        }
    }

    private bool firstLoad = true;
    private void SwitchVisibility(bool isVisible, bool _)
    {
        canvas.enabled = isVisible;

        if (firstLoad)
        {
            GalleryView.LoadImagesAsync();
            GalleryView.ScreenshotsStorageUpdated += ShowGalleryWhenLoaded;
            firstLoad = false;
        }
    }

    private void ShowGalleryWhenLoaded((int current, int max) obj)
    {
        galleryCanvas.enabled = true;
        loadingSpinner.SetActive(false);
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
        CameraReelResponse next = GalleryView.GetNextScreenshot(current);

        if (next != null)
            ShowScreenshotWithMetadata(next);
    }

    private void ShowPrevScreenshot(CameraReelResponse current)
    {
        CameraReelResponse prev = GalleryView.GetPreviousScreenshot(current);

        if (prev != null)
            ShowScreenshotWithMetadata(prev);
    }
}
