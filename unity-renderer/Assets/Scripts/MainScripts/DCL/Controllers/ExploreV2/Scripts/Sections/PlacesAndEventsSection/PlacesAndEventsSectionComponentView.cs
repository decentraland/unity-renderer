using UnityEngine;

public class PlacesAndEventsSectionComponentView : BaseComponentView
{
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] internal HighlightsSubSectionComponentView highlightsSubSection;
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal FavoritesSubSectionComponentView favoritesSubSection;
    [SerializeField] internal MyPlacesSubSectionComponentView myPlacesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    public override void PostInitialization()
    {
        if (subSectionSelector.isFullyInitialized)
            CreateSubSectionSelectorMappings();
        else
            subSectionSelector.OnFullyInitialized += CreateSubSectionSelectorMappings;
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