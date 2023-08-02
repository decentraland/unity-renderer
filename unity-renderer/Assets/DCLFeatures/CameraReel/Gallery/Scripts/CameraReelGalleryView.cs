using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour
    {
        private readonly Dictionary<int, GridContainerComponentView> monthContainers = new ();
        private readonly Dictionary<CameraReelResponse, GameObject> screenshotThumbnails = new ();

        [SerializeField] private RectTransform container;
        [SerializeField] private Button showMoreButton;
        [SerializeField] private RectTransform showMoreButtonPanel;
        [SerializeField] private Canvas canvas;

        [Header("RESOURCES")]
        [SerializeField] private GameObject monthHeaderPrefab;
        [SerializeField] private GridContainerComponentView monthGridContainerPrefab;
        [SerializeField] private CameraReelThumbnail thumbnailPrefab;

        private GridContainerComponentView currentMonthGridContainer;

        public event Action<CameraReelResponse> ScreenshotThumbnailClicked;
        public event Action ShowMoreButtonClicked;

        private void Awake()
        {
            SwitchVisibility(isVisible: false);
        }

        private void OnEnable()
        {
            showMoreButton.onClick.AddListener(() => ShowMoreButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            showMoreButton.onClick.RemoveAllListeners();
        }

        public void SwitchVisibility(bool isVisible) =>
            canvas.enabled = isVisible;

        public void AddScreenshotThumbnail(CameraReelResponse reel) =>
            AddScreenshotThumbnail(reel, setAsFirst: true);

        private void AddScreenshotThumbnail(CameraReelResponse reel, bool setAsFirst)
        {
            int month = reel.metadata.GetLocalizedDateTime().Month;

            GridContainerComponentView gridContainer;

            if (!monthContainers.ContainsKey(month))
            {
                GameObject separator = Instantiate(monthHeaderPrefab, container);
                separator.gameObject.SetActive(true);
                gridContainer = Instantiate(monthGridContainerPrefab, container);
                gridContainer.gameObject.SetActive(true);

                if (setAsFirst)
                {
                    gridContainer.transform.SetAsFirstSibling();
                    separator.transform.SetAsFirstSibling();
                }

                showMoreButtonPanel.SetAsLastSibling();

                monthContainers.Add(month, gridContainer);
            }
            else
                gridContainer = monthContainers[month];

            CameraReelThumbnail thumbnail = Instantiate(thumbnailPrefab, gridContainer.transform);
            thumbnail.SetImage(reel.thumbnailUrl);
            thumbnail.OnClicked += () => ScreenshotThumbnailClicked?.Invoke(reel);
            thumbnail.gameObject.SetActive(true);

            if (setAsFirst)
                thumbnail.transform.SetAsFirstSibling();

            screenshotThumbnails.Add(reel, thumbnail.gameObject);
        }

        public void DeleteScreenshotThumbnail(CameraReelResponse reel)
        {
            if (!screenshotThumbnails.ContainsKey(reel)) return;

            Destroy(screenshotThumbnails[reel]);
            screenshotThumbnails.Remove(reel);
        }
    }
}
