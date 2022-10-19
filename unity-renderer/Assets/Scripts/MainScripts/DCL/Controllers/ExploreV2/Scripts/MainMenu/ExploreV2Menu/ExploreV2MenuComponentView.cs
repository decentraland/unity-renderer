using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

public interface IExploreV2MenuComponentView : IDisposable
{

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
    /// Shows/Hides the game object of the explore menu.
    /// </summary>
    /// <param name="isActive">True to show it.</param>
    void SetVisible(bool isActive);

    /// <summary>
    /// It is called after the show animation has finished.
    /// </summary>
    [UsedImplicitly]
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
    /// <param name="sectionId">Section to configure.</param>
    /// <param name="featureConfiguratorFlag">Flag used to configurates the feature.</param>
    void ConfigureEncapsulatedSection(ExploreSection sectionId, BaseVariable<Transform> featureConfiguratorFlag);

    /// <summary>
    /// Shows the Realm Selector modal.
    /// </summary>
    void ShowRealmSelectorModal();
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
    internal const string REALM_SELECTOR_MODAL_ID = "RealmSelector_Modal";

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

    private DataStore_Camera cameraDataStore;

    private Dictionary<ExploreSection, FeatureEncapsulatorComponentView> exploreSectionsById;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;

    private RectTransform profileCardRectTranform;
    private RealmSelectorComponentView realmSelectorModal;

    public override void Awake()
    {
        base.Awake();

        profileCardRectTranform = profileCard.GetComponent<RectTransform>();
        realmSelectorModal = ConfigureRealmSelectorModal();
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);

        exploreSectionsById = new Dictionary<ExploreSection, FeatureEncapsulatorComponentView>()
        {
            { ExploreSection.Explore, null },
            { ExploreSection.Backpack, backpackSection },
            { ExploreSection.Map, mapSection },
            { ExploreSection.Builder, builderSection },
            { ExploreSection.Quest, questSection },
            { ExploreSection.Settings, settingsSection },
        };
    }

    public override void Start()
    {
        DataStore.i.exploreV2.isInitialized.OnChange += IsInitialized_OnChange;
        IsInitialized_OnChange(DataStore.i.exploreV2.isInitialized.Get(), false);

        ConfigureCloseButton();
    }

    public override void Update() =>
        CheckIfProfileCardShouldBeClosed();

    public void OnDestroy() =>
        hudCanvasCameraModeController?.Dispose();

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
            {
                GoToSection((ExploreSection)DataStore.i.exploreV2.currentSectionIndex.Get());
            }
            else
            {
                List<ISectionToggle> allSections = sectionSelector.GetAllSections();
                foreach (ISectionToggle section in allSections)
                {
                    if (section != null && section.IsActive())
                    {
                        section.SelectToggle(reselectIfAlreadyOn: true);
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

    public void GoToSection(ExploreSection section)
    {
        sectionSelector.GetSection((int)section)?.SelectToggle(reselectIfAlreadyOn: true);

        AudioScriptableObjects.dialogOpen.Play(true);
        AudioScriptableObjects.listItemAppear.ResetPitch();
    }

    public void OnAfterShowAnimationCompleted()
    {
        if (!DataStore.i.exploreV2.isOpen.Get())
            return;

        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);
        OnAfterShowAnimation?.Invoke();
    }

    public void SetSectionActive(ExploreSection section, bool isActive) =>
        sectionSelector.GetSection((int)section).SetActive(isActive);

    public bool IsSectionActive(ExploreSection section) =>
        sectionSelector.GetSection((int)section).IsActive();

    public void ConfigureEncapsulatedSection(ExploreSection sectionId, BaseVariable<Transform> featureConfiguratorFlag) =>
        exploreSectionsById[sectionId]?.EncapsulateFeature(featureConfiguratorFlag);

    public void ShowRealmSelectorModal() =>
        realmSelectorModal.Show();

    public override void RefreshControl()
    {
        placesAndEventsSection.RefreshControl();

        foreach (var section in exploreSectionsById.Keys)
            exploreSectionsById[section]?.RefreshControl();
    }

    internal void RemoveSectionSelectorMappings()
    {
        foreach (int sectionId in Enum.GetValues(typeof(ExploreSection)))
            sectionSelector.GetSection(sectionId)?.onSelect.RemoveAllListeners();
    }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke(false));
        closeAction.OnTriggered += OnCloseActionTriggered;
        DataStore.i.exploreV2.isSomeModalOpen.OnChange += IsSomeModalOpen_OnChange;
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
        {
            realmSelectorModal = existingModal.GetComponent<RealmSelectorComponentView>();
        }
        else
        {
            realmSelectorModal = GameObject.Instantiate(realmSelectorModalPrefab);
            realmSelectorModal.name = REALM_SELECTOR_MODAL_ID;
        }

        realmSelectorModal.Hide(true);

        return realmSelectorModal;
    }

    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2";

        return exploreV2View;
    }

    private void IsInitialized_OnChange(bool current, bool previous)
    {
        if (!current)
            return;

        DataStore.i.exploreV2.isInitialized.OnChange -= IsInitialized_OnChange;
        StartCoroutine(CreateSectionSelectorMappingsAfterDelay());
    }

    private IEnumerator CreateSectionSelectorMappingsAfterDelay()
    {
        yield return null;
        CreateSectionSelectorMappings();
    }

    internal void CreateSectionSelectorMappings()
    {
        foreach (ExploreSection sectionId in Enum.GetValues(typeof(ExploreSection)))
            sectionSelector.GetSection((int)sectionId)
                           ?.onSelect.AddListener(OnSectionSelected(sectionId));
    }

    private UnityAction<bool> OnSectionSelected(ExploreSection sectionId) =>
        isOn =>
        {
            FeatureEncapsulatorComponentView sectionView = exploreSectionsById[sectionId];

            if (isOn)
            {
                // If not an explorer Section, because we do not Show/Hide it
                if (sectionView != null)
                    sectionView.Show();

                OnSectionOpen?.Invoke(sectionId);
            }
            else if (sectionView != null) // If not an explorer Section, because we do not Show/Hide it
            {
                sectionView.Hide();
            }
        };

    private void IsSomeModalOpen_OnChange(bool current, bool previous)
    {
        closeAction.OnTriggered -= OnCloseActionTriggered;

        if (!current)
            closeAction.OnTriggered += OnCloseActionTriggered;
    }

    private void OnCloseActionTriggered(DCLAction_Trigger action) => OnCloseButtonPressed?.Invoke(true);

    private void CheckIfProfileCardShouldBeClosed()
    {
        if (!DataStore.i.exploreV2.profileCardIsOpen.Get())
            return;

        cameraDataStore ??= DataStore.i.camera;

        if (Input.GetMouseButton(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(profileCardRectTranform, Input.mousePosition, cameraDataStore.hudsCamera.Get()) &&
            !RectTransformUtility.RectangleContainsScreenPoint(HUDController.i.profileHud.view.expandedMenu, Input.mousePosition, cameraDataStore.hudsCamera.Get()))
        {
            DataStore.i.exploreV2.profileCardIsOpen.Set(false);
        }
    }
}