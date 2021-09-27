using System;
using UnityEngine;

public interface IExploreV2MenuComponentView
{
    /// <summary>
    /// It will be triggered when the close button is clicked.
    /// </summary>
    event Action OnCloseButtonPressed;

    /// <summary>
    /// Game object of the explore menu.
    /// </summary>
    GameObject go { get; }

    /// <summary>
    /// Real viewer component.
    /// </summary>
    IRealmViewerComponentView currentRealmViewer { get; }

    /// <summary>
    /// Profile card component.
    /// </summary>
    IProfileCardComponentView currentProfileCard { get; }

    /// <summary>
    /// Returns true if the game object is activated.
    /// </summary>
    bool isActive { get; }

    /// <summary>
    /// Activates/Deactivates the game object of the explore menu.
    /// </summary>
    /// <param name="isActive">True to activate it.</param>
    void SetActive(bool isActive);
}

public class ExploreV2MenuComponentView : BaseComponentView, IExploreV2MenuComponentView
{
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView sectionSelector;
    [SerializeField] internal ProfileCardComponentView profileCard;
    [SerializeField] internal RealmViewerComponentView realmViewer;
    [SerializeField] internal ButtonComponentView closeMenuButton;

    [Header("Sections")]
    [SerializeField] internal PlacesAndEventsSectionComponentView placesAndEventsSection;
    [SerializeField] internal QuestSectionComponentView questSection;
    [SerializeField] internal BackpackSectionComponentView backpackSection;
    [SerializeField] internal MapSectionComponentView mapSection;
    [SerializeField] internal BuilderSectionComponentView builderSection;
    [SerializeField] internal MarketSectionComponentView marketSection;
    [SerializeField] internal SettingsSectionComponentView settingsSection;

    public bool isActive => gameObject.activeSelf;

    public GameObject go => this != null ? gameObject : null;
    public IRealmViewerComponentView currentRealmViewer => realmViewer;
    public IProfileCardComponentView currentProfileCard => profileCard;

    public event Action OnCloseButtonPressed;

    public override void PostInitialization()
    {
        if (sectionSelector.isFullyInitialized)
            CreateSectionSelectorMappings();
        else
            sectionSelector.OnFullyInitialized += CreateSectionSelectorMappings;

        if (closeMenuButton.isFullyInitialized)
            ConfigureCloseButton();
        else
            closeMenuButton.OnFullyInitialized += ConfigureCloseButton;
    }

    public override void RefreshControl() { }

    public override void Dispose()
    {
        sectionSelector.OnFullyInitialized -= CreateSectionSelectorMappings;
        RemoveSectionSelectorMappings();
        closeMenuButton.onClick.RemoveAllListeners();
    }

    public void SetActive(bool isActive) { gameObject.SetActive(isActive); }

    internal void CreateSectionSelectorMappings()
    {
        sectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => placesAndEventsSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => questSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(2)?.onSelect.AddListener((isOn) => backpackSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(3)?.onSelect.AddListener((isOn) => mapSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(4)?.onSelect.AddListener((isOn) => builderSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(5)?.onSelect.AddListener((isOn) => marketSection.gameObject.SetActive(isOn));
        sectionSelector.GetSection(6)?.onSelect.AddListener((isOn) => settingsSection.gameObject.SetActive(isOn));

        ShowDefaultSection();
    }

    internal void RemoveSectionSelectorMappings()
    {
        sectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(2)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(3)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(4)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(5)?.onSelect.RemoveAllListeners();
        sectionSelector.GetSection(6)?.onSelect.RemoveAllListeners();
    }

    internal void ConfigureCloseButton()
    {
        closeMenuButton.onClick.AddListener(() =>
        {
            OnCloseButtonPressed?.Invoke();
        });
    }

    internal void ShowDefaultSection() { placesAndEventsSection.gameObject.SetActive(true); }

    internal static ExploreV2MenuComponentView Create()
    {
        ExploreV2MenuComponentView exploreV2View = Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2View.name = "_ExploreV2";

        return exploreV2View;
    }
}