using DCL;
using System;
using UnityEngine;

public interface IExploreV2MenuComponentView : IDisposable
{
    /// <summary>
    /// It will be triggered when the view is fully initialized.
    /// </summary>
    event Action OnInitialized;

    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action OnCloseButtonPressed;

    /// <summary>
    /// It will be triggered when a section is open.
    /// </summary>
    event Action<ExploreSection> OnSectionOpen;

    /// <summary>
    /// Real viewer component.
    /// </summary>
    IRealmViewerComponentView currentRealmViewer { get; }

    /// <summary>
    /// Profile card component.
    /// </summary>
    IProfileCardComponentView currentProfileCard { get; }

    /// <summary>
    /// Places and Events section component.
    /// </summary>
    IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection { get; }

    /// <summary>
    /// Shows/Hides the game object of the explore menu.
    /// </summary>
    /// <param name="isActive">True to show it.</param>
    void SetVisible(bool isActive);

    /// <summary>
    /// Open a section.
    /// </summary>
    /// <param name="section">Section to go.</param>
    void GoToSection(ExploreSection section);

    /// <summary>
    /// Configures a encapsulated section.
    /// </summary>
    /// <param name="section">Section to configure.</param>
    /// <param name="featureConfiguratorFlag">Flag used to configurates the feature.</param>
    /// <param name="isActive">Indicates if the section is active or not.</param>
    void ConfigureEncapsulatedSection(ExploreSection section, BaseVariable<Transform> featureConfiguratorFlag, bool isActive);
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
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

    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IProfileCardComponentView currentProfileCard => profileCard;
    public IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection => placesAndEventsSection;

    public event Action OnInitialized;
    public event Action OnCloseButtonPressed;
    public event Action<ExploreSection> OnSectionOpen;

    internal ExploreSection currentSectionIndex = ExploreSection.Explore;

    public override void Start()
    {
        CreateSectionSelectorMappings();
        ConfigureCloseButton();

        OnInitialized?.Invoke();
    }

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
    }

    public void SetVisible(bool isActive)
    {
        if (isActive)
        {
            Show();
            GoToSection(currentSectionIndex);
        }
        else
        {
            Hide();
            placesAndEventsSection.gameObject.SetActive(false);
        }
    }

    public void GoToSection(ExploreSection section)
    {
        currentSectionIndex = section;
        sectionSelector.GetSection((int)section)?.SelectToggle(true);
    }

    public void ConfigureEncapsulatedSection(ExploreSection section, BaseVariable<Transform> featureConfiguratorFlag, bool isActive)
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

        sectionSelector.GetSection((int)section).SetActive(isActive);

        if (isActive)
            sectionView?.EncapsulateFeature(featureConfiguratorFlag);
    }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection((int)ExploreSection.Explore)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           placesAndEventsSection.gameObject.SetActive(isOn);

                           if (isOn)
                           {
                               currentSectionIndex = ExploreSection.Explore;
                               OnSectionOpen?.Invoke(currentSectionIndex);
                           }
                       });

        sectionSelector.GetSection((int)ExploreSection.Backpack)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           if (isOn)
                           {
                               backpackSection.Show();
                               currentSectionIndex = ExploreSection.Backpack;
                               OnSectionOpen?.Invoke(currentSectionIndex);
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
                               currentSectionIndex = ExploreSection.Map;
                               OnSectionOpen?.Invoke(currentSectionIndex);
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
                               currentSectionIndex = ExploreSection.Builder;
                               OnSectionOpen?.Invoke(currentSectionIndex);
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
                               currentSectionIndex = ExploreSection.Quest;
                               OnSectionOpen?.Invoke(currentSectionIndex);
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
                               currentSectionIndex = ExploreSection.Settings;
                               OnSectionOpen?.Invoke(currentSectionIndex);
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
    }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(CloseMenu);
        closeAction.OnTriggered += OnCloseActionTriggered;
    }

    internal void CloseMenu() { OnCloseButtonPressed?.Invoke(); }

    internal void OnCloseActionTriggered(DCLAction_Trigger action) { CloseMenu(); }

    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2";

        return exploreV2View;
    }
}