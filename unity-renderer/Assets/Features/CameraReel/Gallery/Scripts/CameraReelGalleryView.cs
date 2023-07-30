using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace CameraReel.Gallery
{
    public class CameraReelGalleryView : MonoBehaviour
    {
        private const int LIMIT = 20;
        private readonly LinkedList<CameraReelResponse> reels = new ();

        private readonly Dictionary<int, GridContainerComponentView> monthContainers = new ();

        [SerializeField] internal RectTransform container;
        [SerializeField] private Button showMoreButton;
        [SerializeField] private RectTransform showMoreButtonPanel;

        [Header("RESOURCES")]
        [SerializeField] internal GameObject monthHeaderPrefab;
        [SerializeField] internal GridContainerComponentView monthGridContainerPrefab;
        [SerializeField] internal Image thumbnailPrefab;

        internal GridContainerComponentView currentMonthGridContainer;
        private int offset;

        private ICameraReelNetworkService cameraReelNetworkServiceLazy;
        private ICameraReelNetworkService cameraReelNetworkService => cameraReelNetworkServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelNetworkService>();

        public event Action<CameraReelResponse> ScreenshotThumbnailClicked;
        public event Action<(int current, int max)> ScreenshotsStorageUpdated;

        private void OnEnable()
        {
            Debug.Log("ENABLED");
            showMoreButton.onClick.AddListener(LoadImages);
        }

        private void OnDisable()
        {
            showMoreButton.onClick.RemoveAllListeners();
        }

        private async void LoadImages()
        {
            CameraReelResponses reelImages = await cameraReelNetworkService.GetScreenshotGallery(
                DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

            ScreenshotsStorageUpdated?.Invoke((reelImages.currentImages, reelImages.maxImages));

            offset += LIMIT;

            DownloadImageAndCreateObject(reelImages.images);
        }

        private void DownloadImageAndCreateObject(List<CameraReelResponse> reelImages)
        {
            foreach (CameraReelResponse reel in reelImages)
            {
                reels.AddLast(reel);

                int month = reel.metadata.GetLocalizedDateTime().Month;

                GridContainerComponentView gridContainer;

                if (!monthContainers.ContainsKey(month))
                {
                    GameObject separator = Instantiate(monthHeaderPrefab, container);
                    separator.gameObject.SetActive(true);
                    gridContainer = Instantiate(monthGridContainerPrefab, container);
                    gridContainer.gameObject.SetActive(true);

                    showMoreButtonPanel.SetAsLastSibling();

                    monthContainers.Add(month, gridContainer);
                }
                else
                    gridContainer = monthContainers[month];

                Image image = Instantiate(thumbnailPrefab, gridContainer.transform);
                image.gameObject.SetActive(true);

                Button button = image.GetComponent<Button>();
                button.onClick.AddListener(() => ScreenshotThumbnailClicked?.Invoke(reel));

                SetThumbnailFromWebAsync(reel, image);
            }
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

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;
    }
}
