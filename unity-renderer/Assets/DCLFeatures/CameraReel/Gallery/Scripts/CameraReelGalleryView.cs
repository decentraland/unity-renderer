using DCL.Helpers;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour
    {
        private readonly Dictionary<int, GridContainerComponentView> monthGridContainers = new ();
        private readonly Dictionary<int, GameObject> monthHeaderContainers = new ();
        private readonly Dictionary<CameraReelResponse, GameObject> screenshotThumbnails = new ();
        private readonly Dictionary<int, int> thumbnailsByMonth = new ();

        [SerializeField] private RectTransform container;
        [SerializeField] private Button showMoreButton;
        [SerializeField] private RectTransform showMoreButtonPanel;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject emptyStateContainer;

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

        public void DeleteScreenshotThumbnail(CameraReelResponse reel)
        {
            if (!screenshotThumbnails.ContainsKey(reel)) return;

            Destroy(screenshotThumbnails[reel]);
            screenshotThumbnails.Remove(reel);

            int month = reel.metadata.GetLocalizedDateTime().Month;

            if (thumbnailsByMonth.ContainsKey(month))
                thumbnailsByMonth[month]--;

            RemoveEmptyMonthContainer(month);

            container.ForceUpdateLayout();
        }

        public void SwitchEmptyStateVisibility(bool visible)
        {
            emptyStateContainer.SetActive(visible);
        }

        public void SwitchShowMoreVisibility(bool visible)
        {
            showMoreButtonPanel.gameObject.SetActive(visible);
        }

        public void AddScreenshotThumbnail(CameraReelResponse reel, bool setAsFirst)
        {
            int month = reel.metadata.GetLocalizedDateTime().Month;

            GridContainerComponentView gridContainer = GetMonthContainer(setAsFirst, month);

            CameraReelThumbnail thumbnail = Instantiate(thumbnailPrefab, gridContainer.transform);
            thumbnail.Show(reel);
            thumbnail.OnClicked += () => ScreenshotThumbnailClicked?.Invoke(reel);

            if (setAsFirst)
                thumbnail.transform.SetAsFirstSibling();

            screenshotThumbnails.Add(reel, thumbnail.gameObject);

            if (!thumbnailsByMonth.ContainsKey(month))
                thumbnailsByMonth[month] = 0;
            thumbnailsByMonth[month]++;

            container.ForceUpdateLayout();
        }

        private GridContainerComponentView GetMonthContainer(bool setAsFirst, int month)
        {
            GridContainerComponentView gridContainer;

            if (!monthGridContainers.ContainsKey(month))
            {
                GameObject header = Instantiate(monthHeaderPrefab, container);
                header.gameObject.SetActive(true);
                gridContainer = Instantiate(monthGridContainerPrefab, container);
                gridContainer.gameObject.SetActive(true);

                if (setAsFirst)
                {
                    gridContainer.transform.SetAsFirstSibling();
                    header.transform.SetAsFirstSibling();
                }

                showMoreButtonPanel.SetAsLastSibling();

                monthGridContainers.Add(month, gridContainer);
                monthHeaderContainers.Add(month, header);
            }
            else
                gridContainer = monthGridContainers[month];

            return gridContainer;
        }

        private void RemoveEmptyMonthContainer(int month)
        {
            if (!monthGridContainers.ContainsKey(month)) return;
            if (thumbnailsByMonth.ContainsKey(month) && thumbnailsByMonth[month] > 0) return;

            Destroy(monthHeaderContainers[month]);
            Destroy(monthGridContainers[month]);
            monthHeaderContainers.Remove(month);
            monthGridContainers.Remove(month);
        }
    }
}
