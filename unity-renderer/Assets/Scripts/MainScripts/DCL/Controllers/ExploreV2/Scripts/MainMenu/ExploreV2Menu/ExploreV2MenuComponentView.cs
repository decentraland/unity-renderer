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
    /// Configures the backpack section.
    /// </summary>
    void ConfigureBackpackSection();
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
    [SerializeField] internal BackpackSectionComponentView backpackSection;

    public GameObject go => this != null ? gameObject : null;
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

    public override void RefreshControl() { }

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
    }

    internal void RemoveSectionSelectorMappings()
    {
        sectionSelector.GetSection((int)ExploreSection.Explore)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection((int)ExploreSection.Backpack)?.onSelect.RemoveAllListeners();
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

    public void ConfigureBackpackSection() { backpackSection.ConfigureBackpack(); }
}