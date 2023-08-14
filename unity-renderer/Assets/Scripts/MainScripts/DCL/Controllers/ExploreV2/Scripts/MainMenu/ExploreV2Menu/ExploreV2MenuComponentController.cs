using DCL;
using DCL.Social.Friends;
using DCL.Wallet;
using DCLServices.PlacesAPIService;
using ExploreV2Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Environment = DCL.Environment;

/// <summary>
/// Main controller for the feature "Explore V2".
/// Controls different sections: Maps, Backpack, Settings
/// their initialization and switching between them
/// </summary>
public class ExploreV2MenuComponentController : IExploreV2MenuComponentController
{
    private readonly IPlacesAPIService placesAPIService;
    private readonly IPlacesAnalytics placesAnalytics;

    // TODO: Refactor the ExploreV2MenuComponentController class in order to inject UserProfileWebInterfaceBridge, theGraph and DataStore

    internal const ExploreSection DEFAULT_SECTION = ExploreSection.Explore;

    internal IExploreV2MenuComponentView view;
    internal IExploreV2Analytics exploreV2Analytics;

    internal IPlacesAndEventsSectionComponentController placesAndEventsSectionController;

    internal ExploreSection currentOpenSection;
    internal Dictionary<BaseVariable<bool>, ExploreSection> sectionsByVisibilityVar;
    private ExploreV2ComponentRealmsController realmController;
    private WalletCardHUDController walletCardHUDController;

    private MouseCatcher mouseCatcher;
    private Dictionary<BaseVariable<bool>, ExploreSection> sectionsByInitVar;
    private Dictionary<ExploreSection, (BaseVariable<bool> initVar, BaseVariable<bool> visibilityVar)> sectionsVariables;

    internal BaseVariable<bool> isOpen => DataStore.i.exploreV2.isOpen;
    internal BaseVariable<bool> isInitialized => DataStore.i.exploreV2.isInitialized;
    internal BaseVariable<bool> profileCardIsOpen => DataStore.i.exploreV2.profileCardIsOpen;
    internal BaseVariable<bool> placesAndEventsVisible => DataStore.i.exploreV2.placesAndEventsVisible;
    internal BaseVariable<bool> isAvatarEditorInitialized => DataStore.i.HUDs.isAvatarEditorInitialized;
    internal BaseVariable<bool> avatarEditorVisible => DataStore.i.HUDs.avatarEditorVisible;
    internal BaseVariable<bool> isCameraReelInitialized => DataStore.i.HUDs.isCameraReelInitialized;
    internal BaseVariable<bool> cameraReelSectionVisible => DataStore.i.HUDs.cameraReelSectionVisible;
    internal BaseVariable<bool> isNavmapInitialized => DataStore.i.HUDs.isNavMapInitialized;
    internal BaseVariable<bool> navmapVisible => DataStore.i.HUDs.navmapVisible;
    internal BaseVariable<bool> isQuestInitialized => DataStore.i.Quests.isInitialized;
    internal BaseVariable<bool> questVisible => DataStore.i.HUDs.questsPanelVisible;
    internal BaseVariable<bool> isSettingsPanelInitialized => DataStore.i.settings.isInitialized;
    internal BaseVariable<bool> settingsVisible => DataStore.i.settings.settingsPanelVisible;
    internal BaseVariable<bool> isWalletInitialized => DataStore.i.wallet.isInitialized;
    internal BaseVariable<bool> walletVisible => DataStore.i.wallet.isWalletSectionVisible;
    internal BaseVariable<bool> isMyAccountInitialized => DataStore.i.myAccount.isInitialized;
    internal BaseVariable<bool> myAccountVisible => DataStore.i.myAccount.isMyAccountSectionVisible;

    internal BaseVariable<int> currentSectionIndex => DataStore.i.exploreV2.currentSectionIndex;
    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private BaseVariable<bool> isPlacesAndEventsSectionInitialized => DataStore.i.exploreV2.isPlacesAndEventsSectionInitialized;
    private BaseVariable<bool> isPromoteChannelsToastVisible => DataStore.i.channels.isPromoteToastVisible;

    private RectTransform topMenuTooltipReference => view.currentTopMenuTooltipReference;
    private RectTransform placesAndEventsTooltipReference => view.currentPlacesAndEventsTooltipReference;
    private RectTransform backpackTooltipReference => view.currentBackpackTooltipReference;
    private RectTransform mapTooltipReference => view.currentMapTooltipReference;
    private RectTransform questTooltipReference => view.currentQuestTooltipReference;
    private RectTransform settingsTooltipReference => view.currentSettingsTooltipReference;
    private RectTransform profileCardTooltipReference => view.currentProfileCardTooltipReference;

