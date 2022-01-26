using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExploreV2MenuComponentView : IDisposable
{
    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action<bool> OnCloseButtonPressed;

    /// <summary>
    /// It will be triggered when a section is open.
    /// </summary>
    event Action<ExploreSection> OnSectionOpen;

    /// <summary>
    /// It will be triggered after the show animation has finished.
    /// </summary>
    event Action OnAfterShowAnimation;

    /// <summary>
    /// Real viewer component.
    /// </summary>
    IRealmViewerComponentView currentRealmViewer { get; }

    /// <summary>
    /// Realm Selector component.
    /// </summary>
    IRealmSelectorComponentView currentRealmSelectorModal { get; }

    /// <summary>
    /// Profile card component.
    /// </summary>
    IProfileCardComponentView currentProfileCard { get; }

    /// <summary>
    /// Places and Events section component.
    /// </summary>
    IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection { get; }

    /// <summary>
    /// Transform used to positionate the top menu tooltips.
    /// </summary>
    RectTransform currentTopMenuTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the places and events section tooltips.
    /// </summary>
    RectTransform currentPlacesAndEventsTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the backpack section tooltips.
    /// </summary>
    RectTransform currentBackpackTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the map section tooltips.
    /// </summary>
    RectTransform currentMapTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the builder section tooltips.
    /// </summary>
    RectTransform currentBuilderTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the quest section tooltips.
    /// </summary>
    RectTransform currentQuestTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the settings section tooltips.
    /// </summary>
    RectTransform currentSettingsTooltipReference { get; }

    /// <summary>
    /// Transform used to positionate the profile section tooltips.
    /// </summary>
    RectTransform currentProfileCardTooltipReference { get; }

    /// <summary>
    /// Shows/Hides the game object of the explore menu.
    /// </summary>
    /// <param name="isActive">True to show it.</param>
    void SetVisible(bool isActive);

    /// <summary>
    /// It is called after the show animation has finished.
    /// </summary>
    void OnAfterShowAnimationCompleted();

    /// <summary>
    /// Open a section.
    /// </summary>
    /// <param name="section">Section to go.</param>
    void GoToSection(ExploreSection section);

    /// <summary>
    /// Activates/Deactivates a section in the selector.
    /// </summary>
    /// <param name="section">Section to activate/deactivate.</param>
    /// <param name="isActive">True for activating.</param>
    void SetSectionActive(ExploreSection section, bool isActive);

    /// <summary>
    /// Check if a section is actived or not.
    /// </summary>
    /// <param name="section">Section to check.</param>
    /// <returns></returns>
    bool IsSectionActive(ExploreSection section);

    /// <summary>
    /// Configures a encapsulated section.
    /// </summary>
    /// <param name="section">Section to configure.</param>
    /// <param name="featureConfiguratorFlag">Flag used to configurates the feature.</param>
    void ConfigureEncapsulatedSection(ExploreSection section, BaseVariable<Transform> featureConfiguratorFlag);

    /// <summary>
    /// Shows the Realm Selector modal.
    /// </summary>
    void ShowRealmSelectorModal();
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
    [Header("Assets References")]
    [SerializeField] internal RealmSelectorComponentView realmSelectorModalPrefab;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView sectionSelector;
    [SerializeField] internal ProfileCardComponentView profileCard;
    [SerializeField] internal RealmViewerComponentView realmViewer;
    [SerializeField] internal ButtonComponentView closeMenuButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Sections")]
    [SerializeField] internal PlacesAndEventsSectionComponentView placesAndEventsSection;
    [SerializeField] internal FeatureEncapsulatorComponentView backpackSection;
    [SerializeField] internal FeatureEncapsulatorComponentView mapSection;
    [SerializeField] internal FeatureEncapsulatorComponentView builderSection;
    [SerializeField] internal FeatureEncapsulatorComponentView questSection;
    [SerializeField] internal FeatureEncapsulatorComponentView settingsSection;

    [Header("Tutorial References")]
    [SerializeField] internal RectTransform profileCardTooltipReference;

    internal const ExploreSection DEFAULT_SECTION = ExploreSection.Explore;
    internal const string REALM_SELECTOR_MODAL_ID = "RealmSelector_Modal";

    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IRealmSelectorComponentView currentRealmSelectorModal => realmSelectorModal;
    public IProfileCardComponentView currentProfileCard => profileCard;
    public IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection => placesAndEventsSection;
    public RectTransform currentTopMenuTooltipReference => sectionSelector.GetSection((int) ExploreSection.Explore).pivot;
    public RectTransform currentPlacesAndEventsTooltipReference => sectionSelector.GetSection((int)ExploreSection.Explore).pivot;
    public RectTransform currentBackpackTooltipReference => sectionSelector.GetSection((int)ExploreSection.Backpack).pivot;
    public RectTransform currentMapTooltipReference => sectionSelector.GetSection((int)ExploreSection.Map).pivot;
    public RectTransform currentBuilderTooltipReference => sectionSelector.GetSection((int)ExploreSection.Builder).pivot;
    public RectTransform currentQuestTooltipReference => sectionSelector.GetSection((int)ExploreSection.Quest).pivot;
    public RectTransform currentSettingsTooltipReference => sectionSelector.GetSection((int)ExploreSection.Settings).pivot;
    public RectTransform currentProfileCardTooltipReference => profileCardTooltipReference;

    public event Action<bool> OnCloseButtonPressed;
    public event Action<ExploreSection> OnSectionOpen;
    public event Action OnAfterShowAnimation;

    internal RectTransform profileCardRectTranform;
    internal RealmSelectorComponentView realmSelectorModal;

    public override void Awake()
    {
        base.Awake();

        profileCardRectTranform = profileCard.GetComponent<RectTransform>();
        realmSelectorModal = ConfigureRealmSelectorModal();
    }

    public override void Start()
    {
        DataStore.i.exploreV2.currentSectionIndex.Set((int)DEFAULT_SECTION, false);

        DataStore.i.exploreV2.isInitialized.OnChange += IsInitialized_OnChange;
        IsInitialized_OnChange(DataStore.i.exploreV2.isInitialized.Get(), false);

        ConfigureCloseButton();
    }

    private void IsInitialized_OnChange(bool current, bool previous)
    {
        if (!current)
            return;

        DataStore.i.exploreV2.isInitialized.OnChange -= IsInitialized_OnChange;
        StartCoroutine(CreateSectionSelectorMappingsAfterDelay());
    }

    public override void Update() { CheckIfProfileCardShouldBeClosed(); }

    public override void RefreshControl()
    {
        placesAndEventsSection.RefreshControl();
        backpackSection.RefreshControl();
        mapSection.RefreshControl();
        builderSection.RefreshControl();
        questSection.RefreshControl();
        settingsSection.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        RemoveSectionSelectorMappings();
        closeMenuButton.onClick.RemoveAllListeners();
        closeAction.OnTriggered -= OnCloseActionTriggered;
        DataStore.i.exploreV2.isSomeModalOpen.OnChange -= IsSomeModalOpen_OnChange;
        DataStore.i.exploreV2.isInitialized.OnChange -= IsInitialized_OnChange;

        if (realmSelectorModal != null)
            realmSelectorModal.Dispose();
    }

    public void SetVisible(bool isActive)
    {
        if (isActive)
        {
            DataStore.i.exploreV2.isInShowAnimationTransiton.Set(true);
            Show();

            ISectionToggle sectionToGo = sectionSelector.GetSection(DataStore.i.exploreV2.currentSectionIndex.Get());
            if (sectionToGo != null && sectionToGo.IsActive())
                GoToSection((ExploreSection)DataStore.i.exploreV2.currentSectionIndex.Get());
            else
            {
                List<ISectionToggle> allSections = sectionSelector.GetAllSections();
                foreach (ISectionToggle section in allSections)
                {
                    if (section != null && section.IsActive())
                    {
                        section.SelectToggle(true);
                        break;
                    }
                }
            }
        }
        else
        {
            Hide();
            AudioScriptableObjects.dialogClose.Play(true);
        }
    }

    public void OnAfterShowAnimationCompleted()
    {
        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);
        OnAfterShowAnimation?.Invoke();
    }

    public void GoToSection(ExploreSection section)
    {
        if (DataStore.i.exploreV2.currentSectionIndex.Get() != (int)section)
            DataStore.i.exploreV2.currentSectionIndex.Set((int)section);

        sectionSelector.GetSection((int)section)?.SelectToggle(true);

        AudioScriptableObjects.dialogOpen.Play(true);
        AudioScriptableObjects.listItemAppear.ResetPitch();
    }

    public void SetSectionActive(ExploreSection section, bool isActive) { sectionSelector.GetSection((int)section).SetActive(isActive); }

    public bool IsSectionActive(ExploreSection section) { return sectionSelector.GetSection((int)section).IsActive(); }

    public void ConfigureEncapsulatedSection(ExploreSection section, BaseVariable<Transform> featureConfiguratorFlag)
    {
        FeatureEncapsulatorComponentView sectionView = null;
        switch (section)
        {
            case ExploreSection.Backpack:
                sectionView = backpackSection;
                break;
            case ExploreSection.Map:
                sectionView = mapSection;
                break;
            case ExploreSection.Builder:
                sectionView = builderSection;
                break;
            case ExploreSection.Quest:
                sectionView = questSection;
                break;
            case ExploreSection.Settings:
                sectionView = settingsSection;
                break;
        }

        sectionView?.EncapsulateFeature(featureConfiguratorFlag);
    }

    public IEnumerator CreateSectionSelectorMappingsAfterDelay()
    {
        yield return null;
        CreateSectionSelectorMappings();
    }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection((int)ExploreSection.Explore)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Explore, false);
                               OnSectionOpen?.Invoke(ExploreSection.Explore);
                           }
                       });

        sectionSelector.GetSection((int)ExploreSection.Backpack)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               backpackSection.Show();
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Backpack, false);
                               OnSectionOpen?.Invoke(ExploreSection.Backpack);
                           }
                           else
                               backpackSection.Hide();
                       });

        sectionSelector.GetSection((int)ExploreSection.Map)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               mapSection.Show();
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Map, false);
                               OnSectionOpen?.Invoke(ExploreSection.Map);
                           }
                           else
                               mapSection.Hide();
                       });

        sectionSelector.GetSection((int)ExploreSection.Builder)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               builderSection.Show();
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Builder, false);
                               OnSectionOpen?.Invoke(ExploreSection.Builder);
                           }
                           else
                               builderSection.Hide();
                       });

        sectionSelector.GetSection((int)ExploreSection.Quest)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               questSection.Show();
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Quest, false);
                               OnSectionOpen?.Invoke(ExploreSection.Quest);
                           }
                           else
                               questSection.Hide();
                       });

        sectionSelector.GetSection((int)ExploreSection.Settings)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               settingsSection.Show();
                               DataStore.i.exploreV2.currentSectionIndex.Set((int)ExploreSection.Settings, false);
                               OnSectionOpen?.Invoke(ExploreSection.Settings);
                           }
                           else
                               settingsSection.Hide();
                       });
    }

    internal void RemoveSectionSelectorMappings()
    {
        sectionSelector.GetSection((int)ExploreSection.Explore)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Backpack)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Map)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Builder)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Quest)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Settings)?.onSelect.RemoveAllListeners();
    }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke(false));
        closeAction.OnTriggered += OnCloseActionTriggered;
        DataStore.i.exploreV2.isSomeModalOpen.OnChange += IsSomeModalOpen_OnChange;
    }

    internal void IsSomeModalOpen_OnChange(bool current, bool previous)
    {
        closeAction.OnTriggered -= OnCloseActionTriggered;

        if (!current)
            closeAction.OnTriggered += OnCloseActionTriggered;
    }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { OnCloseButtonPressed?.Invoke(true); }

    internal void CheckIfProfileCardShouldBeClosed()
    {
        if (!DataStore.i.exploreV2.profileCardIsOpen.Get())
            return;

        if (Input.GetMouseButton(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(profileCardRectTranform, Input.mousePosition, Camera.main) &&
            !RectTransformUtility.RectangleContainsScreenPoint(HUDController.i.profileHud.view.expandedMenu, Input.mousePosition, Camera.main))
        {
            DataStore.i.exploreV2.profileCardIsOpen.Set(false);
        }
    }

    /// <summary>
    /// Instantiates (if does not already exists) a realm selector modal from the given prefab.
    /// </summary>
    /// <returns>An instance of a realm modal modal.</returns>
    internal RealmSelectorComponentView ConfigureRealmSelectorModal()
    {
        RealmSelectorComponentView realmSelectorModal = null;

        GameObject existingModal = GameObject.Find(REALM_SELECTOR_MODAL_ID);
        if (existingModal != null)
            realmSelectorModal = existingModal.GetComponent<RealmSelectorComponentView>();
        else
        {
            realmSelectorModal = GameObject.Instantiate(realmSelectorModalPrefab);
            realmSelectorModal.name = REALM_SELECTOR_MODAL_ID;
        }

        realmSelectorModal.Hide(true);

        return realmSelectorModal;
    }

    public void ShowRealmSelectorModal() { realmSelectorModal.Show(); }

    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2";

        return exploreV2View;
    }
}