using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using ExploreV2Analytics;
using UnityEngine;
using Variables.RealmsInfo;

/// <summary>
/// Main controller for the feature "Explore V2".
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    internal const float MIN_TIME_AFTER_CLOSE_OTHER_UI_TO_OPEN_START_MENU = 0.1f;
    internal readonly List<RealmRowComponentModel> currentAvailableRealms = new List<RealmRowComponentModel>();
    internal float chatInputHUDCloseTime = 0f;
    internal float controlsHUDCloseTime = 0f;
    internal ExploreSection currentOpenSection;
    internal float emotesHUDCloseTime = 0f;
    internal IExploreV2Analytics exploreV2Analytics;
    internal MouseCatcher mouseCatcher;
    internal IPlacesAndEventsSectionComponentController placesAndEventsSectionController;
    internal float playerInfoCardHUDCloseTime = 0f;

    internal IExploreV2MenuComponentView view;

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    internal RectTransform topMenuTooltipReference => view.currentTopMenuTooltipReference;
    internal RectTransform placesAndEventsTooltipReference => view.currentPlacesAndEventsTooltipReference;
    internal RectTransform backpackTooltipReference => view.currentBackpackTooltipReference;
    internal RectTransform mapTooltipReference => view.currentMapTooltipReference;
    internal RectTransform builderTooltipReference => view.currentBuilderTooltipReference;
    internal RectTransform questTooltipReference => view.currentQuestTooltipReference;
    internal RectTransform settingsTooltipReference => view.currentSettingsTooltipReference;
    internal RectTransform profileCardTooltipReference => view.currentProfileCardTooltipReference;

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

    private Dictionary<ExploreSection, BaseVariable<bool>> SectionIsInitialized => new Dictionary<ExploreSection, BaseVariable<bool>>
    {
        { ExploreSection.Explore, isInitialized },
        { ExploreSection.Backpack, isAvatarEditorInitialized },
        { ExploreSection.Map, isNavmapInitialized },
        { ExploreSection.Builder, isBuilderInitialized },
        { ExploreSection.Quest, isQuestInitialized },
        { ExploreSection.Settings, isSettingsPanelInitialized },
    };

    private Dictionary<BaseVariable<bool>, ExploreSection> SectionVisibleEnumMap => new Dictionary<BaseVariable<bool>, ExploreSection>
    {
        { placesAndEventsVisible, ExploreSection.Explore },
        { avatarEditorVisible, ExploreSection.Backpack },
        { navmapVisible, ExploreSection.Map },
        { builderVisible, ExploreSection.Builder },
        { questVisible, ExploreSection.Quest },
        { settingsVisible, ExploreSection.Settings },
    };

    public void Initialize()
    {
        mouseCatcher = SceneReferences.i?.mouseCatcher;
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

        isAvatarEditorInitialized.OnChange += IsAvatarEditorInitializedChanged;
        IsAvatarEditorInitializedChanged(isAvatarEditorInitialized.Get(), false);

        isNavmapInitialized.OnChange += IsNavMapInitializedChanged;
        IsNavMapInitializedChanged(isNavmapInitialized.Get(), false);

        isBuilderInitialized.OnChange += IsBuilderInitializedChanged;
        IsBuilderInitializedChanged(isBuilderInitialized.Get(), false);

        isQuestInitialized.OnChange += IsQuestInitializedChanged;
        IsQuestInitializedChanged(isQuestInitialized.Get(), false);

        isSettingsPanelInitialized.OnChange += IsSettingsPanelInitializedChanged;
        IsSettingsPanelInitializedChanged(isSettingsPanelInitialized.Get(), false);

        foreach (var sectionVisible in SectionVisibleEnumMap.Keys)
        {
            sectionVisible.OnChangeWithSenderInfo += OnSectionVisiblityChanged;
            OnSectionVisiblityChanged(sectionVisible, sectionVisible.Get());
        }

        ConfigureOtherUIDependencies();

        isInitialized.Set(true);

        //view.ConfigureEncapsulatedSection(ExploreSection.Backpack, DataStore.i.exploreV2.configureBackpackInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Map, DataStore.i.exploreV2.configureMapInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Builder, DataStore.i.exploreV2.configureBuilderInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Quest, DataStore.i.exploreV2.configureQuestInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Settings, DataStore.i.exploreV2.configureSettingsInFullscreenMenu);
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
        isAvatarEditorInitialized.OnChange -= IsAvatarEditorInitializedChanged;
        isNavmapInitialized.OnChange -= IsNavMapInitializedChanged;
        isBuilderInitialized.OnChange -= IsBuilderInitializedChanged;
        isQuestInitialized.OnChange -= IsQuestInitializedChanged;
        isSettingsPanelInitialized.OnChange -= IsSettingsPanelInitializedChanged;

        foreach (var sectionVisible in SectionVisibleEnumMap.Keys)
            sectionVisible.OnChangeWithSenderInfo -= OnSectionVisiblityChanged;

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

    private void OnSectionVisiblityChanged(BaseVariable<bool> sender, bool current, bool previous = false) =>
        SectionVisibilityChanged(current, SectionVisibleEnumMap[sender]);

    internal void IsAvatarEditorInitializedChanged(bool current, bool previous) => view.SetSectionActive(ExploreSection.Backpack, current);
    internal void IsNavMapInitializedChanged(bool current, bool previous) => view.SetSectionActive(ExploreSection.Map, current);
    internal void IsBuilderInitializedChanged(bool current, bool previous) => view.SetSectionActive(ExploreSection.Builder, current);
    internal void IsQuestInitializedChanged(bool current, bool previous) => view.SetSectionActive(ExploreSection.Quest, current);
    internal void IsSettingsPanelInitializedChanged(bool current, bool previous) => view.SetSectionActive(ExploreSection.Settings, current);

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
            mouseCatcher?.UnlockCursor();

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

    internal void OnAfterShowAnimation() => CommonScriptableObjects.isFullscreenHUDOpen.Set(true);

    internal void IsPlacesAndEventsSectionInitializedChanged(bool current, bool previous)
    {
        view.SetSectionActive(ExploreSection.Explore, current);

        if (current)
            InitializePlacesAndEventsSection();
    }

    internal void SectionVisibilityChanged(bool current, ExploreSection section, bool _ = false)
    {
        if (!SectionIsInitialized[section].Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (current)
        {
            if (!isOpen.Get())
            {
                SetVisibility(true);
                exploreV2Analytics.SendStartMenuVisibility(true, ExploreUIVisibilityMethod.FromShortcut);
            }

            view.GoToSection(section);
            exploreV2Analytics.SendStartMenuSectionVisibility(section, true);
        }
        else if (currentOpenSection == section)
        {
            if (isOpen.Get())
            {
                SetVisibility(false);
                exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
            }

            exploreV2Analytics.SendStartMenuSectionVisibility(section, false);
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
        if (DataStore.i.builderInWorld.areShortcutsBlocked.Get())
            return;

        if (!fromShortcut)
        {
            SetVisibility(false);
            exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromClick);
        }
        else if (isOpen.Get())
        {
            SetVisibility(false);
            exploreV2Analytics.SendStartMenuVisibility(false, ExploreUIVisibilityMethod.FromShortcut);
        }
    }

    internal void ConfigureOtherUIDependencies()
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