    public ExploreV2MenuComponentController(IPlacesAPIService placesAPIService, IPlacesAnalytics placesAnalytics)
    {
        this.placesAPIService = placesAPIService;
        this.placesAnalytics = placesAnalytics;
    }

    public void Initialize()
    {
        sectionsVariables = new Dictionary<ExploreSection, (BaseVariable<bool>, BaseVariable<bool>)>
        {
            { ExploreSection.Explore, (isPlacesAndEventsSectionInitialized, placesAndEventsVisible) },
            { ExploreSection.Quest, (isQuestInitialized, questVisible) },
            { ExploreSection.Backpack, (isAvatarEditorInitialized, avatarEditorVisible) },
            { ExploreSection.Map, (isNavmapInitialized, navmapVisible) },
            { ExploreSection.CameraReel, (isCameraReelInitialized, cameraReelSectionVisible) },
            { ExploreSection.Settings, (isSettingsPanelInitialized, settingsVisible) },
            { ExploreSection.Wallet, (isWalletInitialized, walletVisible) },
            { ExploreSection.MyAccount, (isMyAccountInitialized, myAccountVisible) },
        };

        sectionsByInitVar = sectionsVariables.ToDictionary(pair => pair.Value.initVar, pair => pair.Key);
        sectionsByVisibilityVar = sectionsVariables.ToDictionary(pair => pair.Value.visibilityVar, pair => pair.Key);

        mouseCatcher = SceneReferences.i?.mouseCatcher;
        exploreV2Analytics = CreateAnalyticsController();

        view = CreateView();
        SetVisibility(false);

        realmController = new ExploreV2ComponentRealmsController(DataStore.i.realm, view);
        realmController.Initialize();
        view.currentRealmViewer.onLogoClick?.AddListener(view.ShowRealmSelectorModal);

        if (isWalletInitialized.Get())
            OnWalletInitialized(true, false);
        else
            isWalletInitialized.OnChange += OnWalletInitialized;

        if (isMyAccountInitialized.Get())
            OnMyAccountInitialized(true, false);
        else
            isMyAccountInitialized.OnChange += OnMyAccountInitialized;

        ownUserProfile.OnUpdate += UpdateProfileInfo;
        UpdateProfileInfo(ownUserProfile);
        view.currentProfileCard.onClick?.AddListener(() => { profileCardIsOpen.Set(!profileCardIsOpen.Get()); });

        view.OnCloseButtonPressed += OnCloseButtonPressed;
        view.OnAfterShowAnimation += OnAfterShowAnimation;

        DataStore.i.exploreV2.topMenuTooltipReference.Set(topMenuTooltipReference);
        DataStore.i.exploreV2.placesAndEventsTooltipReference.Set(placesAndEventsTooltipReference);
        DataStore.i.exploreV2.questTooltipReference.Set(questTooltipReference);
        DataStore.i.exploreV2.backpackTooltipReference.Set(backpackTooltipReference);
        DataStore.i.exploreV2.mapTooltipReference.Set(mapTooltipReference);
        DataStore.i.exploreV2.settingsTooltipReference.Set(settingsTooltipReference);
        DataStore.i.exploreV2.profileCardTooltipReference.Set(profileCardTooltipReference);

        view.OnSectionOpen += OnSectionOpen;

        isOpen.OnChange += SetVisibilityOnOpenChanged;
        SetVisibilityOnOpenChanged(isOpen.Get());

        currentSectionIndex.OnChange += CurrentSectionIndexChanged;
        CurrentSectionIndexChanged(currentSectionIndex.Get(), 0);

        foreach ((BaseVariable<bool> initVar, BaseVariable<bool> visibilityVar) sectionsVars in sectionsVariables.Values)
        {
            sectionsVars.initVar.OnChangeWithSenderInfo += OnSectionInitializedChanged;
            OnSectionInitializedChanged(sectionsVars.initVar, sectionsVars.initVar.Get());

            sectionsVars.visibilityVar.OnChangeWithSenderInfo += OnSectionVisibilityChanged;
            OnSectionVisibilityChanged(sectionsVars.visibilityVar, sectionsVars.visibilityVar.Get());
        }

        isInitialized.Set(true);

        currentSectionIndex.Set((int)DEFAULT_SECTION, false);

        view.ConfigureEncapsulatedSection(ExploreSection.Map, DataStore.i.exploreV2.configureMapInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Settings, DataStore.i.exploreV2.configureSettingsInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.CameraReel, DataStore.i.exploreV2.configureCameraReelInFullScreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.Wallet, DataStore.i.exploreV2.configureWalletSectionInFullscreenMenu);
        view.ConfigureEncapsulatedSection(ExploreSection.MyAccount, DataStore.i.exploreV2.configureMyAccountSectionInFullscreenMenu);

        DataStore.i.common.isWorld.OnChange += OnWorldChange;
    }

