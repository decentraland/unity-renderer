using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryStorageView : MonoBehaviour, ICameraReelGalleryStorageView
    {
        [SerializeField] private Slider storageBar;
        [SerializeField] private TMP_Text storageText;
        [SerializeField] private GameObject storageFullMessage;

        public void UpdateStorageBar(int current, int max)
        {
            storageText.text = $"Storage: {current}/{max} photo taken";
            storageText.gameObject.SetActive(true);
            storageFullMessage.SetActive(current >= max);

            storageBar.maxValue = max;
            storageBar.value = current;
            storageBar.gameObject.SetActive(true);
        }

        public void Show() =>
            gameObject.SetActive(true);
    }
}
