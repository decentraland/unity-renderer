using DCLServices.CameraReelService;
using System;
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
        var response1 = await cameraReelNetworkService.GetScreenshotGallery("0x05de05303eab867d51854e8b4fe03f7acb0624d9");

        foreach (var reel in response1)
        {
            StartCoroutine(DownloadImageAndCreateObject(reel.url));
        }
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
