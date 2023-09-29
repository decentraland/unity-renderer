using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections.Generic;

namespace DCL.ContentModeration
{

    public class ContentModerationHUDController
    {
        private readonly IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView;
        private readonly IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView;
        private readonly IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView;
        private readonly IContentModerationReportingComponentView contentModerationReportingComponentView;
        private readonly IWorldState worldState;
        private readonly DataStore_Settings settingsDataStore;
        private readonly DataStore_ContentModeration contentModerationDataStore;
        private readonly IBrowserBridge browserBridge;

        public ContentModerationHUDController(
            IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView,
            IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView,
            IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView,
            IContentModerationReportingComponentView contentModerationReportingComponentView,
            IWorldState worldState,
            DataStore_Settings settingsDataStore,
            DataStore_ContentModeration contentModerationDataStore,
            IBrowserBridge browserBridge)
        {
            this.adultContentSceneWarningComponentView = adultContentSceneWarningComponentView;
            this.adultContentAgeConfirmationComponentView = adultContentAgeConfirmationComponentView;
            this.adultContentEnabledNotificationComponentView = adultContentEnabledNotificationComponentView;
            this.contentModerationReportingComponentView = contentModerationReportingComponentView;
            this.worldState = worldState;
            this.settingsDataStore = settingsDataStore;
            this.contentModerationDataStore = contentModerationDataStore;
            this.browserBridge = browserBridge;

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);
            CommonScriptableObjects.sceneNumber.OnChange += OnSceneNumberChanged;
            adultContentSceneWarningComponentView.OnGoToSettingsClicked += OnGoToSettingsPanelClicked;
            contentModerationDataStore.adultContentAgeConfirmationVisible.OnChange += OnAdultContentAgeConfirmationVisible;
            adultContentAgeConfirmationComponentView.OnConfirmClicked += OnAgeConfirmationAccepted;
            adultContentAgeConfirmationComponentView.OnCancelClicked += OnAgeConfirmationRejected;
            contentModerationDataStore.adultContentSettingEnabled.OnChange += OnAdultContentSettingChanged;
            contentModerationDataStore.reportingScenePanelVisible.OnChange += OnReportingScenePanelVisible;
            contentModerationReportingComponentView.OnPanelClosed += OnContentModerationReportingClosed;
            contentModerationReportingComponentView.OnSendClicked += OnContentModerationReportingSendClicked;
            contentModerationReportingComponentView.OnLearnMoreClicked += OnLearnMoreClicked;
        }

        public void Dispose()
        {
            CommonScriptableObjects.sceneNumber.OnChange -= OnSceneNumberChanged;
            adultContentSceneWarningComponentView.OnGoToSettingsClicked -= OnGoToSettingsPanelClicked;
            contentModerationDataStore.adultContentAgeConfirmationVisible.OnChange -= OnAdultContentAgeConfirmationVisible;
            adultContentAgeConfirmationComponentView.OnConfirmClicked -= OnAgeConfirmationAccepted;
            adultContentAgeConfirmationComponentView.OnCancelClicked -= OnAgeConfirmationRejected;
            contentModerationDataStore.adultContentSettingEnabled.OnChange -= OnAdultContentSettingChanged;
            contentModerationDataStore.reportingScenePanelVisible.OnChange -= OnReportingScenePanelVisible;
            contentModerationReportingComponentView.OnPanelClosed -= OnContentModerationReportingClosed;
            contentModerationReportingComponentView.OnSendClicked -= OnContentModerationReportingSendClicked;
            contentModerationReportingComponentView.OnLearnMoreClicked -= OnLearnMoreClicked;
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
                    // TODO (Santi): Show a different modal for restricted scenes
                    break;
                case SceneContentCategory.TEEN:
                default:
                    adultContentSceneWarningComponentView.HideModal();
                    // TODO (Santi): Hide the modal for restricted scenes
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
            contentModerationDataStore.adultContentAgeConfirmationResult.Set(DataStore_ContentModeration.AdultContentAgeConfirmationResult.Accepted, true);
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
            contentModerationDataStore.adultContentAgeConfirmationResult.Set(DataStore_ContentModeration.AdultContentAgeConfirmationResult.Rejected, true);
        }

        private void OnAdultContentSettingChanged(bool isEnabled, bool _)
        {
            if (!isEnabled)
                adultContentEnabledNotificationComponentView.HideNotification();

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);
        }

        private void OnReportingScenePanelVisible((bool isVisible, SceneContentCategory rating) panelStatus, (bool isVisible, SceneContentCategory rating) _)
        {
            if (panelStatus.isVisible)
            {
                contentModerationReportingComponentView.ShowPanel();
                contentModerationReportingComponentView.SetRating(panelStatus.rating);
            }
            else
                contentModerationReportingComponentView.HidePanel();
        }

        private void OnContentModerationReportingClosed() =>
            contentModerationDataStore.reportingScenePanelVisible.Set((false, contentModerationDataStore.reportingScenePanelVisible.Get().rating), false);

        private void OnContentModerationReportingSendClicked((SceneContentCategory contentCategory, List<string> issues, string comments) report)
        {
            contentModerationReportingComponentView.SetLoadingState(true);

            contentModerationReportingComponentView.SetLoadingState(false);

            // TODO (Santi): Send the report to the backend

            contentModerationReportingComponentView.SetPanelAsSent(true);
        }

        private void OnLearnMoreClicked()
        {
            // TODO (Santi): Open the correct Learn More url
            browserBridge.OpenUrl("https://www.google.es");
        }
    }
}
