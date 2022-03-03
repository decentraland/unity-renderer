using DCL;
using DCL.Helpers;
using ExploreV2Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Variables.RealmsInfo;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    internal const float MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU = 0.1f;

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    internal RectTransform topMenuTooltipReference { get => view.currentTopMenuTooltipReference; }
    internal RectTransform placesAndEventsTooltipReference { get => view.currentPlacesAndEventsTooltipReference; }
    internal RectTransform backpackTooltipReference { get => view.currentBackpackTooltipReference; }
    internal RectTransform mapTooltipReference { get => view.currentMapTooltipReference; }
    internal RectTransform builderTooltipReference { get => view.currentBuilderTooltipReference; }
    internal RectTransform questTooltipReference { get => view.currentQuestTooltipReference; }
    internal RectTransform settingsTooltipReference { get => view.currentSettingsTooltipReference; }
    internal RectTransform profileCardTooltipReference { get => view.currentProfileCardTooltipReference; }

    internal IExploreV2MenuComponentView view;
    internal IPlacesAndEventsSectionComponentController placesAndEventsSectionController;
    internal IExploreV2Analytics exploreV2Analytics;
    internal ExploreSection currentOpenSection;
    internal float controlsHUDCloseTime = 0f;
    internal float emotesHUDCloseTime = 0f;
    internal float playerInfoCardHUDCloseTime = 0f;
    internal float chatInputHUDCloseTime = 0f;
    internal List<RealmRowComponentModel> currentAvailableRealms = new List<RealmRowComponentModel>();

    internal BaseVariable<bool> isOpen => DataStore.i.exploreV2.isOpen;
    internal BaseVariable<int> currentSectionIndex => DataStore.i.exploreV2.currentSectionIndex;
    internal BaseVariable<bool> profileCardIsOpen => DataStore.i.exploreV2.profileCardIsOpen;
    internal BaseVariable<bool> isInitialized => DataStore.i.exploreV2.isInitialized;
    internal BaseVariable<bool> isPlacesAndEventsSectionInitialized => DataStore.i.exploreV2.isPlacesAndEventsSectionInitialized;
    internal BaseVariable<bool> placesAndEventsVisible => DataStore.i.exploreV2.placesAndEventsVisible;
    internal BaseVariable<bool> isAvatarEditorInitialized => DataStore.i.HUDs.isAvatarEditorInitialized;
    internal BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
    internal BaseVariable<bool> isNavmapInitialized => DataStore.i.HUDs.isNavMapInitialized;
    internal BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
    internal BaseVariable<bool> isBuilderInitialized => DataStore.i.builderInWorld.isInitialized;
    internal BaseVariable<bool> builderVisible => DataStore.i.HUDs.builderProjectsPanelVisible;
    internal BaseVariable<bool> isQuestInitialized => DataStore.i.Quests.isInitialized;
    internal BaseVariable<bool> questVisible => DataStore.i.HUDs.questsPanelVisible;
    internal BaseVariable<bool> isSettingsPanelInitialized => DataStore.i.settings.isInitialized;
    internal BaseVariable<bool> settingsVisible => DataStore.i.settings.settingsPanelVisible;

    internal BaseVariable<bool> controlsVisible => DataStore.i.HUDs.controlsVisible;
    internal BaseVariable<bool> emotesVisible => DataStore.i.HUDs.emotesVisible;
    internal BaseVariable<bool> chatInputVisible => DataStore.i.HUDs.chatInputVisible;
    internal BooleanVariable playerInfoCardVisible => CommonScriptableObjects.playerInfoCardVisibleState;

    public void Initialize()
    {
        exploreV2Analytics = CreateAnalyticsController();
        view = CreateView();
        SetVisibility(false);

        DataStore.i.realm.playerRealm.OnChange += UpdateRealmInfo;
        UpdateRealmInfo(DataStore.i.realm.playerRealm.Get(), null);

        DataStore.i.realm.realmsInfo.OnSet += UpdateAvailableRealmsInfo;
        UpdateAvailableRealmsInfo(DataStore.i.realm.realmsInfo.Get());

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);
        view.currentProfileCard.onClick?.AddListener(() => { profileCardIsOpen.Set(!profileCardIsOpen.Get()); });
        view.currentRealmViewer.onLogoClick?.AddListener(view.ShowRealmSelectorModal);
        view.OnCloseButtonPressed += OnCloseButtonPressed;
        view.OnAfterShowAnimation += OnAfterShowAnimation;

        DataStore.i.exploreV2.topMenuTooltipReference.Set(topMenuTooltipReference);
        DataStore.i.exploreV2.placesAndEventsTooltipReference.Set(placesAndEventsTooltipReference);
        DataStore.i.exploreV2.backpackTooltipReference.Set(backpackTooltipReference);
        DataStore.i.exploreV2.mapTooltipReference.Set(mapTooltipReference);
        DataStore.i.exploreV2.builderTooltipReference.Set(builderTooltipReference);
        DataStore.i.exploreV2.questTooltipReference.Set(questTooltipReference);
        DataStore.i.exploreV2.settingsTooltipReference.Set(settingsTooltipReference);
        DataStore.i.exploreV2.profileCardTooltipReference.Set(profileCardTooltipReference);

        view.OnSectionOpen += OnSectionOpen;

        isOpen.OnChange += IsOpenChanged;
        IsOpenChanged(isOpen.Get(), false);

        currentSectionIndex.OnChange += CurrentSectionIndexChanged;
        CurrentSectionIndexChanged(currentSectionIndex.Get(), 0);

        isPlacesAndEventsSectionInitialized.OnChange += IsPlacesAndEventsSectionInitializedChanged;
        IsPlacesAndEventsSectionInitializedChanged(isPlacesAndEventsSectionInitialized.Get(), false);
        placesAndEventsVisible.OnChange += PlacesAndEventsVisibleChanged;
        PlacesAndEventsVisibleChanged(placesAndEventsVisible.Get(), false);

        isAvatarEditorInitialized.OnChange += IsAvatarEditorInitializedChanged;
        IsAvatarEditorInitializedChanged(isAvatarEditorInitialized.Get(), false);
        avatarEditorVisible.OnChange += AvatarEditorVisibleChanged;
        AvatarEditorVisibleChanged(avatarEditorVisible.Get(), false);

        isNavmapInitialized.OnChange += IsNavMapInitializedChanged;
        IsNavMapInitializedChanged(isNavmapInitialized.Get(), false);
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
        
        ConfigureOhterUIDependencies();

        isInitialized.Set(true);
    }

    internal void InitializePlacesAndEventsSection()
    {
        if (placesAndEventsSectionController != null)
            return;

        placesAndEventsSectionController = new PlacesAndEventsSectionComponentController(view.currentPlacesAndEventsSection, exploreV2Analytics);
        placesAndEventsSectionController.OnCloseExploreV2 += OnCloseButtonPressed;
    }

    internal void OnSectionOpen(ExploreSection section)
    {
        if (section != currentOpenSection)
            exploreV2Analytics.SendStartMenuSectionVisibility(currentOpenSection, false);

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
        DataStore.i.realm.playerRealm.OnChange -= UpdateRealmInfo;
        DataStore.i.realm.realmsInfo.OnSet -= UpdateAvailableRealmsInfo;
        ownUserProfile.OnUpdate -= UpdateProfileInfo;
        view?.currentProfileCard.onClick?.RemoveAllListeners();
        isOpen.OnChange -= IsOpenChanged;
        currentSectionIndex.OnChange -= CurrentSectionIndexChanged;
        isPlacesAndEventsSectionInitialized.OnChange -= IsPlacesAndEventsSectionInitializedChanged;
        placesAndEventsVisible.OnChange -= PlacesAndEventsVisibleChanged;
        isAvatarEditorInitialized.OnChange -= IsAvatarEditorInitializedChanged;
        avatarEditorVisible.OnChange -= AvatarEditorVisibleChanged;
        isNavmapInitialized.OnChange -= IsNavMapInitializedChanged;
        navmapVisible.OnChange -= NavmapVisibleChanged;
        isBuilderInitialized.OnChange -= IsBuilderInitializedChanged;
        builderVisible.OnChange -= BuilderVisibleChanged;
        isQuestInitialized.OnChange -= IsQuestInitializedChanged;
        questVisible.OnChange -= QuestVisibleChanged;
        isSettingsPanelInitialized.OnChange -= IsSettingsPanelInitializedChanged;
        settingsVisible.OnChange -= SettingsVisibleChanged;

        if (view != null)
        {
            view.currentProfileCard.onClick?.RemoveAllListeners();
            view.currentRealmViewer.onLogoClick?.RemoveAllListeners();
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            view.OnAfterShowAnimation -= OnAfterShowAnimation;
            view.OnSectionOpen -= OnSectionOpen;
            view.Dispose();
        }

        if (placesAndEventsSectionController != null)
        {
            placesAndEventsSectionController.OnCloseExploreV2 -= OnCloseButtonPressed;
            placesAndEventsSectionController.Dispose();
        }
    }

    public void SetVisibility(bool visible) { isOpen.Set(visible); }

    internal void IsOpenChanged(bool current, bool previous) { SetVisibility_Internal(current); }

    internal void CurrentSectionIndexChanged(int current, int previous)
    {
        if (DataStore.i.exploreV2.isInShowAnimationTransiton.Get())
            return;

        if (Enum.IsDefined(typeof(ExploreSection), current))
        {
            if (!view.IsSectionActive((ExploreSection)current))
                CurrentSectionIndexChanged(current + 1, current);
            else
                view.GoToSection((ExploreSection)current);
        }
        else
            view.GoToSection(0);
    }

    internal void SetVisibility_Internal(bool visible)
    {
        if (view == null || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (visible)
        {
            Utils.UnlockCursor();

            if (DataStore.i.common.isTutorialRunning.Get())
                view.GoToSection(ExploreV2MenuComponentView.DEFAULT_SECTION);
        }
        else
        {
            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
            placesAndEventsVisible.Set(false);
            avatarEditorVisible.Set(false);
            profileCardIsOpen.Set(false);
            navmapVisible.Set(false);
            builderVisible.Set(false);
            questVisible.Set(false);
            settingsVisible.Set(false);
        }

        view.SetVisible(visible);
    }

    internal void OnAfterShowAnimation() { CommonScriptableObjects.isFullscreenHUDOpen.Set(true); }

    internal void IsPlacesAndEventsSectionInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Explore, current);

        if (current)
            InitializePlacesAndEventsSection();
    }

    internal void PlacesAndEventsVisibleChanged(bool current, bool previous)
    {
        if (!isInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(ExploreSection.Explore);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Explore, true);
        }
        else if (currentOpenSection == ExploreSection.Explore)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Explore, false);
        }
    }

    internal void IsAvatarEditorInitializedChanged(bool current, bool previous) { view.SetSectionActive(ExploreSection.Backpack, current); }

    internal void AvatarEditorVisibleChanged(bool current, bool previous)
    {
        if (!isAvatarEditorInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(ExploreSection.Backpack);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Backpack, true);
        }
        else if (currentOpenSection == ExploreSection.Backpack)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Backpack, false);
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
        if (!isNavmapInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(ExploreSection.Map);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Map, true);
        }
        else if (currentOpenSection == ExploreSection.Map)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Map, false);
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
        if (!isBuilderInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(ExploreSection.Builder);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Builder, true);
        }
        else if (currentOpenSection == ExploreSection.Builder)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Builder, false);
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
        if (!isQuestInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(ExploreSection.Quest);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Quest, true);
        }
        else if (currentOpenSection == ExploreSection.Quest)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Quest, false);
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
        if (!isSettingsPanelInitialized.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }
            view.GoToSection(ExploreSection.Settings);
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Settings, true);
        }
        else if (currentOpenSection == ExploreSection.Settings)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }
            exploreV2Analytics.SendStartMenuSectionVisibility(ExploreSection.Settings, false);
        }
    }

    internal void UpdateRealmInfo(CurrentRealmModel currentRealm, CurrentRealmModel previousRealm)
    {
        if (currentRealm == null)
            return;

        // Get the name of the current realm
        view.currentRealmViewer.SetRealm(currentRealm.serverName);
        view.currentRealmSelectorModal.SetCurrentRealm(currentRealm.serverName);

        // Calculate number of users in the current realm
        List<RealmModel> realmList = DataStore.i.realm.realmsInfo.Get()?.ToList();
        RealmModel currentRealmModel = realmList?.FirstOrDefault(r => r.serverName == currentRealm.serverName);
        int realmUsers = 0;
        if (currentRealmModel != null)
            realmUsers = currentRealmModel.usersCount;

        view.currentRealmViewer.SetNumberOfUsers(realmUsers);
    }

    internal void UpdateAvailableRealmsInfo(IEnumerable<RealmModel> currentRealmList)
    {
        if (!NeedToRefreshRealms(currentRealmList))
            return;

        currentAvailableRealms.Clear();
        CurrentRealmModel currentRealm = DataStore.i.realm.playerRealm.Get();

        if (currentRealmList != null)
        {
            foreach (RealmModel realmModel in currentRealmList)
            {
                RealmRowComponentModel realmToAdd = new RealmRowComponentModel
                {
                    name = realmModel.serverName,
                    players = realmModel.usersCount,
                    isConnected = realmModel.serverName == currentRealm?.serverName
                };

                currentAvailableRealms.Add(realmToAdd);
            }
        }

        view.currentRealmSelectorModal.SetAvailableRealms(currentAvailableRealms);
    }

    internal bool NeedToRefreshRealms(IEnumerable<RealmModel> newRealmList)
    {
        if (newRealmList == null)
            return true;

        bool needToRefresh = false;
        if (newRealmList.Count() == currentAvailableRealms.Count)
        {
            foreach (RealmModel realm in newRealmList)
            {
                if (!currentAvailableRealms.Exists(x => x.name == realm.serverName && x.players == realm.usersCount))
                {
                    needToRefresh = true;
                    break;
                }
            }
        }
        else
            needToRefresh = true;

        return needToRefresh;
    }

    internal void UpdateProfileInfo(UserProfile profile)
    {
        view.currentProfileCard.SetIsClaimedName(profile.hasClaimedName);
        view.currentProfileCard.SetProfileName(profile.userName);
        view.currentProfileCard.SetProfileAddress(profile.ethAddress);
        view.currentProfileCard.SetProfilePicture(profile.face256SnapshotURL);
    }

    internal void OnCloseButtonPressed(bool fromShortcut)
    {
        if (!fromShortcut)
        {
            SetVisibility(false);
            exploreV2Analytics.SendStartMenuVisibility(false, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
        }
        else
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
            }
            else
            {
                if (Time.realtimeSinceStartup - controlsHUDCloseTime >= MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU &&
                    Time.realtimeSinceStartup - emotesHUDCloseTime >= MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU &&
                    Time.realtimeSinceStartup - playerInfoCardHUDCloseTime >= MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU &&
                    Time.realtimeSinceStartup - chatInputHUDCloseTime >= MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU)
                {
                    SetVisibility(true);
                    exploreV2Analytics.SendStartMenuVisibility(true, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
                }
            }
        }
    }

    internal void ConfigureOhterUIDependencies()
    {
        controlsHUDCloseTime = Time.realtimeSinceStartup;
        controlsVisible.OnChange += (current, old) =>
        {
            if (!current)
                controlsHUDCloseTime = Time.realtimeSinceStartup;
        };

        emotesHUDCloseTime = Time.realtimeSinceStartup;
        emotesVisible.OnChange += (current, old) =>
        {
            if (!current)
                emotesHUDCloseTime = Time.realtimeSinceStartup;
        };

        chatInputHUDCloseTime = Time.realtimeSinceStartup;
        chatInputVisible.OnChange += (current, old) =>
        {
            if (!current)
                chatInputHUDCloseTime = Time.realtimeSinceStartup;
        };

        playerInfoCardHUDCloseTime = Time.realtimeSinceStartup;
        playerInfoCardVisible.OnChange += (current, old) =>
        {
            if (!current)
                playerInfoCardHUDCloseTime = Time.realtimeSinceStartup;
        };
    }

    internal virtual IExploreV2Analytics CreateAnalyticsController() => new ExploreV2Analytics.ExploreV2Analytics();

    protected internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}