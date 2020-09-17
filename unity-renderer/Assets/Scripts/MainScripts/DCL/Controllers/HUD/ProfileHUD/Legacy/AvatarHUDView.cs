using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DCL.Interface;
using Image = UnityEngine.UI.Image;

[assembly: InternalsVisibleTo("ProfileHUDTests")]

namespace Legacy
{
    public class AvatarHUDView : MonoBehaviour
    {
        private const string VIEW_PATH = "Legacy/AvatarHUD";
        private const string VIEW_OBJECT_NAME = "_AvatarHUD";


        [SerializeField] private GameObject loadingAvatar;

        [Header("Minimized UI")]
        [SerializeField] internal Image topAvatarPic;

        [SerializeField] private Button toggleExpandButton;

        [Header("User Info")]
        [SerializeField] private GameObject expandedContainer;

        [SerializeField] private TextMeshProUGUI nameText;

        [Header("Edit Avatar")]
        [SerializeField] private Button editAvatarButton;

        [SerializeField] private GameObject newWearableNotification;
        [SerializeField] private TextMeshProUGUI newWearableNotificationText;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;

        [Header("Controls")]
        [SerializeField] private Button controlsButton;

        [Header("Sign Out")]
        [SerializeField] private Button signOutButton;

        [Header("FAQ")]
        [SerializeField] private Button faqButton;

        [Header("Talking Feedback")]
        [SerializeField] private GameObject playerTalkingIcon;

        private AvatarHUDController controller;

        private void Initialize(AvatarHUDController controller)
        {
            gameObject.name = VIEW_OBJECT_NAME;

            toggleExpandButton.onClick.AddListener(controller.ToggleExpanded);

            editAvatarButton.onClick.AddListener(controller.EditAvatar);
            editAvatarButton.onClick.AddListener(controller.ToggleExpanded);

            signOutButton.onClick.AddListener(controller.SignOut);
            signOutButton.onClick.AddListener(controller.ToggleExpanded);

            settingsButton.onClick.AddListener(controller.ShowSettings);
            settingsButton.onClick.AddListener(controller.ToggleExpanded);

            controlsButton.onClick.AddListener(controller.ShowControls);
            controlsButton.onClick.AddListener(controller.ToggleExpanded);

            faqButton.onClick.AddListener(() =>
            {
                WebInterface.OpenURL("https://docs.decentraland.org/decentraland/faq/");
            });
        }

        internal static AvatarHUDView Create(AvatarHUDController controller)
        {
            var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<AvatarHUDView>();
            view.Initialize(controller);
            return view;
        }

        internal void UpdateData(AvatarHUDModel model)
        {
            topAvatarPic.sprite = model.avatarPic;
            loadingAvatar.SetActive(topAvatarPic.sprite == null);
            nameText.text = model.name;
            newWearableNotificationText.text = model.newWearables.ToString();
            newWearableNotification.SetActive(model.newWearables != 0);
        }

        internal void SetVisibility(bool visibility)
        {
            gameObject.SetActive(visibility);
        }

        internal void SetExpanded(bool visibility)
        {
            expandedContainer.SetActive(visibility);
        }

        internal void SetTalking(bool talking)
        {
            playerTalkingIcon.SetActive(talking);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}