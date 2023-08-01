using Features.CameraReel.Gallery;
using Features.CameraReel.ScreenshotViewer;
using UnityEngine;

namespace Features.CameraReel.Section
{
    public class CameraReelSectionView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject loadingSpinner;

        [field: SerializeField] public ScreenshotViewerView ScreenshotViewerPrefab { get; private set; }
        [field: SerializeField] public CameraReelGalleryStorageView GalleryStorageView { get; private set; }
        [field: SerializeField] public CameraReelGalleryView GalleryView { get; private set; }

        public void SwitchVisibility(bool isVisible) =>
            canvas.enabled = isVisible;

        public void ShowGalleryWhenLoaded()
        {
            GalleryView.SwitchVisibility(isVisible: true);
            loadingSpinner.SetActive(false);
        }
    }
}
