using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// Events sub-section component.
    /// </summary>
    IEventsSubSectionComponentView currentEventsSubSectionComponentView { get; }
}

public class PlacesAndEventsSectionComponentView : MonoBehaviour, IPlacesAndEventsSectionComponentView
{
    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    public IEventsSubSectionComponentView currentEventsSubSectionComponentView => eventsSubSection;

    private void Start()
    {
        if (subSectionSelector.isFullyInitialized)
            CreateSubSectionSelectorMappings();
        else
            subSectionSelector.OnFullyInitialized += CreateSubSectionSelectorMappings;
    }

    private void OnDestroy()
    {
        subSectionSelector.OnFullyInitialized -= CreateSubSectionSelectorMappings;
        RemoveSectionSelectorMappings();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.AddListener((isOn) => placesSubSection.gameObject.SetActive(isOn));
        subSectionSelector.GetSection(1)?.onSelect.AddListener((isOn) => eventsSubSection.gameObject.SetActive(isOn));

        ShowDefaultSubSection();
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(0)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(1)?.onSelect.RemoveAllListeners();
    }

    internal void ShowDefaultSubSection() { placesSubSection.gameObject.SetActive(true); }
}