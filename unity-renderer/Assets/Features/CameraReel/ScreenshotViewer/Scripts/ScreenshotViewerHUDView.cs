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
        private const float SIDE_PANEL_ANIM_DURATION = 0.5f;

        private readonly List<GameObject> profiles = new ();

        [SerializeField] private Image screenshotImage;
        [SerializeField] private RectTransform rootContainer;

        [Header("NAVIGATION BUTTONS")]
        [SerializeField] private Button closeView;

        [Header("ACTIONS PANEL")]
        [SerializeField] internal Button downloadButton;
        [SerializeField] internal Button deleteButton;
        [SerializeField] internal Button linkButton;
        [SerializeField] internal Button twitterButton;
        [SerializeField] internal Button infoButton;
        [SerializeField] internal Button infoPanelTextButton;

        [Header("INFORMATION PANEL")]
        [SerializeField] private TMP_Text dataTime;
        [SerializeField] private TMP_Text sceneInfo;
        [SerializeField] private Button sceneInfoButton;

        [Header("VISIBLE PEOPLE PANEL")]
        [SerializeField] internal ScreenshotVisiblePersonView profileEntryTemplate;
        [SerializeField] internal Transform profileGridContrainer;

        private CameraReelResponse currentScreenshot;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;
        private bool metadataPanelIsOpen = true;

        public void Awake()
        {
            profileEntryTemplate.gameObject.SetActive(false);
            metadataSidePanelAnimator = new MetadataSidePanelAnimator(rootContainer, infoButton.image);
        }

        private void OnEnable()
        {
            closeView.onClick.AddListener(() => gameObject.SetActive(false));

            downloadButton.onClick.AddListener(DownloadImage);
            deleteButton.onClick.AddListener(DeleteImage);
            linkButton.onClick.AddListener(CopyLink);
            twitterButton.onClick.AddListener(CopyTwitterLink);

            infoButton.onClick.AddListener(ToggleMetadataPanel);
            infoPanelTextButton.onClick.AddListener(ToggleMetadataPanel);

            sceneInfoButton.onClick.AddListener(JumpInScene);
        }

        private void OnDisable()
        {
            closeView.onClick.RemoveAllListeners();

            downloadButton.onClick.RemoveAllListeners();
            deleteButton.onClick.RemoveAllListeners();
            linkButton.onClick.RemoveAllListeners();
            twitterButton.onClick.RemoveAllListeners();

            infoButton.onClick.RemoveAllListeners();
            infoPanelTextButton.onClick.RemoveAllListeners();
        }

        private void JumpInScene()
        {
            if (int.TryParse(currentScreenshot.metadata.scene.location.x, out int x) && int.TryParse(currentScreenshot.metadata.scene.location.y, out int y))
                Environment.i.world.teleportController.JumpIn(x, y, currentScreenshot.metadata.realm, string.Empty);
        }

        private void ToggleMetadataPanel()
        {
            metadataSidePanelAnimator.ToggleSizeMode(toFullScreen: metadataPanelIsOpen, SIDE_PANEL_ANIM_DURATION);
            metadataPanelIsOpen = !metadataPanelIsOpen;
        }

        public void Show(CameraReelResponse reel)
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

            foreach (VisiblePerson visiblePerson in reel.metadata.visiblePeople.OrderBy(person => person.isGuest).ThenByDescending(person => person.wearables.Length))
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

        private void DownloadImage()
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
