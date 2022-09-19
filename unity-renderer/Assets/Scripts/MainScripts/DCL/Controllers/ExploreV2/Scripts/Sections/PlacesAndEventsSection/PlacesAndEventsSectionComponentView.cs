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

    /// <summary>
    /// Open a sub-section.
    /// </summary>
    /// <param name="subSectionIndex">Sub-section index.</param>
    void GoToSubsection(int subSectionIndex);

    /// <summary>
    /// Activates/deactivates the section.
    /// </summary>
    /// <param name="isActive"></param>
    void SetActive(bool isActive);
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

    internal bool isDefaultSubSectionLoadedByFirstTime = false;

    public override void Start() =>
        CreateSubSectionSelectorMappings();

    public IHighlightsSubSectionComponentView currentHighlightsSubSectionComponentView => highlightsSubSection;
    public IPlacesSubSectionComponentView currentPlacesSubSectionComponentView => placesSubSection;
    public IEventsSubSectionComponentView currentEventsSubSectionComponentView => eventsSubSection;

    public void GoToSubsection(int subSectionIndex) =>
        subSectionSelector.GetSection(subSectionIndex)?.SelectToggle(reselectIfAlreadyOn: true);

    public void SetActive(bool isActive) =>
        gameObject.SetActive(isActive);

    public override void RefreshControl()
    {
        highlightsSubSection.RefreshControl();
        placesSubSection.RefreshControl();
        eventsSubSection.RefreshControl();
    }

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
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)
                          ?.onSelect.AddListener((isOn) =>
                          {
                              highlightsSubSection.gameObject.SetActive(isOn);

                              isDefaultSubSectionLoadedByFirstTime = true;
                          });

        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)
                          ?.onSelect.AddListener((isOn) =>
                          {
                              placesSubSection.gameObject.SetActive(isOn);
                          });

        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)
                          ?.onSelect.AddListener((isOn) =>
                          {
                              eventsSubSection.gameObject.SetActive(isOn);
                          });

        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.SelectToggle(reselectIfAlreadyOn: true);
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
    }
}