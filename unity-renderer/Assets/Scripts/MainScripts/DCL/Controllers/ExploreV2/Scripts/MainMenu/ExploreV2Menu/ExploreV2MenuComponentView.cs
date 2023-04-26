using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField] internal FeatureEncapsulatorComponentView questSection;
    [SerializeField] internal FeatureEncapsulatorComponentView settingsSection;

    [Header("Tutorial References")]
    [SerializeField] internal RectTransform profileCardTooltipReference;

    private DataStore_Camera cameraDataStore;

    private Dictionary<ExploreSection, FeatureEncapsulatorComponentView> exploreSectionsById;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;

    private RectTransform profileCardRectTransform;
    private RealmSelectorComponentView realmSelectorModal;
    private bool isOpeningSectionThisFrame;

    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IRealmSelectorComponentView currentRealmSelectorModal => realmSelectorModal;

    public IProfileCardComponentView currentProfileCard => profileCard;
    public IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection => placesAndEventsSection;

    public RectTransform currentTopMenuTooltipReference => sectionSelector.GetSection((int)ExploreSection.Explore).pivot;
    public RectTransform currentPlacesAndEventsTooltipReference => sectionSelector.GetSection((int)ExploreSection.Explore).pivot;
    public RectTransform currentBackpackTooltipReference => sectionSelector.GetSection((int)ExploreSection.Backpack).pivot;
    public RectTransform currentMapTooltipReference => sectionSelector.GetSection((int)ExploreSection.Map).pivot;
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

    public override void Awake()
    {
        base.Awake();
        showHideAnimator.OnWillFinishStart += OnAfterShowAnimationCompleted;

        profileCardRectTransform = profileCard.GetComponent<RectTransform>();
        realmSelectorModal = ConfigureRealmSelectorModal();
        hudCanvasCameraModeController = new HUDCanvasCameraModeController(GetComponent<Canvas>(), DataStore.i.camera.hudsCamera);

        exploreSectionsById = new Dictionary<ExploreSection, FeatureEncapsulatorComponentView>
        {
            { ExploreSection.Explore, null },
            { ExploreSection.Backpack, backpackSection },
            { ExploreSection.Map, mapSection },
            { ExploreSection.Quest, questSection },
            { ExploreSection.Settings, settingsSection },
        };
    }

    public void Start()
    {
        DataStore.i.exploreV2.isInitialized.OnChange += IsInitialized_OnChange;
        IsInitialized_OnChange(DataStore.i.exploreV2.isInitialized.Get(), false);

        ConfigureCloseButton();
    }

    public void Update() =>
        CheckIfProfileCardShouldBeClosed();

    public void OnDestroy() =>
        hudCanvasCameraModeController?.Dispose();

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
                if (isOpeningSectionThisFrame)
                    return;

                isOpeningSectionThisFrame = true;
                StartCoroutine(ResetSectionOpenLock());

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

    private IEnumerator ResetSectionOpenLock()
    {
        yield return null;
        isOpeningSectionThisFrame = false;
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            DataStore.i.exploreV2.isInShowAnimationTransiton.Set(true);
            Show();

            ISectionToggle sectionToGo = sectionSelector.GetSection(DataStore.i.exploreV2.currentSectionIndex.Get());

            if (sectionToGo != null && sectionToGo.IsActive())
                GoToSection((ExploreSection)DataStore.i.exploreV2.currentSectionIndex.Get());
            else
                GoToFirstActiveSection();
        }
        else
        {
            Hide();
            AudioScriptableObjects.dialogClose.Play(true);
        }
    }

    private void GoToFirstActiveSection()
    {
        foreach (ISectionToggle section in sectionSelector.GetAllSections())
            if (section != null && section.IsActive())
            {
                section.SelectToggle(reselectIfAlreadyOn: true);
                break;
            }
    }

    public void GoToSection(ExploreSection section)
    {
        sectionSelector.GetSection((int)section)?.SelectToggle(reselectIfAlreadyOn: true);

        AudioScriptableObjects.dialogOpen.Play(true);
        AudioScriptableObjects.listItemAppear.ResetPitch();
    }

    public void SetSectionActive(ExploreSection section, bool isActive) =>
        sectionSelector.GetSection((int)section).SetActive(isActive);

    public bool IsSectionActive(ExploreSection section) =>
        sectionSelector.GetSection((int)section).IsActive();

    private void OnAfterShowAnimationCompleted(ShowHideAnimator _)
    {
        if (!DataStore.i.exploreV2.isOpen.Get())
            return;

        DataStore.i.exploreV2.isInShowAnimationTransiton.Set(false);
        OnAfterShowAnimation?.Invoke();
    }

    public void ConfigureEncapsulatedSection(ExploreSection sectionId, BaseVariable<Transform> featureConfiguratorFlag) =>
        exploreSectionsById[sectionId]?.EncapsulateFeature(featureConfiguratorFlag);

    public override void RefreshControl()
    {
        placesAndEventsSection.RefreshControl();

        foreach (ExploreSection section in exploreSectionsById.Keys)
            exploreSectionsById[section]?.RefreshControl();
    }

    internal void RemoveSectionSelectorMappings()
    {
        foreach (int sectionId in Enum.GetValues(typeof(ExploreSection)))
            sectionSelector.GetSection(sectionId)?.onSelect.RemoveAllListeners();
    }

    private void IsSomeModalOpen_OnChange(bool current, bool previous)
    {
        closeAction.OnTriggered -= OnCloseActionTriggered;

        if (!current)
            closeAction.OnTriggered += OnCloseActionTriggered;
    }

    private void CheckIfProfileCardShouldBeClosed()
    {
        if (!DataStore.i.exploreV2.profileCardIsOpen.Get())
            return;

        cameraDataStore ??= DataStore.i.camera;

        if (Input.GetMouseButton(0) &&
            !RectTransformUtility.RectangleContainsScreenPoint(profileCardRectTransform, Input.mousePosition, cameraDataStore.hudsCamera.Get()) &&
            !RectTransformUtility.RectangleContainsScreenPoint(HUDController.i.profileHud.view.ExpandedMenu, Input.mousePosition, cameraDataStore.hudsCamera.Get()))
            DataStore.i.exploreV2.profileCardIsOpen.Set(false);
    }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(() => OnCloseButtonPressed?.Invoke(false));
        closeAction.OnTriggered += OnCloseActionTriggered;
        DataStore.i.exploreV2.isSomeModalOpen.OnChange += IsSomeModalOpen_OnChange;
    }

    private void OnCloseActionTriggered(DCLAction_Trigger action) =>
        OnCloseButtonPressed?.Invoke(true);

    public void ShowRealmSelectorModal() =>
        realmSelectorModal.Show();

    /// <summary>
    /// Instantiates (if does not already exists) a realm selector modal from the given prefab.
    /// </summary>
    /// <returns>An instance of a realm modal modal.</returns>
    internal RealmSelectorComponentView ConfigureRealmSelectorModal()
    {
        RealmSelectorComponentView realmSelectorView;

        var existingModal = GameObject.Find(REALM_SELECTOR_MODAL_ID);

        if (existingModal != null)
            realmSelectorView = existingModal.GetComponent<RealmSelectorComponentView>();
        else
        {
            realmSelectorView = Instantiate(realmSelectorModalPrefab);
            realmSelectorView.name = REALM_SELECTOR_MODAL_ID;
        }

        realmSelectorView.Hide(true);

        return realmSelectorView;
    }

    public void HideMapOnEnteringWorld()
    {
        placesAndEventsSection.Show();
        mapSection.Hide();
    }
}
