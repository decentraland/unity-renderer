using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLServices.CameraReelService;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraReelSectionView : MonoBehaviour
{
    private const string ADDRESS = "CameraReelSectionView";

    [SerializeField] private Slider storageBar;
    [SerializeField] private TMP_Text storageText;

    [SerializeField] private CameraReelGalleryView galleryView;
    [SerializeField] private ScreenshotViewerHUDView screenshotViewerPrefab;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject loadingSpinner;

    private ScreenshotViewerHUDView screenshotViewer;
    private Canvas galleryCanvas;

    private void Awake()
    {
        storageBar.gameObject.SetActive(false);
        storageText.gameObject.SetActive(false);

        galleryCanvas = galleryView.GetComponent<Canvas>();
        galleryCanvas.enabled = false;
    }

    private void OnEnable()
    {
        DataStore.i.HUDs.cameraReelVisible.OnChange += SwitchVisibility;

        galleryView.ScreenshotsStorageUpdated += UpdateStorageBar;
        galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
    }

    private void OnDisable()
    {
        DataStore.i.HUDs.cameraReelVisible.OnChange -= SwitchVisibility;

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

    public static async UniTask<CameraReelSectionView> Create(IAddressableResourceProvider assetProvider) =>
        await assetProvider.Instantiate<CameraReelSectionView>(ADDRESS, ADDRESS, DataStore.i.exploreV2.configureCameraReelInFullScreenMenu.Get());

    private bool firstLoad = true;
    private void SwitchVisibility(bool isVisible, bool _)
    {
        canvas.enabled = isVisible;

        if (firstLoad)
        {
            galleryView.LoadImagesAsync();
            galleryView.ScreenshotsStorageUpdated += ShowGalleryWhenLoaded;
            firstLoad = false;
        }
    }

    private void ShowGalleryWhenLoaded((int current, int max) obj)
    {
        galleryCanvas.enabled = true;
        loadingSpinner.SetActive(false);
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
