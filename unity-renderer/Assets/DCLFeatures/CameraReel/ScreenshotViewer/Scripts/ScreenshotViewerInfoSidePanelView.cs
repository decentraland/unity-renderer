using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI.InWorldCamera.Scripts;
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

        [Header("VISIBLE PEOPLE PANEL")]
        [SerializeField] private ScreenshotVisiblePersonView profileEntryTemplate;
        [SerializeField] private Transform profileGridContainer;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;

        public event Action SceneButtonClicked;
        public event Action SidePanelButtonClicked;

        private void Awake()
        {
            profileEntryTemplate.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            infoPanelTextButton.onClick.AddListener(() => SidePanelButtonClicked?.Invoke());
            sceneInfoButton.onClick.AddListener(() => SceneButtonClicked?.Invoke());
        }

        private void OnDisable()
        {
            infoPanelTextButton.onClick.RemoveAllListeners();
            sceneInfoButton.onClick.RemoveAllListeners();
        }

        public void SetSceneInfoText(Scene scene) =>
            sceneInfo.text = $"{scene.name}, {scene.location.x}, {scene.location.y}";

        public void SetDateText(DateTime dateTime) =>
            dataTime.text = dateTime.ToString("MMMM dd, yyyy");

        public void ShowVisiblePersons(VisiblePerson[] visiblePeople)
        {
            foreach (GameObject profileGameObject in profiles)
                Destroy(profileGameObject);

            profiles.Clear();

            foreach (VisiblePerson visiblePerson in visiblePeople.OrderBy(person => person.isGuest).ThenByDescending(person => person.wearables.Length))
            {
                ScreenshotVisiblePersonView profileEntry = Instantiate(profileEntryTemplate, profileGridContainer);

                profiles.Add(profileEntry.gameObject);
                profileEntry.Configure(visiblePerson);
                profileEntry.gameObject.SetActive(true);
            }
        }
    }
}
