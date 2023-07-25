using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.CameraReelService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class CameraReelSectionView : MonoBehaviour
{
    private const int LIMIT = 20;
    [Header("Gallery view")]
    [SerializeField] private Button showMore;
    [SerializeField] internal Image prefab;
    [SerializeField] internal GridContainerComponentView gridContrainer;

    [SerializeField] internal ScrollRect scrollView;
    [SerializeField] private Canvas canvas;

    [Header("Screenshot View")]
    [SerializeField] private GameObject screenShotView;
    [SerializeField] private Image screenImage;
    [SerializeField] private Button closeScreenshotView;
    [SerializeField] private TMP_Text dataTime;
    [SerializeField] private TMP_Text sceneInfo;

    [SerializeField] internal GameObject profileCard;
    [SerializeField] internal Transform profileGridContrainer;
    [SerializeField] internal GameObject wearableCard;

    private Canvas gridCanvas;
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

        StartCoroutine(DownloadImageAndCreateObject(reelImages));
    }

    private IEnumerator DownloadImageAndCreateObject(CameraReelResponse[] reelImages)
    {
        foreach (CameraReelResponse reel in reelImages)
        {
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
                button.onClick.AddListener(() => StartCoroutine(ShowScreenshotWithMetadata(reel)));
            }
        }
    }

    private IEnumerator ShowScreenshotWithMetadata(CameraReelResponse reel)
    {
        screenShotView.SetActive(true);

        // Show Screenshot
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.thumbnailUrl);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                sceneInfo.text = $"{reel.metadata.scene.name}, {reel.metadata.scene.location.x}, {reel.metadata.scene.location.y}";

                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                screenImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        // Show DateTime Metadata
        if (long.TryParse(reel.metadata.dateTime, out long unixTimestamp))
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dataTime.text = epoch.AddSeconds(unixTimestamp).ToLocalTime().ToString("MMMM dd, yyyy");
        }

        // Show Visible Persons
        foreach (GameObject p in profiles)
            Destroy(p);
        profiles.Clear();

        var wearablesService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();

        foreach (var person in reel.metadata.visiblePeople)
        {
            GameObject profile = Instantiate(profileCard, profileGridContrainer);
            var button = profile.GetComponentInChildren<ButtonComponentView>();
            button.SetText(person.userName);
            profile.gameObject.SetActive(true);

            profiles.Add(profile);

            FetchWearables(person, wearablesService, profile.gameObject.transform);
        }
    }

    private async void FetchWearables(VisiblePeople person, IWearablesCatalogService wearablesService, Transform parent)
    {
        foreach (string wearable in person.wearables)
        {
            var wearableItem = await wearablesService.RequestWearableAsync(wearable, default(CancellationToken));

            var button = Instantiate(wearableCard, parent).GetComponent<ButtonComponentView>();
            button.SetText(wearableItem.GetName());

            // Show Screenshot
            {
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(wearableItem.ComposeThumbnailUrl());

                await request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                    Debug.Log(request.error);
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                    var sprite  = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    button.SetIcon(sprite);
                }
            }

            button.gameObject.SetActive(true);
        }
    }

    private readonly List<GameObject> profiles = new ();
}
