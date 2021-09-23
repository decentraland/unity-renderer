using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// Fill the model and updates the explore section with this data.
    /// </summary>
    /// <param name="model">Data to configure the explore section.</param>
    void Configure(PlacesAndEventsSectionComponentModel model);
}

public class PlacesAndEventsSectionComponentView : BaseComponentView, IPlacesAndEventsSectionComponentView
{
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] internal HighlightsSubSectionComponentView highlightsSubSection;
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal FavoritesSubSectionComponentView favoritesSubSection;
    [SerializeField] internal MyPlacesSubSectionComponentView myPlacesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    [Header("Configuration")]
    [SerializeField] internal PlacesAndEventsSectionComponentModel model;

    public override void PostInitialization()
    {
        Configure(model);

        if (subSectionSelector.isFullyInitialized)
            CreateSubSectionSelectorMappings();
        else
            subSectionSelector.OnFullyInitialized += CreateSubSectionSelectorMappings;
    }

    public void Configure(PlacesAndEventsSectionComponentModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl() { }

    public override void Dispose()
    {
        subSectionSelector.OnFullyInitialized -= CreateSubSectionSelectorMappings;
        RemoveSectionSelectorMappings();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => highlightsSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => placesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(2)?.onSelect.AddListener((isOn) => favoritesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(3)?.onSelect.AddListener((isOn) => myPlacesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(4)?.onSelect.AddListener((isOn) => eventsSubSection.gameObject.SetActive(isOn));

        ShowDefaultSubSection();
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(2)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(3)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(4)?.onSelect.RemoveAllListeners();
    }

    internal void ShowDefaultSubSection() { highlightsSubSection.gameObject.SetActive(true); }
}