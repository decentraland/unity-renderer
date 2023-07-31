using CameraReel.Gallery;
using Features.CameraReel;
using UnityEngine;

public class CameraReelSectionView : MonoBehaviour
{
    [field: SerializeField] public CameraReelGalleryStorageView GalleryStorageView { get; private set;}
    [field: SerializeField] public CameraReelGalleryView GalleryView { get; private set; }

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
