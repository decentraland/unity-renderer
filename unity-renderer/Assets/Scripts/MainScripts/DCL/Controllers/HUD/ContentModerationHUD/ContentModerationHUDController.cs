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
        private const int REPORT_PLACE_TIMEOUT = 30;
        private const string LEARN_MORE_URL = "https://decentraland.org/blog/";

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
        private readonly IContentModerationAnalytics contentModerationAnalytics;

        private string currentPlaceId;
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
            IUserProfileBridge userProfileBridge,
            IContentModerationAnalytics contentModerationAnalytics)
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
            this.contentModerationAnalytics = contentModerationAnalytics;

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);
            CommonScriptableObjects.sceneNumber.OnChange += OnSceneNumberChanged;
            UpdateSceneNameByCoords(CommonScriptableObjects.playerCoords.Get(), Vector2Int.zero);
            CommonScriptableObjects.playerCoords.OnChange += UpdateSceneNameByCoords;
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
            CommonScriptableObjects.playerCoords.OnChange -= UpdateSceneNameByCoords;
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
                    adultContentSceneWarningComponentView.SetRestrictedMode(false);
                    break;
                case SceneContentCategory.RESTRICTED:
                    adultContentSceneWarningComponentView.ShowModal();
                    adultContentSceneWarningComponentView.SetRestrictedMode(true);
                    break;
                case SceneContentCategory.TEEN:
                default:
                    adultContentSceneWarningComponentView.HideModal();
                    break;
            }

            currentPlaceId = currentParcelScene.associatedPlaceId;
            UpdateSceneNameByCoords(CommonScriptableObjects.playerCoords.Get(), Vector2Int.zero);
        }

        private void UpdateSceneNameByCoords(Vector2Int playerCoords, Vector2Int _)
        {
            MinimapMetadata.MinimapSceneInfo sceneInfo = MinimapMetadata.GetMetadata().GetSceneInfo(playerCoords.x, playerCoords.y);

            if (sceneInfo != null)
                contentModerationReportingComponentView.SetScene(sceneInfo.name);
        }

        private void OnGoToSettingsPanelClicked()
        {
            settingsDataStore.settingsPanelVisible.Set(true);
            contentModerationAnalytics.OpenSettingsFromContentWarning();
        }

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

        private void OnAdultContentSettingChanged(bool currentIsEnabled, bool previousIsEnabled)
        {
            if (!currentIsEnabled)
                adultContentEnabledNotificationComponentView.HideNotification();

            OnSceneNumberChanged(CommonScriptableObjects.sceneNumber.Get(), 0);

            if (currentIsEnabled != previousIsEnabled)
                contentModerationAnalytics.SwitchAdultContentSetting(currentIsEnabled);
        }

        private void OnReportingScenePanelVisible((bool isVisible, SceneContentCategory rating) panelStatus, (bool isVisible, SceneContentCategory rating) _)
        {
            if (panelStatus.isVisible)
            {
                contentModerationReportingComponentView.ShowPanel();
                contentModerationReportingComponentView.SetRatingAsMarked(panelStatus.rating);
                contentModerationReportingComponentView.SetRating(panelStatus.rating);
                contentModerationAnalytics.OpenReportForm(currentPlaceId);
            }
            else
                contentModerationReportingComponentView.HidePanel(false);
        }

        private void OnContentModerationReportingClosed(bool isCancelled)
        {
            contentModerationDataStore.reportingScenePanelVisible.Set((false, contentModerationDataStore.reportingScenePanelVisible.Get().rating), false);
            contentModerationAnalytics.CloseReportForm(currentPlaceId, isCancelled);
        }

        private void OnContentModerationReportingSendClicked((SceneContentCategory contentCategory, List<string> issues, string comments) report)
        {
            reportPlaceCts = reportPlaceCts.SafeRestart();
            SendReportAsync(
                    new PlaceContentReportPayload
                    {
                        placeId = "e0d0fc69-1628-4a2e-914a-ad76d681528b",//currentPlaceId, // TODO (Santi): Uncomment this and remove the hardcoded id!! Test places for .zone: e0d0fc69-1628-4a2e-914a-ad76d681528b, f8f2d59b-0755-47c3-88ac-b117f697d251
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
                contentModerationAnalytics.SubmitReportForm(
                    placeContentReport.placeId,
                    placeContentReport.rating,
                    placeContentReport.issues,
                    placeContentReport.comment);

                contentModerationReportingComponentView.SetLoadingState(true);
                await placesAPIService.ReportPlace(placeContentReport, ct)
                                      .Timeout(TimeSpan.FromSeconds(REPORT_PLACE_TIMEOUT));
                contentModerationReportingComponentView.SetPanelAsSent(true);
                contentModerationReportingComponentView.SetLoadingState(false);
            }
            catch (Exception ex)
            {
                contentModerationAnalytics.ErrorSendingReportingForm(currentPlaceId);
                contentModerationReportingComponentView.SetPanelAsError();
                Debug.LogError($"An error occurred while reporting the content category for ({placeContentReport.coordinates}): {ex.Message}");
            }
        }

        private void OnLearnMoreClicked()
        {
            browserBridge.OpenUrl(LEARN_MORE_URL); // TODO (Santi): Change this to the correct URL when it's ready
            contentModerationAnalytics.ClickLearnMoreContentModeration(currentPlaceId);
        }
    }
}
