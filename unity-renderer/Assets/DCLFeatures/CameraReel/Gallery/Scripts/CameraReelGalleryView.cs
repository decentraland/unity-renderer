using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour, ICameraReelGalleryView
    {
        private readonly SortedDictionary<DateTime, (GridContainerComponentView gridContainer, GameObject headerContainer)> monthContainers = new ();
        private readonly Dictionary<CameraReelResponse, CameraReelThumbnail> thumbnails = new ();
        private readonly Dictionary<CameraReelThumbnail, PoolableObject> thumbnailPoolObjects = new ();
        private readonly Dictionary<DateTime, int> thumbnailsByMonth = new ();
        private readonly List<CameraReelThumbnail> thumbnailSortingBuffer = new ();
        private readonly HashSet<GridContainerComponentView> containersToBeSorted = new ();

        [SerializeField] private RectTransform container;
        [SerializeField] private Button showMoreButton;
        [SerializeField] private RectTransform showMoreButtonPanel;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject emptyStateContainer;
        [SerializeField] private RectTransform scrollMaskTransform;

        [Header("RESOURCES")]
        [SerializeField] private GameObject monthHeaderPrefab;
        [SerializeField] private GridContainerComponentView monthGridContainerPrefab;
        [SerializeField] private CameraReelThumbnail thumbnailPrefab;

        private GridContainerComponentView currentMonthGridContainer;
        private CancellationTokenSource updateLayoutAndSortingCancellationToken;

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
            if (!thumbnails.ContainsKey(reel)) return;

            CameraReelThumbnail thumbnail = thumbnails[reel];
            DateTime month = reel.metadata.GetStartOfTheMonthDate();

            if (thumbnailPoolObjects.ContainsKey(thumbnail))
            {
                thumbnailPoolObjects[thumbnail].Release();
                thumbnailPoolObjects.Remove(thumbnail);
            }

            thumbnails.Remove(reel);

            if (thumbnailsByMonth.ContainsKey(month))
                thumbnailsByMonth[month]--;

            RemoveEmptyMonthContainer(month);

            updateLayoutAndSortingCancellationToken = updateLayoutAndSortingCancellationToken.SafeRestart();
            try { UpdateLayoutOnNextFrame(updateLayoutAndSortingCancellationToken.Token).Forget(); }
            catch (OperationCanceledException) { }
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
            DateTime month = reel.metadata.GetStartOfTheMonthDate();

            GridContainerComponentView gridContainer = GetMonthContainer(month);

            Pool thumbnailPool = GetThumbnailEntryPool();
            PoolableObject thumbnailPoolObj = thumbnailPool.Get();
            CameraReelThumbnail thumbnail = thumbnailPoolObj.gameObject.GetComponent<CameraReelThumbnail>();
            thumbnail.transform.SetParent(gridContainer.transform, false);
            thumbnail.Show(reel);
            thumbnail.OnClicked -= OnThumbnailClicked;
            thumbnail.OnClicked += OnThumbnailClicked;

            if (setAsFirst)
                thumbnail.transform.SetAsFirstSibling();

            thumbnails.Add(reel, thumbnail);
            thumbnailPoolObjects[thumbnail] = thumbnailPoolObj;

            if (!thumbnailsByMonth.ContainsKey(month))
                thumbnailsByMonth[month] = 0;

            thumbnailsByMonth[month]++;

            updateLayoutAndSortingCancellationToken = updateLayoutAndSortingCancellationToken.SafeRestart();

            if (!containersToBeSorted.Contains(gridContainer))
                containersToBeSorted.Add(gridContainer);

            try
            {
                SortThumbnailsByDateOnNextFrame(containersToBeSorted, updateLayoutAndSortingCancellationToken.Token)
                   .ContinueWith(() => containersToBeSorted.Clear())
                   .Forget();
                UpdateLayoutOnNextFrame(updateLayoutAndSortingCancellationToken.Token).Forget();
            }
            catch (OperationCanceledException) { }
        }

        private void OnThumbnailClicked(CameraReelResponse picture) =>
            ScreenshotThumbnailClicked?.Invoke(picture);

        private async UniTask SortThumbnailsByDateOnNextFrame(IEnumerable<GridContainerComponentView> containers, CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken: cancellationToken);

            foreach (GridContainerComponentView container in containers)
                SortThumbnails(container);
        }

        private void SortThumbnails(GridContainerComponentView container)
        {
            lock (thumbnailSortingBuffer)
            {
                thumbnailSortingBuffer.Clear();

                foreach (Transform child in container.transform)
                {
                    CameraReelThumbnail thumbnail = child.GetComponent<CameraReelThumbnail>();
                    if (thumbnail == null) continue;
                    thumbnailSortingBuffer.Add(thumbnail);
                }

                thumbnailSortingBuffer.Sort((t1, t2) => t1.CompareTo(t2));

                foreach (CameraReelThumbnail thumbnail in thumbnailSortingBuffer)
                    thumbnail.transform.SetAsFirstSibling();
            }
        }

        private async UniTaskVoid UpdateLayoutOnNextFrame(CancellationToken cancellationToken)
        {
            await UniTask.NextFrame(cancellationToken: cancellationToken);
            container.ForceUpdateLayout();
        }

        private GridContainerComponentView GetMonthContainer(DateTime date, bool createIfEmpty = true)
        {
            GridContainerComponentView gridContainer;

            if (!monthContainers.ContainsKey(date) && createIfEmpty)
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

        private string GetMonthName(DateTime date) =>
            date.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        private Pool GetThumbnailEntryPool()
        {
            const string POOL_ID = "CameraReelThumbnails";
            var entryPool = PoolManager.i.GetPool(POOL_ID);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                POOL_ID,
                Instantiate(thumbnailPrefab).gameObject,
                maxPrewarmCount: 10,
                isPersistent: true);

            return entryPool;
        }
    }
}