    public void Dispose()
    {
        realmController.Dispose();

        ownUserProfile.OnUpdate -= UpdateProfileInfo;
        view?.currentProfileCard.onClick?.RemoveAllListeners();

        isOpen.OnChange -= SetVisibilityOnOpenChanged;
        currentSectionIndex.OnChange -= CurrentSectionIndexChanged;

        foreach ((BaseVariable<bool> initVar, BaseVariable<bool> visibilityVar) sectionsVars in sectionsVariables.Values)
        {
            sectionsVars.initVar.OnChangeWithSenderInfo -= OnSectionInitializedChanged;
            sectionsVars.visibilityVar.OnChangeWithSenderInfo -= OnSectionVisibilityChanged;
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

        DataStore.i.common.isWorld.OnChange -= OnWorldChange;

        isWalletInitialized.OnChange -= OnWalletInitialized;
        walletCardHUDController?.Dispose();

        isMyAccountInitialized.OnChange -= OnMyAccountInitialized;
    }

    protected internal virtual IExploreV2MenuComponentView CreateView() =>
        ExploreV2MenuComponentView.Create();

    private void OnWalletInitialized(bool current, bool previous)
    {
        if (!current)
            return;

        isWalletInitialized.OnChange -= OnWalletInitialized;

        walletCardHUDController = new WalletCardHUDController(
            view.currentWalletCard,
            new UserProfileWebInterfaceBridge(),
            Environment.i.platform.serviceProviders.theGraph,
            DataStore.i);
    }

    private void OnMyAccountInitialized(bool current, bool previous)
    {
        if (!current)
            return;

        isMyAccountInitialized.OnChange -= OnMyAccountInitialized;
    }

    internal virtual IExploreV2Analytics CreateAnalyticsController() =>
        new ExploreV2Analytics.ExploreV2Analytics();

    internal void InitializePlacesAndEventsSection()
    {
        if (placesAndEventsSectionController != null)
            return;

        placesAndEventsSectionController = new PlacesAndEventsSectionComponentController(
            view.currentPlacesAndEventsSection, exploreV2Analytics, DataStore.i, new UserProfileWebInterfaceBridge(),
            Environment.i.serviceLocator.Get<IFriendsController>(), placesAPIService, placesAnalytics);

        placesAndEventsSectionController.OnCloseExploreV2 += OnCloseButtonPressed;
    }

    private void OnSectionInitializedChanged(BaseVariable<bool> initVar, bool initialized, bool _ = false) =>
        SectionInitializedChanged(sectionsByInitVar[initVar], initialized);

    internal void SectionInitializedChanged(ExploreSection section, bool initialized, bool _ = false)
    {
        view.SetSectionActive(section, initialized);

        if (section == ExploreSection.Explore && initialized)
            InitializePlacesAndEventsSection();
    }

    public void SetVisibility(bool visible) =>
        isOpen.Set(visible);

    internal void SetVisibilityOnOpenChanged(bool open, bool _ = false) =>
        SetVisibility_Internal(open);

    internal void SetMenuTargetVisibility(ExploreSection section, bool toVisible, bool _ = false)
    {
        if (toVisible)
        {
            if (currentSectionIndex.Get() != (int)section)
                currentSectionIndex.Set((int)section);

            SetSectionTargetVisibility(section, toVisible: true);
        }
        else if (currentOpenSection == section)
            SetSectionTargetVisibility(section, toVisible: false);
    }

    private void SetSectionTargetVisibility(ExploreSection section, bool toVisible)
    {
        bool wasInTargetVisibility = toVisible ^ isOpen.Get();

        if (wasInTargetVisibility)
        {
            SetVisibility(toVisible);
            exploreV2Analytics.SendStartMenuVisibility(toVisible, ExploreUIVisibilityMethod.FromShortcut);
        }

        exploreV2Analytics.SendStartMenuSectionVisibility(section, toVisible);
    }

    private void SetVisibility_Internal(bool visible)
    {
        if (view == null || DataStore.i.common.isSignUpFlow.Get())
            return;

        if (visible)
        {
            // TODO: This is temporal while we want to keep the NEW tag for the new Backpack feature
            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("backpack_editor_v2"))
                view.SetSectionAsNew(ExploreSection.Backpack, false);

            view.SetWalletActive(isWalletInitialized.Get(), ownUserProfile.isGuest);

            if (mouseCatcher != null)
                mouseCatcher.UnlockCursor();

            if (DataStore.i.common.isTutorialRunning.Get())
                view.GoToSection(DEFAULT_SECTION);

            isPromoteChannelsToastVisible.Set(false);
        }
        else
        {
            CommonScriptableObjects.isFullscreenHUDOpen.Set(false);

            foreach ((BaseVariable<bool> initVar, BaseVariable<bool> visibilityVar) sectionsVars in sectionsVariables.Values)
                sectionsVars.visibilityVar.Set(false);

            profileCardIsOpen.Set(false);
        }

        DataStore.i.wallet.isWalletCardVisible.Set(visible);
        view.SetVisible(visible);
    }

