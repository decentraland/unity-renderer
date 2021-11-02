using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
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
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    public IEventsSubSectionComponentView currentEventsSubSectionComponentView => eventsSubSection;
    public IPlacesSubSectionComponentView currentPlacesSubSectionComponentView => placesSubSection;

    public override void Start() { CreateSubSectionSelectorMappings(); }

    public override void RefreshControl() { }

    public override void Dispose()
    {
        base.Dispose();

        RemoveSectionSelectorMappings();
        placesSubSection.Dispose();
        eventsSubSection.Dispose();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => placesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => eventsSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(1)?.SelectToggle();
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
    }
}