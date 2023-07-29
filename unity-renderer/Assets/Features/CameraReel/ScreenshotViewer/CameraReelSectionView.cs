using CameraReel.ScreenshotViewer;
using DCL;
using DCLServices.CameraReelService;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CameraReelSectionView : MonoBehaviour
{
    private const int LIMIT = 20;

    [Header("GALLERY VIEW")]
    [SerializeField] private Button showMore;
    [SerializeField] internal Image prefab;
    [SerializeField] internal GridContainerComponentView gridContrainer;

    [Space(10)]
    [SerializeField] private ScreenshotViewerHUDView screenshotViewer;

    private int offset;
    private readonly LinkedList<CameraReelResponse> reels = new ();

    private void OnEnable()
    {
        showMore.onClick.AddListener(LoadImages);
        screenshotViewer.PrevScreenshotClicked += LoadPrevScreenshot;
        screenshotViewer.NextScreenshotClicked += LoadNextScreenshot;
    }

    private void OnDisable()
    {
        showMore.onClick.RemoveAllListeners();
        screenshotViewer.PrevScreenshotClicked -= LoadPrevScreenshot;
        screenshotViewer.NextScreenshotClicked -= LoadNextScreenshot;
    }

    private void LoadNextScreenshot(CameraReelResponse current)
    {
        CameraReelResponse next = reels.Find(current)?.Next?.Value;

        if (next != null)
            ShowScreenshotWithMetadata(next);
    }

    private void LoadPrevScreenshot(CameraReelResponse current)
    {
        CameraReelResponse prev = reels.Find(current)?.Previous?.Value;

        if (prev != null)
            ShowScreenshotWithMetadata(prev);
    }

    private async void LoadImages()
    {
        ICameraReelNetworkService cameraReelNetworkService = Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
        CameraReelResponse[] reelImages = await cameraReelNetworkService.GetScreenshotGallery(DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

        offset += LIMIT;

        StartCoroutine(DownloadImageAndCreateObject(reelImages));
    }

    private IEnumerator DownloadImageAndCreateObject(CameraReelResponse[] reelImages)
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

                Image image = Instantiate(prefab, gridContrainer.transform);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.gameObject.SetActive(true);

                Button button = image.GetComponent<Button>();
                button.onClick.AddListener(() => ShowScreenshotWithMetadata(reel));
            }
        }
    }

    private void ShowScreenshotWithMetadata(CameraReelResponse reel)
    {
        screenshotViewer.Show(reel);
    }
}