    private void OnSectionVisibilityChanged(BaseVariable<bool> visibilityVar, bool visible, bool previous = false)
    {
        ExploreSection section = sectionsByVisibilityVar[visibilityVar];

        if (section is ExploreSection.Wallet or ExploreSection.MyAccount)
            return;

        BaseVariable<bool> initVar = section == ExploreSection.Explore ? isInitialized : sectionsVariables[section].initVar;

        if (!initVar.Get() || DataStore.i.common.isSignUpFlow.Get())
            return;

        SetMenuTargetVisibility(sectionsByVisibilityVar[visibilityVar], visible);
    }

    private void ChangeVisibilityVarForSwitchedSections()
    {
        foreach (BaseVariable<bool> visibilityVar in sectionsByVisibilityVar.Keys)
            if (visibilityVar.Get() != (currentOpenSection == sectionsByVisibilityVar[visibilityVar]))
                visibilityVar.Set(currentOpenSection == sectionsByVisibilityVar[visibilityVar]);
    }

    internal void OnSectionOpen(ExploreSection section)
    {
        if (section != currentOpenSection)
            exploreV2Analytics.SendStartMenuSectionVisibility(currentOpenSection, false);

        currentOpenSection = section;

        if (currentOpenSection == ExploreSection.Backpack)
            view.ConfigureEncapsulatedSection(ExploreSection.Backpack, DataStore.i.exploreV2.configureBackpackInFullscreenMenu);
        if (currentOpenSection == ExploreSection.Quest)
            view.ConfigureEncapsulatedSection(ExploreSection.Quest, DataStore.i.exploreV2.configureQuestInFullscreenMenu);
        if (currentOpenSection == ExploreSection.CameraReel)
            DataStore.i.HUDs.cameraReelOpenSource.Set("Menu");

        ChangeVisibilityVarForSwitchedSections();

        profileCardIsOpen.Set(false);
    }

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

    private static void OnAfterShowAnimation() =>
        CommonScriptableObjects.isFullscreenHUDOpen.Set(true);

    internal void UpdateProfileInfo(UserProfile profile)
    {
        view.currentProfileCard.SetIsClaimedName(profile.hasClaimedName);
        view.currentProfileCard.SetProfileName(profile.userName);
        view.currentProfileCard.SetProfileAddress(profile.ethAddress);
        view.currentProfileCard.SetProfilePicture(profile.face256SnapshotURL);
    }

    internal void OnCloseButtonPressed(bool fromShortcut)
    {
        SetVisibility(false);
        exploreV2Analytics.SendStartMenuVisibility(false, fromShortcut ? ExploreUIVisibilityMethod.FromShortcut : ExploreUIVisibilityMethod.FromClick);
    }

    private void OnWorldChange(bool isWorld, bool wasWorld)
    {
        if (isWorld == wasWorld) return;

        if (isWorld && view.IsSectionActive(ExploreSection.Map))
            view.HideMapOnEnteringWorld();

        view.SetSectionActive(ExploreSection.Map, !isWorld);
    }
}
