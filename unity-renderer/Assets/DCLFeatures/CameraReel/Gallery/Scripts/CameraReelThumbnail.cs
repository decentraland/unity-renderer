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
        [SerializeField] private Image flashImage;
        [SerializeField] private Button interactionButton;
        [SerializeField] private Button contextMenuButton;
        [SerializeField] private ThumbnailContextMenuView contextMenu;
        [SerializeField] private Canvas canvas;

        private CameraReelResponse picture;
        private int canvasOriginalSortingOrder;

        public event Action<CameraReelResponse> OnClicked;

        private void Awake()
        {
            interactionButton.onClick.AddListener(() => OnClicked?.Invoke(picture));
            contextMenuButton.onClick.AddListener(() => contextMenu.Show(picture));
            canvasOriginalSortingOrder = canvas.sortingOrder;

            image.OnLoaded += _ =>
            {
                flashImage.color = Color.white;
                flashImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
                AudioScriptableObjects.listItemAppear.Play(true);
            };
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
            canvas.sortingOrder++;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            transform.DOScale(Vector3.one, 0.3f);
            canvas.sortingOrder = canvasOriginalSortingOrder;
        }
    }
}
