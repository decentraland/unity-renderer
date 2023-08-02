using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelThumbnail : MonoBehaviour
    {
        [SerializeField] private ImageComponentView image;
        [SerializeField] private Button interactionButton;

        public event Action OnClicked;

        private void Awake()
        {
            interactionButton.onClick.AddListener(() => OnClicked?.Invoke());
        }

        public void SetImage(string url) =>
            image.SetImage(url);
    }
}
