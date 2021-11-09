using DCL;
using DCL.Helpers;
using ExploreV2Analytics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables.RealmsInfo;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    internal IExploreV2MenuComponentView view;
    internal IPlacesAndEventsSectionComponentController placesAndEventsSectionController;
    internal InputAction_Trigger toggleExploreTrigger;
    internal IExploreV2Analytics exploreV2Analytics;
    internal ExploreSection currentOpenSection;
    internal float lastTimeSectionWasOpen = 0f;
    internal bool anyActionExecutedFromLastOpen = false;

    public void Initialize()
    {
        exploreV2Analytics = CreateAnalyticsController();
        view = CreateView();
        SetVisibility(false, false);

        DataStore.i.playerRealm.OnChange += UpdateRealmInfo;
        UpdateRealmInfo(DataStore.i.playerRealm.Get(), null);

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        DataStore.i.taskbar.isExploreV2Enabled.OnChange += OnActivateFromTaskbar;
        DataStore.i.exploreV2.isInitialized.Set(true);

        view.OnInitialized += CreateControllers;
        view.OnSectionOpen += OnSectionOpen;

        toggleExploreTrigger = Resources.Load<InputAction_Trigger>("ToggleExploreHud");
        toggleExploreTrigger.OnTriggered += OnToggleActionTriggered;
    }

    internal void CreateControllers()
    {
        placesAndEventsSectionController = new PlacesAndEventsSectionComponentController(view.currentPlacesAndEventsSection, exploreV2Analytics);
        placesAndEventsSectionController.OnCloseExploreV2 += OnCloseButtonPressed;
        placesAndEventsSectionController.OnAnyActionExecuted += OnAnyActionExecuted;
    }

    internal void OnSectionOpen(ExploreSection section)
    {
        if (lastTimeSectionWasOpen > 0f && section != currentOpenSection)
        {
            exploreV2Analytics.SendExploreSectionElapsedTime(currentOpenSection, Time.realtimeSinceStartup - lastTimeSectionWasOpen);
            anyActionExecutedFromLastOpen = true;
        }

        lastTimeSectionWasOpen = Time.realtimeSinceStartup;
        currentOpenSection = section;
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= UpdateRealmInfo;
        ownUserProfile.OnUpdate -= UpdateProfileInfo;

        if (view != null)
        {
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            view.OnInitialized -= CreateControllers;
            view.OnSectionOpen -= OnSectionOpen;
            view.Dispose();
        }

        DataStore.i.taskbar.isExploreV2Enabled.OnChange -= OnActivateFromTaskbar;

        if (placesAndEventsSectionController != null)
        {
            placesAndEventsSectionController.OnCloseExploreV2 -= OnCloseButtonPressed;
            placesAndEventsSectionController.OnAnyActionExecuted -= OnAnyActionExecuted;
            placesAndEventsSectionController.Dispose();
        }

        toggleExploreTrigger.OnTriggered -= OnToggleActionTriggered;
    }

    public void SetVisibility(bool visible, bool fromShortcut)
    {
        if (view == null)
            return;

        if (visible != DataStore.i.exploreV2.isOpen.Get())
        {
            if (visible)
            {
                Utils.UnlockCursor();
                AudioScriptableObjects.dialogOpen.Play(true);
                AudioScriptableObjects.listItemAppear.ResetPitch();
            }
            else
            {
                AudioScriptableObjects.dialogClose.Play(true);

                float elpasedTimeInCurrentSection = Time.realtimeSinceStartup - lastTimeSectionWasOpen;
                exploreV2Analytics.SendExploreSectionElapsedTime(currentOpenSection, elpasedTimeInCurrentSection);
                lastTimeSectionWasOpen = 0f;

                if (!anyActionExecutedFromLastOpen)
                    exploreV2Analytics.SendExploreExitWithoutActions(elpasedTimeInCurrentSection);
            }

            exploreV2Analytics.SendExploreVisibility(visible, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
            anyActionExecutedFromLastOpen = false;
        }

        DataStore.i.exploreV2.isOpen.Set(visible);
        view.SetVisible(visible);
    }

    internal void UpdateRealmInfo(CurrentRealmModel currentRealm, CurrentRealmModel previousRealm)
    {
        // Get the name of the current realm
        string currentRealmServer = currentRealm?.serverName;
        string currentRealmLayer = currentRealm?.layer;
        string formattedRealmName = currentRealmServer;
        if (!string.IsNullOrEmpty(currentRealmLayer))
        {
            formattedRealmName = $"{formattedRealmName}-{currentRealmLayer}";
        }

        view.currentRealmViewer.SetRealm(formattedRealmName);

        // Calculate number of users in the current realm
        List<RealmModel> realmList = DataStore.i.realmsInfo.Get()?.ToList();
        RealmModel currentRealmModel = realmList?.FirstOrDefault(r => r.serverName == currentRealmServer && (r.layer == null || r.layer == currentRealmLayer));
        int realmUsers = 0;
        if (currentRealmModel != null)
            realmUsers = currentRealmModel.usersCount;

        view.currentRealmViewer.SetNumberOfUsers(realmUsers);
    }

    internal void UpdateProfileInfo(UserProfile profile)
    {
        view.currentProfileCard.SetProfileName(profile.userName);
        view.currentProfileCard.SetProfileAddress(profile.ethAddress);
        view.currentProfileCard.SetProfilePicture(profile.face128SnapshotURL);
    }

    internal void OnCloseButtonPressed() { SetVisibility(false, false); }

    internal void OnToggleActionTriggered(DCLAction_Trigger action)
    {
        bool isVisible = !DataStore.i.exploreV2.isOpen.Get();
        SetVisibility(isVisible, true);
    }

    internal void OnActivateFromTaskbar(bool current, bool previous) { SetVisibility(current, false); }

    internal void OnAnyActionExecuted() { anyActionExecutedFromLastOpen = true; }

    internal virtual IExploreV2Analytics CreateAnalyticsController() => new ExploreV2Analytics.ExploreV2Analytics();

    internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}