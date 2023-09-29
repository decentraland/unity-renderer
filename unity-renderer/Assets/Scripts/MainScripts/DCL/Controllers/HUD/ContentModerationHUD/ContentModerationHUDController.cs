using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Tasks;
using DCLServices.PlacesAPIService;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.ContentModeration
{

    public class ContentModerationHUDController
    {
        private const int SECONDS_TO_HIDE_ADULT_CONTENT_ENABLED_NOTIFICATION = 5;

        private readonly IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView;
        private readonly IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView;
        private readonly IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView;
        private readonly IContentModerationReportingComponentView contentModerationReportingComponentView;
        private readonly IWorldState worldState;
        private readonly DataStore_Settings settingsDataStore;
        private readonly DataStore_ContentModeration contentModerationDataStore;
        private readonly IBrowserBridge browserBridge;
        private readonly IPlacesAPIService placesAPIService;
        private readonly IUserProfileBridge userProfileBridge;

        private CancellationTokenSource reportPlaceCts;

        public ContentModerationHUDController(
            IAdultContentSceneWarningComponentView adultContentSceneWarningComponentView,
            IAdultContentAgeConfirmationComponentView adultContentAgeConfirmationComponentView,
            IAdultContentEnabledNotificationComponentView adultContentEnabledNotificationComponentView,
            IContentModerationReportingComponentView contentModerationReportingComponentView,
            IWorldState worldState,
            DataStore_Settings settingsDataStore,
            DataStore_ContentModeration contentModerationDataStore,
            IBrowserBridge browserBridge,
            IPlacesAPIService placesAPIService,
            IUserProfileBridge userProfileBridge)
        {
            this.adultContentSceneWarningComponentView = adultContentSceneWarningComponentView;
            this.adultContentAgeConfirmationComponentView = adultContentAgeConfirmationComponentView;
            this.adultContentEnabledNotificationComponentView = adultContentEnabledNotificationComponentView;
            this.contentModerationReportingComponentView = contentModerationReportingComponentView;
            this.worldState = worldState;
            this.settingsDataStore = settingsDataStore;
            this.contentModerationDataStore = contentModerationDataStore;
            this.browserBridge = browserBridge;
            this.placesAPIService = placesAPIService;
            this.userProfileBridge = userProfileBridge;

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

            reportPlaceCts.SafeCancelAndDispose();
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
            HideNotificationAfterDelay(SECONDS_TO_HIDE_ADULT_CONTENT_ENABLED_NOTIFICATION).Forget();
        }

        private UniTask HideNotificationAfterDelay(int delayInSeconds)
        {
            return UniTask.Delay(delayInSeconds * 1000).ContinueWith(() =>
                adultContentEnabledNotificationComponentView.HideNotification());
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
            if (!worldState.TryGetScene(CommonScriptableObjects.sceneNumber.Get(), out IParcelScene currentParcelScene))
                return;

            reportPlaceCts = reportPlaceCts.SafeRestart();
            SendReportAsync(
                    new PlaceContentReportPayload
                    {
                        // TODO (Santi): Test places for .zone: e0d0fc69-1628-4a2e-914a-ad76d681528b, f8f2d59b-0755-47c3-88ac-b117f697d251
                        placeId = currentParcelScene.associatedPlaceId,
                        guest = userProfileBridge.GetOwn().isGuest,
                        coordinates = $"{CommonScriptableObjects.playerCoords.Get().x},{CommonScriptableObjects.playerCoords.Get().y}",
                        rating = report.contentCategory switch
                                 {
                                     SceneContentCategory.TEEN => "T",
                                     SceneContentCategory.ADULT => "A",
                                     SceneContentCategory.RESTRICTED => "R",
                                     _ => "E",
                                 },
                        issues = report.issues.ToArray(),
                        comment = report.comments,
                    },
                    reportPlaceCts.Token)
               .Forget();
        }

        private async UniTask SendReportAsync(PlaceContentReportPayload placeContentReport, CancellationToken ct)
        {
            try
            {
                contentModerationReportingComponentView.SetLoadingState(true);
                await placesAPIService.ReportPlace(placeContentReport, ct)
                                      .Timeout(TimeSpan.FromSeconds(5));
                contentModerationReportingComponentView.SetPanelAsSent(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"An error occurred while reporting the content category for ({placeContentReport.coordinates}): {ex.Message}");
            }
            finally
            {
                contentModerationReportingComponentView.SetLoadingState(false);
            }
        }

        private void OnLearnMoreClicked()
        {
            // TODO (Santi): Open the correct Learn More url
            browserBridge.OpenUrl("https://www.google.es");
        }
    }
}
