﻿using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerInfoSidePanelView : MonoBehaviour
    {
        private readonly List<GameObject> profiles = new ();

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
            foreach (GameObject profileGameObject in profiles)
                Destroy(profileGameObject);

            profiles.Clear();

            foreach (VisiblePerson visiblePerson in visiblePeople.OrderBy(person => person.isGuest)
                                                                 .ThenByDescending(person => person.wearables.Length))
            {
                ScreenshotVisiblePersonView profileEntry = Instantiate(profileEntryTemplate, profileGridContainer);

                profiles.Add(profileEntry.gameObject);
                profileEntry.Configure(visiblePerson);
                profileEntry.gameObject.SetActive(true);
            }
        }

        public void SetPictureOwner(string userName, string avatarPictureUrl)
        {
            photoOwnerNameLabel.text = userName;
            photoOwnerAvatarPicture.SetImage(avatarPictureUrl);
        }
    }
}
