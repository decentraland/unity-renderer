using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] internal GridContainerComponentView monthGridContainer;
        [SerializeField] internal Image thumbnailPrefab;
        [SerializeField] private Button showMore;

        private int offset;

        private ICameraReelNetworkService cameraReelNetworkServiceLazy;
        private ICameraReelNetworkService cameraReelNetworkService => cameraReelNetworkServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelNetworkService>();

        public event Action<CameraReelResponse> ScreenshotThumbnailClicked;
        public event Action<(int current, int max)> ScreenshotsLoaded;

        private void OnEnable()
        {
            showMore.onClick.AddListener(LoadImages);
        }

        private void OnDisable()
        {
            showMore.onClick.RemoveAllListeners();
        }

        private async void LoadImages()
        {
            CameraReelResponses reelImages = await cameraReelNetworkService.GetScreenshotGallery(DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);
            offset += LIMIT;

            ScreenshotsLoaded?.Invoke((reelImages.currentImages, reelImages.maxImages));
            StartCoroutine(DownloadImageAndCreateObject(reelImages.images));
        }

        private IEnumerator DownloadImageAndCreateObject(List<CameraReelResponse> reelImages)
        {
            foreach (CameraReelResponse reel in reelImages)
            {
                reels.AddLast(reel);

                UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.Log(request.error);
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                    Image image = Instantiate(thumbnailPrefab, monthGridContainer.transform);
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    image.gameObject.SetActive(true);

                    Button button = image.GetComponent<Button>();
                    button.onClick.AddListener( () => ScreenshotThumbnailClicked?.Invoke(reel));
                }
            }
        }

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;
    }
}
