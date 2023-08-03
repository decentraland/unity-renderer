using DCLServices.CameraReelService;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelThumbnail : MonoBehaviour
    {
        [SerializeField] private ImageComponentView image;
        [SerializeField] private Button interactionButton;
        [SerializeField] private Button contextMenuButton;
        [SerializeField] private ThumbnailContextMenuView contextMenu;

        private CameraReelResponse picture;

        public event Action OnClicked;

        private void Awake()
        {
            interactionButton.onClick.AddListener(() => OnClicked?.Invoke());
            contextMenuButton.onClick.AddListener(() => contextMenu.Show(picture));
        }

        public void Show(CameraReelResponse picture)
        {
            this.picture = picture;
            image.SetImage(picture.thumbnailUrl);
            gameObject.SetActive(true);
        }
    }
}
