using Cysharp.Threading.Tasks;
using DCL;
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

namespace Features.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerHUDView : MonoBehaviour
    {
        private const float SIDE_PANEL_ANIM_DURATION = 0.5f;

        private readonly List<GameObject> profiles = new ();

        [SerializeField] private Image screenshotImage;
        [SerializeField] private RectTransform rootContainer;

        [Header("NAVIGATION BUTTONS")]
        [SerializeField] private Button closeView;
        [SerializeField] private Button prevScreenshotButton;
        [SerializeField] private Button nextScreenshotButton;

        [Header("INFORMATION PANEL")]
        [SerializeField] internal Button infoPanelTextButton;
        [SerializeField] private TMP_Text dataTime;
        [SerializeField] private TMP_Text sceneInfo;
        [SerializeField] private Button sceneInfoButton;

        [Header("VISIBLE PEOPLE PANEL")]
        [SerializeField] internal ScreenshotVisiblePersonView profileEntryTemplate;
        [SerializeField] internal Transform profileGridContainer;

        public CameraReelResponse currentScreenshot;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;
        private bool metadataPanelIsOpen = true;
        [field: SerializeField] public ScreenshotViewerActionsPanelView ActionPanel { get; private set; }

        public event Action<CameraReelResponse> PrevScreenshotClicked;
        public event Action<CameraReelResponse> NextScreenshotClicked;

        public void Awake()
        {
            profileEntryTemplate.gameObject.SetActive(false);
            metadataSidePanelAnimator = new MetadataSidePanelAnimator(rootContainer, ActionPanel.InfoButtonBackground);
        }

        private void OnEnable()
        {
            closeView.onClick.AddListener(Hide);

            infoPanelTextButton.onClick.AddListener(ToggleInfoPanel);

            sceneInfoButton.onClick.AddListener(JumpInScene);

            prevScreenshotButton.onClick.AddListener(() => PrevScreenshotClicked?.Invoke(currentScreenshot));
            nextScreenshotButton.onClick.AddListener(() => NextScreenshotClicked?.Invoke(currentScreenshot));
        }

        private void OnDisable()
        {
            closeView.onClick.RemoveAllListeners();

            infoPanelTextButton.onClick.RemoveAllListeners();

            prevScreenshotButton.onClick.RemoveAllListeners();
            nextScreenshotButton.onClick.RemoveAllListeners();
        }

        private void JumpInScene()
        {
            if (int.TryParse(currentScreenshot.metadata.scene.location.x, out int x) && int.TryParse(currentScreenshot.metadata.scene.location.y, out int y))
            {
                Environment.i.world.teleportController.JumpIn(x, y, currentScreenshot.metadata.realm, string.Empty);
                Hide();
                DataStore.i.exploreV2.isOpen.Set(false);
            }
        }

        public void ToggleInfoPanel()
        {
            metadataSidePanelAnimator.ToggleSizeMode(toFullScreen: metadataPanelIsOpen, SIDE_PANEL_ANIM_DURATION);
            metadataPanelIsOpen = !metadataPanelIsOpen;
        }

        private void Hide()
        {
            gameObject.SetActive(false);
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
                ScreenshotVisiblePersonView profileEntry = Instantiate(profileEntryTemplate, profileGridContainer);

                profiles.Add(profileEntry.gameObject);
                profileEntry.Configure(visiblePerson);
                profileEntry.gameObject.SetActive(true);
            }
        }

        private void SetSceneInfoText(CameraReelResponse reel)
        {
            sceneInfo.text = $"{reel.metadata.scene.name}, {reel.metadata.scene.location.x}, {reel.metadata.scene.location.y}";
        }

        private void SetDateText(CameraReelResponse reel) =>
            dataTime.text = reel.metadata.GetLocalizedDateTime().ToString("MMMM dd, yyyy");

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
    }
}
