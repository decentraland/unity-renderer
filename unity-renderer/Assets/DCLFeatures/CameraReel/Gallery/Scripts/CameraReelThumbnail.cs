using DCLServices.CameraReelService;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelThumbnail : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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

        public int CompareTo(CameraReelThumbnail thumbnail) =>
            picture.metadata.GetLocalizedDateTime().CompareTo(thumbnail.picture.metadata.GetLocalizedDateTime());

        public void OnPointerEnter(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one * 1.05f, 0.3f);
            transform.DORotate(Vector3.forward * -5, 0.3f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one, 0.3f);
            transform.DORotate(Vector3.zero, 0.3f);
        }
    }
}
