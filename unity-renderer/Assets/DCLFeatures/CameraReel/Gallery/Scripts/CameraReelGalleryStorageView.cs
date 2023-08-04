using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryStorageView : MonoBehaviour
    {
        [SerializeField] private Slider storageBar;
        [SerializeField] private TMP_Text storageText;

        private void Awake()
        {
            storageBar.gameObject.SetActive(false);
            storageText.gameObject.SetActive(false);
        }

        public void UpdateStorageBar(int current, int max)
        {
            storageText.text = $"Storage: {current}/{max} photo taken";
            storageText.gameObject.SetActive(true);

            storageBar.maxValue = max;
            storageBar.value = current;
            storageBar.gameObject.SetActive(true);
        }
    }
}
