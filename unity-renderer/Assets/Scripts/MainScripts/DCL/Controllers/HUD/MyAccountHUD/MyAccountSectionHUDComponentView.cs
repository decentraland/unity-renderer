using Cysharp.Threading.Tasks;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class MyAccountSectionHUDComponentView : BaseComponentView, IMyAccountSectionHUDComponentView
    {
        private const float COPY_TOAST_VISIBLE_TIME = 3;

        [SerializeField] internal GameObject sectionsMenu;
        [SerializeField] internal MyProfileComponentView myProfileComponentView;
        [SerializeField] internal EmailNotificationsComponentView emailNotificationsComponentView;
        [SerializeField] internal ShowHideAnimator accountSettingsUpdatedToast;

        [Header("Sections Menu Configuration")]
        [SerializeField] internal Button myProfileButton;
        [SerializeField] internal GameObject myProfileButtonDeselected;
        [SerializeField] internal Image myProfileButtonDeselectedImage;
        [SerializeField] internal GameObject myProfileButtonSelected;
        [SerializeField] internal Image myProfileButtonSelectedImage;
        [SerializeField] internal Button emailNotificationsButton;
        [SerializeField] internal GameObject emailNotificationsButtonDeselected;
        [SerializeField] internal Image emailNotificationsButtonDeselectedImage;
        [SerializeField] internal GameObject emailNotificationsButtonSelected;
        [SerializeField] internal Image emailNotificationsButtonSelectedImage;

        public IMyProfileComponentView CurrentMyProfileView => myProfileComponentView;
        public IEmailNotificationsComponentView CurrentEmailNotificationsView => emailNotificationsComponentView;

        private Transform thisTransform;
        private CancellationTokenSource showAccountSettingsCancellationToken = new ();

        public override void Awake()
        {
            base.Awake();

            thisTransform = transform;

            myProfileButton.onClick.AddListener(() => OpenSection(MyAccountSection.MyProfile));
            emailNotificationsButton.onClick.AddListener(() => OpenSection(MyAccountSection.EmailNotifications));

            OpenSection(MyAccountSection.MyProfile);
        }

        public override void Dispose()
        {
            base.Dispose();

            showAccountSettingsCancellationToken.SafeCancelAndDispose();
            myProfileButton.onClick.RemoveAllListeners();
            emailNotificationsButton.onClick.RemoveAllListeners();
        }

        public override void RefreshControl() { }

        public void SetAsFullScreenMenuMode(Transform parentTransform)
        {
            if (parentTransform == null)
                return;

            thisTransform.SetParent(parentTransform);
            thisTransform.localScale = Vector3.one;

            RectTransform rectTransform = thisTransform as RectTransform;
            if (rectTransform == null) return;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localPosition = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }

        public void ShowAccountSettingsUpdatedToast()
        {
            async UniTaskVoid ShowAccountSettingsUpdatedToastAsync(CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!accountSettingsUpdatedToast.gameObject.activeSelf)
                    accountSettingsUpdatedToast.gameObject.SetActive(true);

                accountSettingsUpdatedToast.Show();
                await UniTask.Delay(TimeSpan.FromSeconds(COPY_TOAST_VISIBLE_TIME), cancellationToken: cancellationToken);
                accountSettingsUpdatedToast.Hide();
            }

            showAccountSettingsCancellationToken = showAccountSettingsCancellationToken.SafeRestart();
            ShowAccountSettingsUpdatedToastAsync(showAccountSettingsCancellationToken.Token).Forget();
        }

        public void SetSectionsMenuActive(bool isActive) =>
            sectionsMenu.SetActive(isActive);

        private void OpenSection(MyAccountSection section)
        {
            DeselectButtons();

            switch (section)
            {
                default:
                case MyAccountSection.MyProfile:
                    SetMyProfileButtonStatus(true);
                    SetEmailNotificationsButtonStatus(false);
                    myProfileComponentView.Show();
                    emailNotificationsComponentView.Hide();
                    break;
                case MyAccountSection.EmailNotifications:
                    SetMyProfileButtonStatus(false);
                    SetEmailNotificationsButtonStatus(true);
                    myProfileComponentView.Hide();
                    emailNotificationsComponentView.Show();
                    break;
            }

            DataStore.i.myAccount.openSection.Set(section.ToString());
        }

        private void SetMyProfileButtonStatus(bool isSelected)
        {
            myProfileButton.targetGraphic = isSelected ? myProfileButtonSelectedImage : myProfileButtonDeselectedImage;
            myProfileButtonDeselected.SetActive(!isSelected);
            myProfileButtonSelected.SetActive(isSelected);
        }

        private void SetEmailNotificationsButtonStatus(bool isSelected)
        {
            emailNotificationsButton.targetGraphic = isSelected ? emailNotificationsButtonSelectedImage : emailNotificationsButtonDeselectedImage;
            emailNotificationsButtonDeselected.SetActive(!isSelected);
            emailNotificationsButtonSelected.SetActive(isSelected);
        }

        private static void DeselectButtons()
        {
            if (EventSystem.current == null)
                return;

            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
