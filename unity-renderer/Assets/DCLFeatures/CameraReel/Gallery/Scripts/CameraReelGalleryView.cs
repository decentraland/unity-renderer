using DCL.Helpers;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour
    {
        private readonly SortedDictionary<DateTime, (GridContainerComponentView gridContainer, GameObject headerContainer)> monthContainers = new ();
        private readonly Dictionary<CameraReelResponse, GameObject> screenshotThumbnails = new ();
        private readonly Dictionary<DateTime, int> thumbnailsByMonth = new ();

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

            DateTime date = reel.metadata.GetStartOfTheMonthDate();

            if (thumbnailsByMonth.ContainsKey(date))
                thumbnailsByMonth[date]--;

            RemoveEmptyMonthContainer(date);

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
            DateTime date = reel.metadata.GetStartOfTheMonthDate();

            GridContainerComponentView gridContainer = GetMonthContainer(date);

            CameraReelThumbnail thumbnail = Instantiate(thumbnailPrefab, gridContainer.transform);
            thumbnail.Show(reel);
            thumbnail.OnClicked += () => ScreenshotThumbnailClicked?.Invoke(reel);

            if (setAsFirst)
                thumbnail.transform.SetAsFirstSibling();

            screenshotThumbnails.Add(reel, thumbnail.gameObject);

            if (!thumbnailsByMonth.ContainsKey(date))
                thumbnailsByMonth[date] = 0;
            thumbnailsByMonth[date]++;

            container.ForceUpdateLayout();
        }

        private GridContainerComponentView GetMonthContainer(DateTime date)
        {
            GridContainerComponentView gridContainer;

            if (!monthContainers.ContainsKey(date))
            {
                gridContainer = CreateMonthContainer(date);
                showMoreButtonPanel.SetAsLastSibling();
                SortAllMonthContainers();
            }
            else
                gridContainer = monthContainers[date].gridContainer;

            return gridContainer;
        }

        private GridContainerComponentView CreateMonthContainer(DateTime date)
        {
            GameObject header = Instantiate(monthHeaderPrefab, container);
            header.gameObject.SetActive(true);
            header.GetComponentInChildren<TMP_Text>().SetText(GetMonthName(date));
            GridContainerComponentView gridContainer = Instantiate(monthGridContainerPrefab, container);
            gridContainer.gameObject.SetActive(true);

            monthContainers.Add(date, (gridContainer, header));
            return gridContainer;
        }

        private void SortAllMonthContainers()
        {
            foreach ((DateTime date, (GridContainerComponentView gridContainer, GameObject header)) in monthContainers)
            {
                gridContainer.transform.SetAsFirstSibling();
                header.transform.SetAsFirstSibling();
            }
        }

        private void RemoveEmptyMonthContainer(DateTime date)
        {
            if (!monthContainers.ContainsKey(date)) return;
            if (thumbnailsByMonth.ContainsKey(date) && thumbnailsByMonth[date] > 0) return;

            (GridContainerComponentView gridContainer, GameObject headerContainer) containers = monthContainers[date];
            Destroy(containers.gridContainer);
            Destroy(containers.headerContainer);
            monthContainers.Remove(date);
        }

        private string GetMonthName(DateTime date)
        {
            var month = "";

            switch (date.Month)
            {
                case 1:
                    month = "January";
                    break;
                case 2:
                    month = "February";
                    break;
                case 3:
                    month = "March";
                    break;
                case 4:
                    month = "April";
                    break;
                case 5:
                    month = "May";
                    break;
                case 6:
                    month = "June";
                    break;
                case 7:
                    month = "July";
                    break;
                case 8:
                    month = "August";
                    break;
                case 9:
                    month = "September";
                    break;
                case 10:
                    month = "October";
                    break;
                case 11:
                    month = "November";
                    break;
                case 12:
                    month = "December";
                    break;
            }

            return $"{month} {date.Year}";
        }

    }
}
