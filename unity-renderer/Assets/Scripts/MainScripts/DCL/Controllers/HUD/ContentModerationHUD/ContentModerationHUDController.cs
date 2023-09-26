using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers;

namespace DCL.ContentModeration
{

    public class ContentModerationHUDController
    {
        private readonly IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView;
        private readonly IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView;
        private readonly IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView;
        private readonly IWorldState worldState;
        private readonly DataStore_Settings settingsDataStore;
        private readonly DataStore_ContentModeration contentModerationDataStore;

        public ContentModerationHUDController(
            IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView,
            IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView,
            IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView,
            IWorldState worldState,
            DataStore_Settings settingsDataStore,
            DataStore_ContentModeration contentModerationDataStore)
        {
            this.adultContentSceneWarningComponentView = adultContentSceneWarningComponentView;
            this.adultContentAgeConfirmationComponentView = adultContentAgeConfirmationComponentView;
            this.adultContentEnabledNotificationComponentView = adultContentEnabledNotificationComponentView;
            this.worldState = worldState;
            this.settingsDataStore = settingsDataStore;
            this.contentModerationDataStore = contentModerationDataStore;

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);
            CommonScriptableObjects.sceneNumber.OnChange += OnSceneNumberChanged;
            adultContentSceneWarningComponentView.OnGoToSettingsClicked += OnGoToSettingsPanelClicked;
            contentModerationDataStore.adultContentAgeConfirmationVisible.OnChange += OnAdultContentAgeConfirmationVisible;
            adultContentAgeConfirmationComponentView.OnConfirmClicked += OnAgeConfirmationAccepted;
            adultContentAgeConfirmationComponentView.OnCancelClicked += OnAgeConfirmationRejected;
            contentModerationDataStore.adultContentSettingEnabled.OnChange += OnAdultContentSettingChanged;
        }

        public void Dispose()
        {
            CommonScriptableObjects.sceneNumber.OnChange -= OnSceneNumberChanged;
            adultContentSceneWarningComponentView.OnGoToSettingsClicked -= OnGoToSettingsPanelClicked;
            contentModerationDataStore.adultContentAgeConfirmationVisible.OnChange -= OnAdultContentAgeConfirmationVisible;
            adultContentAgeConfirmationComponentView.OnConfirmClicked -= OnAgeConfirmationAccepted;
            adultContentAgeConfirmationComponentView.OnCancelClicked -= OnAgeConfirmationRejected;
            contentModerationDataStore.adultContentSettingEnabled.OnChange -= OnAdultContentSettingChanged;
        }

        private void OnSceneNumberChanged(int currentSceneNumber, int _)
        {
            if (!worldState.TryGetScene(currentSceneNumber, out IParcelScene currentParcelScene))
                return;

            switch (currentParcelScene.contentCategory)
            {
                case SceneContentCategory.ADULT when !contentModerationDataStore.adultContentSettingEnabled.Get():
                    Utils.UnlockCursor();
                    adultContentSceneWarningComponentView.ShowModal();
                    break;
                case SceneContentCategory.RESTRICTED:
                    // TODO: Show a different modal for restricted scenes
                    break;
                case SceneContentCategory.TEEN:
                default:
                    adultContentSceneWarningComponentView.HideModal();
                    // TODO: Hide the modal for restricted scenes
                    break;
            }
        }

        private void OnGoToSettingsPanelClicked() =>
            settingsDataStore.settingsPanelVisible.Set(true);

        private void OnAdultContentAgeConfirmationVisible(bool isVisible, bool _)
        {
            if (isVisible)
                adultContentAgeConfirmationComponentView.ShowModal();
        }

        private void OnAgeConfirmationAccepted()
        {
            contentModerationDataStore.adultContentSettingEnabled.Set(true);
            settingsDataStore.settingsPanelVisible.Set(false);
            adultContentEnabledNotificationComponentView.ShowNotification();
            HideNotificationAfterDelay(5).Forget();
            return;

            UniTask HideNotificationAfterDelay(int delayInSeconds)
            {
                return UniTask.Delay(delayInSeconds * 1000).ContinueWith(() =>
                    adultContentEnabledNotificationComponentView.HideNotification());
            }
        }

        private void OnAgeConfirmationRejected()
        {
            contentModerationDataStore.adultContentSettingEnabled.Set(false, false);
            contentModerationDataStore.resetAdultContentSetting.Set(true, true);
        }

        private void OnAdultContentSettingChanged(bool current, bool previous)
        {
            if (!current)
                adultContentEnabledNotificationComponentView.HideNotification();

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);
        }
    }
}
