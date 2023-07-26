using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.CameraReelService;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private readonly List<GameObject> profiles = new ();
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

    [SerializeField] internal Button downloadButton;
    [SerializeField] internal Button deleteButton;
    [SerializeField] internal Button linkButton;
    [SerializeField] internal Button twitterButton;

    private Canvas gridCanvas;
    private int offset;

    private CameraReelResponse currentScreenshot;

    public void Awake()
    {
        gridCanvas = gridContrainer.GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        showMore.onClick.AddListener(LoadImages);
        closeScreenshotView.onClick.AddListener(() => screenShotView.SetActive(false));
        downloadButton.onClick.AddListener(Download);

        deleteButton.onClick.AddListener(DeleteImage);
        linkButton.onClick.AddListener(CopyLink);
        twitterButton.onClick.AddListener(CopyTwitterLink);
    }

    private void OnDisable()
    {
        showMore.onClick.RemoveAllListeners();
        downloadButton.onClick.RemoveAllListeners();
        closeScreenshotView.onClick.RemoveAllListeners();

        deleteButton.onClick.RemoveAllListeners();
        linkButton.onClick.RemoveAllListeners();
        twitterButton.onClick.RemoveAllListeners();
    }

    private void CopyTwitterLink()
    {
        var description = "Check out what I'm doing in Decentraland right now and join me!";
        var url = $"https://dcl.gg/reels?image={currentScreenshot.id}";
        var twitterUrl = $"https://twitter.com/intent/tweet?text={description}&hashtags=DCLCamera&url={url}";

        GUIUtility.systemCopyBuffer = twitterUrl;
        Application.OpenURL(twitterUrl);
    }

    private void CopyLink()
    {
        var url = $"https://dcl.gg/reels?image={currentScreenshot.id}";

        GUIUtility.systemCopyBuffer = url;
        Application.OpenURL(url);
    }

    private void Download()
    {
        Application.OpenURL(currentScreenshot.url);
    }

    private async void DeleteImage()
    {
        ICameraReelNetworkService cameraReelNetworkService = Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
        await cameraReelNetworkService.DeleteScreenshot(currentScreenshot.id);
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
        currentScreenshot = reel;
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

        IWearablesCatalogService wearablesService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();

        foreach (VisiblePerson person in reel.metadata.visiblePeople)
        {
            GameObject profile = Instantiate(profileCard, profileGridContrainer);
            ButtonComponentView button = profile.GetComponentInChildren<ButtonComponentView>();
            button.SetText(person.userName);
            profile.gameObject.SetActive(true);

            UpdateProfileIcon(person.userAddress, button);

            profiles.Add(profile);

            FetchWearables(person, wearablesService, profile.gameObject.transform);
        }
    }

    private async void UpdateProfileIcon(string userId, ButtonComponentView button)
    {
        UserProfile profile = UserProfileController.userProfilesCatalog.Get(userId) ?? await UserProfileController.i.RequestFullUserProfileAsync(userId);

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(profile.face256SnapshotURL);
        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.Log(request.error);
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            button.SetIcon(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
        }
    }

    private async void FetchWearables(VisiblePerson person, IWearablesCatalogService wearablesService, Transform parent)
    {
        foreach (string wearable in person.wearables)
        {
            WearableItem wearableItem = await wearablesService.RequestWearableAsync(wearable, default(CancellationToken));

            ButtonComponentView button = Instantiate(wearableCard, parent).GetComponent<ButtonComponentView>();
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
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    button.SetIcon(sprite);
                }
            }

            button.gameObject.SetActive(true);
        }
    }
}
