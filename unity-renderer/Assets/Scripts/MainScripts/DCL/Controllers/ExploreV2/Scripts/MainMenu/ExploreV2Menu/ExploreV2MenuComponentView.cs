using ExploreV2Analytics;
using System;
using UnityEngine;
using UnityEngine.UI;

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
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
    internal const int EXPLORE_SECTION_INDEX = 0;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView sectionSelector;
    [SerializeField] internal ProfileCardComponentView profileCard;
    [SerializeField] internal RealmViewerComponentView realmViewer;
    [SerializeField] internal ButtonComponentView closeMenuButton;
    [SerializeField] internal InputAction_Trigger closeAction;

    [Header("Sections")]
    [SerializeField] internal PlacesAndEventsSectionComponentView placesAndEventsSection;

    public GameObject go => this != null ? gameObject : null;
    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IProfileCardComponentView currentProfileCard => profileCard;
    public IPlacesAndEventsSectionComponentView currentPlacesAndEventsSection => placesAndEventsSection;

    public event Action OnInitialized;
    public event Action OnCloseButtonPressed;
    public event Action<ExploreSection> OnSectionOpen;

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
            sectionSelector.GetSection(EXPLORE_SECTION_INDEX)?.SelectToggle(true);
        }
        else
        {
            Hide();
            placesAndEventsSection.gameObject.SetActive(false);
        }
    }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection(EXPLORE_SECTION_INDEX)
                       ?.onSelect.AddListener((isOn) =>
                       {
                           placesAndEventsSection.gameObject.SetActive(isOn);

                           if (isOn)
                               OnSectionOpen?.Invoke(ExploreSection.Explore);
                       });
    }

    internal void RemoveSectionSelectorMappings() { sectionSelector.GetSection(EXPLORE_SECTION_INDEX)?.onSelect.RemoveAllListeners(); }

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