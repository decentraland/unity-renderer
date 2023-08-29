using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerInfoSidePanelView : MonoBehaviour, IScreenshotViewerInfoSidePanelView
    {
        private readonly Dictionary<ScreenshotVisiblePersonView, PoolableObject> profiles = new ();

        [Header("INFORMATION PANEL")]
        [SerializeField] private Button infoPanelTextButton;
        [SerializeField] private TMP_Text dataTime;
        [SerializeField] private TMP_Text sceneInfo;
        [SerializeField] private Button sceneInfoButton;
        [SerializeField] private TMP_Text photoOwnerNameLabel;
        [SerializeField] private ImageComponentView photoOwnerAvatarPicture;
        [SerializeField] private Button pictureOwnerProfileButton;

        [Header("VISIBLE PEOPLE PANEL")]
        [SerializeField] private ScreenshotVisiblePersonView profileEntryTemplate;
        [SerializeField] private Transform profileGridContainer;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;

        public event Action SceneButtonClicked;
        public event Action SidePanelButtonClicked;
        public event Action OnOpenPictureOwnerProfile;

        private void Awake()
        {
            profileEntryTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            infoPanelTextButton.onClick.AddListener(() => SidePanelButtonClicked?.Invoke());
            sceneInfoButton.onClick.AddListener(() => SceneButtonClicked?.Invoke());
            pictureOwnerProfileButton.onClick.AddListener(() => OnOpenPictureOwnerProfile?.Invoke());
        }

        private void OnDisable()
        {
            infoPanelTextButton.onClick.RemoveAllListeners();
            sceneInfoButton.onClick.RemoveAllListeners();
            pictureOwnerProfileButton.onClick.RemoveAllListeners();
        }

        public void SetSceneInfoText(Scene scene) =>
            sceneInfo.text = $"{scene.name}, {scene.location.x}, {scene.location.y}";

        public void SetDateText(DateTime dateTime) =>
            dataTime.text = dateTime.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture);

        public void ShowVisiblePersons(VisiblePerson[] visiblePeople)
        {
            ClearCurrentProfiles();

            Pool profilePool = GetProfileEntryPool();

            foreach (VisiblePerson visiblePerson in visiblePeople.OrderBy(person => person.isGuest)
                                                                 .ThenByDescending(person => person.wearables.Length))
            {
                PoolableObject poolObj = profilePool.Get();
                ScreenshotVisiblePersonView profileEntry = poolObj.gameObject.GetComponent<ScreenshotVisiblePersonView>();
                profileEntry.transform.SetParent(profileGridContainer, false);
                profiles[profileEntry] = poolObj;
                profileEntry.Configure(visiblePerson);
                profileEntry.gameObject.SetActive(true);
            }
        }

        public void SetPictureOwner(string userName, string avatarPictureUrl)
        {
            photoOwnerNameLabel.text = userName;
            photoOwnerAvatarPicture.SetImage(avatarPictureUrl);
        }

        private void ClearCurrentProfiles()
        {
            foreach ((_, PoolableObject poolObj) in profiles)
                poolObj.Release();

            profiles.Clear();
        }

        private Pool GetProfileEntryPool()
        {
            const string POOL_ID = "PictureDetailProfile";
            var entryPool = PoolManager.i.GetPool(POOL_ID);
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                POOL_ID,
                Instantiate(profileEntryTemplate).gameObject,
                maxPrewarmCount: 10,
                isPersistent: true);

            return entryPool;
        }
    }
}
