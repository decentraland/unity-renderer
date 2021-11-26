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
    internal BaseVariable<bool> profileCardIsOpen => DataStore.i.exploreV2.profileCardIsOpen;
    internal BaseVariable<bool> placesAndEventsVisible => DataStore.i.exploreV2.placesAndEventsVisible;
    internal BaseVariable<bool> isAvatarEditorInitialized => DataStore.i.HUDs.isAvatarEditorInitialized;
    internal BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
    internal BaseVariable<bool> isNavmapVisibleInitialized => DataStore.i.HUDs.isNavMapInitialized;
    internal BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
    internal BaseVariable<bool> isBuilderInitialized => DataStore.i.builderInWorld.isInitialized;
    internal BaseVariable<bool> builderVisible => DataStore.i.HUDs.builderProjectsPanelVisible;
    internal BaseVariable<bool> isQuestInitialized => DataStore.i.Quests.isInitialized;
    internal BaseVariable<bool> questVisible => DataStore.i.HUDs.questsPanelVisible;
    internal BaseVariable<bool> isSettingsPanelInitialized => DataStore.i.settings.isInitialized;
    internal BaseVariable<bool> settingsVisible => DataStore.i.settings.settingsPanelVisible;

    public void Initialize()
    {
        exploreV2Analytics = CreateAnalyticsController();
        view = CreateView();
        SetVisibility(false);

        DataStore.i.playerRealm.OnChange += UpdateRealmInfo;
        UpdateRealmInfo(DataStore.i.playerRealm.Get(), null);

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);
        view.currentProfileCard.onClick?.AddListener(() => { profileCardIsOpen.Set(!profileCardIsOpen.Get()); });

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        DataStore.i.exploreV2.isInitialized.Set(true);

        view.OnInitialized += CreateControllers;
        view.OnSectionOpen += OnSectionOpen;

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        placesAndEventsVisible.OnChange += PlacesAndEventsVisibleChanged;
        PlacesAndEventsVisibleChanged(placesAndEventsVisible.Get(), false);

        isAvatarEditorInitialized.OnChange += IsAvatarEditorInitializedChanged;
        IsAvatarEditorInitializedChanged(isAvatarEditorInitialized.Get(), false);
        avatarEditorVisible.OnChange += AvatarEditorVisibleChanged;
        AvatarEditorVisibleChanged(avatarEditorVisible.Get(), false);

        isNavmapVisibleInitialized.OnChange += IsNavMapInitializedChanged;
        IsNavMapInitializedChanged(isNavmapVisibleInitialized.Get(), false);
        navmapVisible.OnChange += NavmapVisibleChanged;
        NavmapVisibleChanged(navmapVisible.Get(), false);

        isBuilderInitialized.OnChange += IsBuilderInitializedChanged;
        IsBuilderInitializedChanged(isBuilderInitialized.Get(), false);
        builderVisible.OnChange += BuilderVisibleChanged;
        BuilderVisibleChanged(builderVisible.Get(), false);

        isQuestInitialized.OnChange += IsQuestInitializedChanged;
        IsQuestInitializedChanged(isQuestInitialized.Get(), false);
        questVisible.OnChange += QuestVisibleChanged;
        QuestVisibleChanged(questVisible.Get(), false);

        isSettingsPanelInitialized.OnChange += IsSettingsPanelInitializedChanged;
        IsSettingsPanelInitializedChanged(isSettingsPanelInitialized.Get(), false);
        settingsVisible.OnChange += SettingsVisibleChanged;
        SettingsVisibleChanged(settingsVisible.Get(), false);
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

        if (currentOpenSection == ExploreSection.Backpack)
        {
            view.ConfigureEncapsulatedSection(
                ExploreSection.Backpack,
                DataStore.i.exploreV2.configureBackpackInFullscreenMenu);
        }

        placesAndEventsVisible.Set(currentOpenSection == ExploreSection.Explore);
        avatarEditorVisible.Set(currentOpenSection == ExploreSection.Backpack);
        navmapVisible.Set(currentOpenSection == ExploreSection.Map);
        builderVisible.Set(currentOpenSection == ExploreSection.Builder);
        questVisible.Set(currentOpenSection == ExploreSection.Quest);
        settingsVisible.Set(currentOpenSection == ExploreSection.Settings);
        profileCardIsOpen.Set(false);
    }

    public void Dispose()
    {
        DataStore.i.playerRealm.OnChange -= UpdateRealmInfo;
        ownUserProfile.OnUpdate -= UpdateProfileInfo;
        view.currentProfileCard.onClick?.RemoveAllListeners();
        isOpen.OnChange -= IsOpenChanged;
        placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
        isAvatarEditorInitialized.OnChange += IsAvatarEditorInitializedChanged;
        avatarEditorVisible.OnChange -= AvatarEditorVisibleChanged;
        isNavmapVisibleInitialized.OnChange -= IsNavMapInitializedChanged;
        navmapVisible.OnChange -= NavmapVisibleChanged;
        isBuilderInitialized.OnChange -= IsBuilderInitializedChanged;
        builderVisible.OnChange -= BuilderVisibleChanged;
        isQuestInitialized.OnChange -= IsQuestInitializedChanged;
        questVisible.OnChange -= QuestVisibleChanged;
        isSettingsPanelInitialized.OnChange -= IsSettingsPanelInitializedChanged;
        settingsVisible.OnChange -= SettingsVisibleChanged;

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
            placesAndEventsVisible.Set(false);
            avatarEditorVisible.Set(false);
            profileCardIsOpen.Set(false);
            navmapVisible.Set(false);
            builderVisible.Set(false);
            questVisible.Set(false);
            settingsVisible.Set(false);
            exploreV2Analytics.anyActionExecutedFromLastOpen = false;
        }

        view.SetVisible(visible);
    }

    internal void PlacesAndEventsVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Explore);
        }
        else if (currentOpenSection == ExploreSection.Explore)
        {
            SetVisibility(false);
        }
    }

    internal void IsAvatarEditorInitializedChanged(bool current, bool previous) { view.SetSectionActive(ExploreSection.Backpack, current); }

    internal void AvatarEditorVisibleChanged(bool current, bool previous)
    {
        if (DataStore.i.isSignUpFlow.Get())
            return;

        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Backpack);
        }
        else if (currentOpenSection == ExploreSection.Backpack)
        {
            SetVisibility(false);
        }
    }

    internal void IsNavMapInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Map, current);

        if (current)
            view.ConfigureEncapsulatedSection(ExploreSection.Map, DataStore.i.exploreV2.configureMapInFullscreenMenu);
    }

    internal void NavmapVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Map);
        }
        else if (currentOpenSection == ExploreSection.Map)
        {
            SetVisibility(false);
        }
    }

    internal void IsBuilderInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Builder, current);

        if (current)
            view.ConfigureEncapsulatedSection(ExploreSection.Builder, DataStore.i.exploreV2.configureBuilderInFullscreenMenu);
    }

    internal void BuilderVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Builder);
        }
        else if (currentOpenSection == ExploreSection.Builder)
        {
            SetVisibility(false);
        }
    }

    internal void IsQuestInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Quest, current);

        if (current)
            view.ConfigureEncapsulatedSection(ExploreSection.Quest, DataStore.i.exploreV2.configureQuestInFullscreenMenu);
    }

    internal void QuestVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Quest);
        }
        else if (currentOpenSection == ExploreSection.Quest)
        {
            SetVisibility(false);
        }
    }

    internal void IsSettingsPanelInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Settings, current);

        if (current)
            view.ConfigureEncapsulatedSection(ExploreSection.Settings, DataStore.i.exploreV2.configureSettingsInFullscreenMenu);
    }

    internal void SettingsVisibleChanged(bool current, bool previous)
    {
        if (current)
        {
            SetVisibility(true);
            view.GoToSection(ExploreSection.Settings);
        }
        else if (currentOpenSection == ExploreSection.Settings)
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