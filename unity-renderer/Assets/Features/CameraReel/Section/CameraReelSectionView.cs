using CameraReel.Gallery;
using Features.CameraReel;
using Features.CameraReel.ScreenshotViewer;
using UnityEngine;

public class CameraReelSectionView : MonoBehaviour
{
    [field: SerializeField] public ScreenshotViewerHUDView ScreenshotViewerPrefab { get; private set; }
    [field: SerializeField] public CameraReelGalleryStorageView GalleryStorageView { get; private set;}
    [field: SerializeField] public CameraReelGalleryView GalleryView { get; private set; }

    [Space]
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject loadingSpinner;

    private Canvas galleryCanvas;

    private void Awake()
    {
        galleryCanvas = GalleryView.GetComponent<Canvas>();
        galleryCanvas.enabled = false;
    }

    public void SwitchVisibility(bool isVisible) =>
        canvas.enabled = isVisible;

    public void ShowGalleryWhenLoaded()
    {
        galleryCanvas.enabled = true;
        loadingSpinner.SetActive(false);
    }
}
