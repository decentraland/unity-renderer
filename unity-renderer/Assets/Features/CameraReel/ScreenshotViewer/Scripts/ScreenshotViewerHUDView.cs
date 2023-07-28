using Cysharp.Threading.Tasks;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Environment = DCL.Environment;

namespace CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerHUDView : MonoBehaviour
    {
        private readonly List<GameObject> profiles = new ();

        [SerializeField] private Image screenshotImage;

        [Header("NAVIGATION BUTTONS")]
        [SerializeField] private Button closeView;

        [Header("ACTIONS PANEL")]
        [SerializeField] internal Button downloadButton;
        [SerializeField] internal Button deleteButton;
        [SerializeField] internal Button linkButton;
        [SerializeField] internal Button twitterButton;

        [Header("INFORMATION PANEL")]
        [SerializeField] private TMP_Text dataTime;
        [SerializeField] private TMP_Text sceneInfo;

        [Header("VISIBLE PEOPLE PANEL")]
        [SerializeField] internal ScreenshotVisiblePersonView profileEntryTemplate;
        [SerializeField] internal Transform profileGridContrainer;

        private CameraReelResponse currentScreenshot;

        public void Awake()
        {
            profileEntryTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            closeView.onClick.AddListener(() => gameObject.SetActive(false));

            downloadButton.onClick.AddListener(Download);
            deleteButton.onClick.AddListener(DeleteImage);
            linkButton.onClick.AddListener(CopyLink);
            twitterButton.onClick.AddListener(CopyTwitterLink);
        }

        private void OnDisable()
        {
            closeView.onClick.RemoveAllListeners();

            downloadButton.onClick.RemoveAllListeners();
            deleteButton.onClick.RemoveAllListeners();
            linkButton.onClick.RemoveAllListeners();
            twitterButton.onClick.RemoveAllListeners();
        }

        public async void Show(CameraReelResponse reel)
        {
            currentScreenshot = reel;
            gameObject.SetActive(true);

            SetSceneInfoText(reel);
            SetDateText(reel);
            SetScreenshotImage(reel);
            ShowVisiblePersons(reel);
        }

        private void ShowVisiblePersons(CameraReelResponse reel)
        {
            foreach (GameObject profileGameObject in profiles)
                Destroy(profileGameObject);
            profiles.Clear();

            foreach (VisiblePerson visiblePerson in reel.metadata.visiblePeople.OrderBy( person => person.isGuest).ThenByDescending(person => person.wearables.Length))
            {
                ScreenshotVisiblePersonView profileEntry = Instantiate(profileEntryTemplate, profileGridContrainer);

                profiles.Add(profileEntry.gameObject);
                profileEntry.Configure(visiblePerson);
                profileEntry.gameObject.SetActive(true);
            }
        }

        private void SetSceneInfoText(CameraReelResponse reel)
        {
            sceneInfo.text = $"{reel.metadata.scene.name}, {reel.metadata.scene.location.x}, {reel.metadata.scene.location.y}";
        }

        private void SetDateText(CameraReelResponse reel)
        {
            if (!long.TryParse(reel.metadata.dateTime, out long unixTimestamp)) return;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            dataTime.text = epoch.AddSeconds(unixTimestamp).ToLocalTime().ToString("MMMM dd, yyyy");
        }

        private async Task SetScreenshotImage(CameraReelResponse reel)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                screenshotImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
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
    }
}
