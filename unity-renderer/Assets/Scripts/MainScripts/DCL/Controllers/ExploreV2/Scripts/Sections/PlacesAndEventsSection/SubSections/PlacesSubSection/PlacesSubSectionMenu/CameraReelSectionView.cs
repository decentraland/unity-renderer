using DCL;
using DCL.Helpers;
using DCLServices.CameraReelService;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class CameraReelSectionView : MonoBehaviour
{
    [Header("Gallery view")]
    [SerializeField] private Button showMore;
    [SerializeField] internal Image prefab;
    [SerializeField] internal GridContainerComponentView gridContrainer;

    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] private Canvas canvas;
    private Canvas gridCanvas;

    [Header("Screenshot View")]
    [SerializeField] private GameObject screenShotView;
    [SerializeField] private Image screenImage;
    [SerializeField] private Button closeScreenshotView;
    [SerializeField] private TMP_Text dataTime;
    [SerializeField] private TMP_Text sceneInfo;

    [SerializeField] internal GameObject profileCard;
    [SerializeField] internal GridContainerComponentView profileGridContrainer;

    private const int LIMIT = 20;
    private int offset;

    public void Awake()
    {
        gridCanvas = gridContrainer.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        showMore.onClick.AddListener(LoadImages);
        closeScreenshotView.onClick.AddListener(() => screenShotView.SetActive(false));
    }

    private void OnDisable()
    {
        showMore.onClick.RemoveListener(LoadImages);
    }

    private async void LoadImages()
    {
        ICameraReelNetworkService cameraReelNetworkService = Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
        CameraReelResponse[] reelImages = await cameraReelNetworkService.GetScreenshotGallery(DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

        offset += LIMIT;

        foreach (var reel in reelImages)
            StartCoroutine(DownloadImageAndCreateObject(reel));
    }

    private IEnumerator DownloadImageAndCreateObject(CameraReelResponse reel)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
        yield return request.SendWebRequest();
        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

            Image image = Instantiate(prefab, gridContrainer.transform);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.gameObject.SetActive(true);

            var button = image.GetComponent<Button>();
            button.onClick.AddListener(() => StartCoroutine(ShowScreenshotWithMetadata(reel)));
        }
    }

    private IEnumerator ShowScreenshotWithMetadata(CameraReelResponse reel)
    {
        screenShotView.SetActive(true);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
        yield return request.SendWebRequest();

        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
            screenImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            dataTime.text = Utils.UnixTimeStampToLocalTime(ulong.Parse(reel.metadata.dateTime));
            sceneInfo.text = $"{reel.metadata.scene.name}, {reel.metadata.scene.location.x}, {reel.metadata.scene.location.y}";
        }
    }
}
