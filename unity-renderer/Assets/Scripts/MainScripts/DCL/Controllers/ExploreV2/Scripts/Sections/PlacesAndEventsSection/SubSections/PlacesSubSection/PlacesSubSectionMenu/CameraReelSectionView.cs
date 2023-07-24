using DCL;
using DCLServices.CameraReelService;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class CameraReelSectionView : MonoBehaviour
{
    [Header("Prefab References")]
    [SerializeField] internal Image prefab;

    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] internal GridContainerComponentView gridContrainer;
    [SerializeField] private Canvas canvas;

    [SerializeField] private Button showMore;

    private Canvas gridCanvas;

    private const int LIMIT = 20;
    private int offset;

    public void Awake()
    {
        gridCanvas = gridContrainer.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        showMore.onClick.AddListener(LoadImages);
    }

    private void OnDisable()
    {
        showMore.onClick.RemoveListener(LoadImages);
    }

    private async void LoadImages()
    {
        ICameraReelNetworkService cameraReelNetworkService = Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
        var reelImages = await cameraReelNetworkService.GetScreenshotGallery(DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);
        offset += LIMIT;

        foreach (var reel in reelImages)
            StartCoroutine(DownloadImageAndCreateObject(reel.thumbnailUrl));
    }

    private IEnumerator DownloadImageAndCreateObject(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if(request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture) request.downloadHandler).texture;

            Image image = Instantiate(prefab, gridContrainer.transform);

            // Convert Texture2D to Sprite
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
    }
}
