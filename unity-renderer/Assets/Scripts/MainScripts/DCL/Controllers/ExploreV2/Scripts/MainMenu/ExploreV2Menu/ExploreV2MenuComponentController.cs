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
    internal const ExploreSection DEFAULT_SECTION = ExploreSection.Explore;

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
    private Dictionary<BaseVariable<bool>, ExploreSection> sectionsByInitVar;
    internal Dictionary<BaseVariable<bool>, ExploreSection> sectionsByVisiblityVar;

    private Dictionary<ExploreSection, (BaseVariable<bool> initVar, BaseVariable<bool> visibilityVar)> sectionsVariables;

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
    internal BaseVariable<bool> isPromoteChannelsToastVisible => DataStore.i.channels.isPromoteToastVisible;

    public void Initialize()
    {
        sectionsVariables = new Dictionary<ExploreSection, (BaseVariable<bool>, BaseVariable<bool>)>
        {
            { ExploreSection.Explore, (isPlacesAndEventsSectionInitialized,  placesAndEventsVisible) },
            { ExploreSection.Backpack, (isAvatarEditorInitialized,  avatarEditorVisible) },
            { ExploreSection.Map, (isNavmapInitialized,  navmapVisible) },
            { ExploreSection.Builder, (isBuilderInitialized,  builderVisible) },
            { ExploreSection.Quest, (isQuestInitialized,  questVisible) },
            { ExploreSection.Settings, (isSettingsPanelInitialized,  settingsVisible) },
        };
        sectionsByInitVar = sectionsVariables.ToDictionary(pair => pair.Value.initVar, pair => pair.Key);
        sectionsByVisiblityVar = sectionsVariables.ToDictionary(pair => pair.Value.visibilityVar, pair => pair.Key);

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

        isOpen.OnChange += SetVisibilityOnOpenChanged;
        SetVisibilityOnOpenChanged(isOpen.Get());

        currentSectionIndex.OnChange += CurrentSectionIndexChanged;
        CurrentSectionIndexChanged(currentSectionIndex.Get(), 0);

        foreach (var sectionsVariables in sectionsVariables.Values)
        {
            sectionsVariables.initVar.OnChangeWithSenderInfo += OnSectionInitializedChanged;
            OnSectionInitializedChanged(sectionsVariables.initVar, sectionsVariables.initVar.Get());

            sectionsVariables.visibilityVar.OnChangeWithSenderInfo += OnSectionVisiblityChanged;
            OnSectionVisiblityChanged(sectionsVariables.visibilityVar, sectionsVariables.visibilityVar.Get());
        }

        ConfigureOtherUIDependencies();

        isInitialized.Set(true);

        currentSectionIndex.Set((int)DEFAULT_SECTION, false);

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

        isOpen.OnChange -= SetVisibilityOnOpenChanged;
        currentSectionIndex.OnChange -= CurrentSectionIndexChanged;

        foreach (var sectionsVariables in sectionsVariables.Values)
        {
            sectionsVariables.initVar.OnChangeWithSenderInfo -= OnSectionInitializedChanged;
            sectionsVariables.visibilityVar.OnChangeWithSenderInfo -= OnSectionVisiblityChanged;
        }

        if (placesAndEventsSectionController != null)
        {
            placesAndEventsSectionController.OnCloseExploreV2 -= OnCloseButtonPressed;
            placesAndEventsSectionController.Dispose();
        }

        if (view != null)
        {
            view.currentProfileCard.onClick?.RemoveAllListeners();
            view.currentRealmViewer.onLogoClick?.RemoveAllListeners();
            view.OnCloseButtonPressed -= OnCloseButtonPressed;
            view.OnAfterShowAnimation -= OnAfterShowAnimation;
            view.OnSectionOpen -= OnSectionOpen;
            view.Dispose();
        }
    }

    public void SetVisibility(bool visible) => isOpen.Set(visible);

    private void OnSectionInitializedChanged(BaseVariable<bool> initVar, bool initialized, bool _ = false) =>
        SectionInitializedChanged(sectionsByInitVar[initVar], initialized);

    internal void SectionInitializedChanged(ExploreSection section, bool initialized, bool _ = false)
    {
        view.SetSectionActive(section, initialized);

        if (section == ExploreSection.Explore && initialized)
            InitializePlacesAndEventsSection();
    }

    private void OnSectionVisiblityChanged(BaseVariable<bool> visibilityVar, bool visible, bool previous = false)
    {
        ExploreSection section = sectionsByVisiblityVar[visibilityVar];
        BaseVariable<bool> initVar = section == ExploreSection.Explore ? isInitialized : sectionsVariables[section].initVar;

        if (!initVar.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        SetMenuTargetVisibility(sectionsByVisiblityVar[visibilityVar], visible);
    }

    internal void InitializePlacesAndEventsSection()
    {
        if (placesAndEventsSectionController != null)
            return;

        placesAndEventsSectionController = new PlacesAndEventsSectionComponentController(view.currentPlacesAndEventsSection, exploreV2Analytics, DataStore.i);
        placesAndEventsSectionController.OnCloseExploreV2 += OnCloseButtonPressed;
    }

    internal void OnSectionOpen(ExploreSection section)
    {
        if (section != currentOpenSection)
            exploreV2Analytics.SendStartMenuSectionVisibility(currentOpenSection, false);

        currentOpenSection = section;

        if (currentOpenSection == ExploreSection.Backpack)
            view.ConfigureEncapsulatedSection(ExploreSection.Backpack, DataStore.i.exploreV2.configureBackpackInFullscreenMenu);

        foreach (var visibilityVar in sectionsByVisiblityVar.Keys)
            visibilityVar.Set(currentOpenSection == sectionsByVisiblityVar[visibilityVar]);

        profileCardIsOpen.Set(false);
    }

    internal void SetVisibilityOnOpenChanged(bool open, bool _ = false) => SetVisibility_Internal(open);

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
                view.GoToSection(DEFAULT_SECTION);

            isPromoteChannelsToastVisible.Set(false);
        }
        else
        {
            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);

            foreach (var sectionsVariables in sectionsVariables.Values)
                sectionsVariables.visibilityVar.Set(false);

            profileCardIsOpen.Set(false);
        }

        view.SetVisible(visible);
    }

    internal void OnAfterShowAnimation() => CommonScriptableObjects.isFullscreenHUDOpen.Set(true);

    internal void SetMenuTargetVisibility(ExploreSection section, bool toVisible, bool _ = false)
    {
        if (toVisible)
        {
            if (currentSectionIndex.Get() != (int)section)
                currentSectionIndex.Set((int)section);

            SetSectionTargetVisibility(section, toVisible: true);
            view.GoToSection(section);
        }
        else if (currentOpenSection == section)
        {
            SetSectionTargetVisibility(section, toVisible: false);
        }
    }

    private void SetSectionTargetVisibility(ExploreSection section, bool toVisible)
    {
        bool wasInTargetVisibility =  toVisible ^ isOpen.Get();

        if (wasInTargetVisibility)
        {
            SetVisibility(toVisible);
            exploreV2Analytics.SendStartMenuVisibility(toVisible, ExploreUIVisibilityMethod.FromShortcut);
        }
        exploreV2Analytics.SendStartMenuSectionVisibility(section, toVisible);
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
        if (DataStore.i.builderInWorld.areShortcutsBlocked.Get() || !isOpen.Get() )
            return;

        SetVisibility(false);
        exploreV2Analytics.SendStartMenuVisibility(false, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
    }

    internal void ConfigureOtherUIDependencies()
    {
        controlsHUDCloseTime = Time.realtimeSinceStartup;
        controlsVisible.OnChange += (current, _) =>
        {
            if (!current)
                controlsHUDCloseTime = Time.realtimeSinceStartup;
        };

        emotesHUDCloseTime = Time.realtimeSinceStartup;
        emotesVisible.OnChange += (current, _) =>
        {
            if (!current)
                emotesHUDCloseTime = Time.realtimeSinceStartup;
        };

        chatInputHUDCloseTime = Time.realtimeSinceStartup;
        chatInputVisible.OnChange += (current, _) =>
        {
            if (!current)
                chatInputHUDCloseTime = Time.realtimeSinceStartup;
        };

        playerInfoCardHUDCloseTime = Time.realtimeSinceStartup;
        playerInfoCardVisible.OnChange += (current, _) =>
        {
            if (!current)
                playerInfoCardHUDCloseTime = Time.realtimeSinceStartup;
        };
    }

    internal virtual IExploreV2Analytics CreateAnalyticsController() => new ExploreV2Analytics.ExploreV2Analytics();

    protected internal virtual IExploreV2MenuComponentView CreateView() => ExploreV2MenuComponentView.Create();
}