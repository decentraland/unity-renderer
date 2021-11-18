using DCL;
using DCL.Helpers;
using ExploreV2Analytics;
using System.Collections.Generic;
using System.Linq;
using Variables.RealmsInfo;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    internal IExploreV2MenuComponentView view;
    internal IPlacesAndEventsSectionComponentController placesAndEventsSectionController;
    internal IExploreV2Analytics exploreV2Analytics;
    internal ExploreSection currentOpenSection;

    internal BaseVariable<bool> isOpen => DataStore.i.exploreV2.isOpen;
    internal BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;

    public void Initialize()
    {
        exploreV2Analytics = CreateAnalyticsController();
        view = CreateView();
        SetVisibility(false);

        DataStore.i.playerRealm.OnChange += UpdateRealmInfo;
        UpdateRealmInfo(DataStore.i.playerRealm.Get(), null);

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        DataStore.i.exploreV2.isInitialized.Set(true);

        view.OnInitialized += CreateControllers;
        view.OnSectionOpen += OnSectionOpen;

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        avatarEditorVisible.OnChange += IsAvatarEditorVisibleChanged;
        IsAvatarEditorVisibleChanged(avatarEditorVisible.Get(), false);
    }

    internal void CreateControllers()
    {
        placesAndEventsSectionController = new PlacesAndEventsSectionComponentController(view.currentPlacesAndEventsSection, exploreV2Analytics);
        placesAndEventsSectionController.OnCloseExploreV2 += OnCloseButtonPressed;
        placesAndEventsSectionController.OnAnyActionExecuted += OnAnyActionExecuted;
    }

    internal void OnSectionOpen(ExploreSection section)
    {
        if (section != currentOpenSection)
        {
            exploreV2Analytics.SendExploreSectionVisibility(currentOpenSection, false);
            exploreV2Analytics.SendExploreSectionVisibility(section, true);
            exploreV2Analytics.anyActionExecutedFromLastOpen = true;
        }

        currentOpenSection = section;

        avatarEditorVisible.Set(currentOpenSection == ExploreSection.Backpack);
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= UpdateRealmInfo;
        ownUserProfile.OnUpdate -= UpdateProfileInfo;
        isOpen.OnChange -= IsOpenChanged;
        avatarEditorVisible.OnChange -= IsAvatarEditorVisibleChanged;

        if (view != null)
        {
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            view.OnInitialized -= CreateControllers;
            view.OnSectionOpen -= OnSectionOpen;
            view.Dispose();
        }

        if (placesAndEventsSectionController != null)
        {
            placesAndEventsSectionController.OnCloseExploreV2 -= OnCloseButtonPressed;
            placesAndEventsSectionController.OnAnyActionExecuted -= OnAnyActionExecuted;
            placesAndEventsSectionController.Dispose();
        }
    }

    public void SetVisibility(bool visible) { isOpen.Set(visible); }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    internal void SetVisibility_Internal(bool visible)
    {
        if (view == null)
            return;

        if (visible)
        {
            Utils.UnlockCursor();
            AudioScriptableObjects.dialogOpen.Play(true);
            AudioScriptableObjects.listItemAppear.ResetPitch();
            CommonScriptableObjects.isFullscreenHUDOpen.Set(true);
        }
        else
        {
            AudioScriptableObjects.dialogClose.Play(true);
            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
            avatarEditorVisible.Set(false);
            exploreV2Analytics.anyActionExecutedFromLastOpen = false;
        }

        view.SetVisible(visible);
    }

    internal void IsAvatarEditorVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            if (!DataStore.i.isSignUpFlow.Get())
                view.ConfigureBackpackSection();

            SetVisibility(true);
            view.GoToSection(ExploreSection.Backpack);
        }
        else if (currentOpenSection == ExploreSection.Backpack)
        {
            SetVisibility(false);
        }
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

    internal void OnCloseButtonPressed()
    {
        if (DataStore.i.exploreV2.isOpen.Get())
            exploreV2Analytics.SendExploreMainMenuVisibility(false, ExploreUIVisibilityMethod.FromClick);
        SetVisibility(false);
    }

    internal void OnAnyActionExecuted() { exploreV2Analytics.anyActionExecutedFromLastOpen = true; }

    internal virtual IExploreV2Analytics CreateAnalyticsController() => new ExploreV2Analytics.ExploreV2Analytics();

    internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}