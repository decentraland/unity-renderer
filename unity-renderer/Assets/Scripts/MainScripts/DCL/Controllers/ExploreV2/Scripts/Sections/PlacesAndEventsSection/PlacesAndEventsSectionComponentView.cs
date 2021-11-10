using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// Highlights sub-section component.
    /// </summary>
    IHighlightsSubSectionComponentView currentHighlightsSubSectionComponentView { get; }

    /// <summary>
    /// Places sub-section component.
    /// </summary>
    IPlacesSubSectionComponentView currentPlacesSubSectionComponentView { get; }

    /// <summary>
    /// Events sub-section component.
    /// </summary>
    IEventsSubSectionComponentView currentEventsSubSectionComponentView { get; }
}

public class PlacesAndEventsSectionComponentView : BaseComponentView, IPlacesAndEventsSectionComponentView
{
    internal const int HIGHLIGHTS_SUB_SECTION_INDEX = 0;
    internal const int PLACES_SUB_SECTION_INDEX = 1;
    internal const int EVENTS_SUB_SECTION_INDEX = 2;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sub-Sections")]
    [SerializeField] internal HighlightsSubSectionComponentView highlightsSubSection;
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    public IHighlightsSubSectionComponentView currentHighlightsSubSectionComponentView => highlightsSubSection;
    public IEventsSubSectionComponentView currentEventsSubSectionComponentView => eventsSubSection;
    public IPlacesSubSectionComponentView currentPlacesSubSectionComponentView => placesSubSection;

    public override void Start() { CreateSubSectionSelectorMappings(); }

    public override void RefreshControl() { }

    public override void Dispose()
    {
        base.Dispose();

        RemoveSectionSelectorMappings();
        highlightsSubSection.Dispose();
        placesSubSection.Dispose();
        eventsSubSection.Dispose();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.onSelect.AddListener((isOn) => highlightsSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.AddListener((isOn) => placesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.AddListener((isOn) => eventsSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.SelectToggle(true);
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
    }
}