using Cysharp.Threading.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
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
        [SerializeField] private Image thumbnailPrefab;

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

        public void AddScreenshotThumbnails(List<CameraReelResponse> reelImages)
        {
            foreach (CameraReelResponse reel in reelImages)
                AddScreenshotThumbnail(reel, setAsFirst: false);
        }

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

            Image image = Instantiate(thumbnailPrefab, gridContainer.transform);
            image.GetComponent<Button>().onClick.AddListener(() => ScreenshotThumbnailClicked?.Invoke(reel));
            image.gameObject.SetActive(true);

            if (setAsFirst)
                image.transform.SetAsFirstSibling();

            screenshotThumbnails.Add(reel, image.gameObject);

            SetThumbnailFromWebAsync(reel, image);
        }

        public async void DeleteScreenshotThumbnail(CameraReelResponse reel)
        {
            if (!screenshotThumbnails.ContainsKey(reel)) return;

            Destroy(screenshotThumbnails[reel]);
            screenshotThumbnails.Remove(reel);
        }

        private static async Task SetThumbnailFromWebAsync(CameraReelResponse reel, Image image)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
}
