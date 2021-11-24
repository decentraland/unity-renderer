using System;
using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// It will be triggered when any action is executed inside the places and events section.
    /// </summary>
    event Action OnAnyActionExecuted;

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
    internal const int PLACES_SUB_SECTION_INDEX = 0;
    internal const int EVENTS_SUB_SECTION_INDEX = 1;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sections")]
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;

    internal bool isDefaultSubSectionLoadedByFirstTime = false;

    public IEventsSubSectionComponentView currentEventsSubSectionComponentView => eventsSubSection;
    public IPlacesSubSectionComponentView currentPlacesSubSectionComponentView => placesSubSection;

    public event Action OnAnyActionExecuted;

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
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)
                          ?.onSelect.AddListener((isOn) =>
                          {
                              placesSubSection.gameObject.SetActive(isOn);

                              if (isDefaultSubSectionLoadedByFirstTime)
                                  OnAnyActionExecuted?.Invoke();
                          });

        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)
                          ?.onSelect.AddListener((isOn) =>
                          {
                              eventsSubSection.gameObject.SetActive(isOn);

                              if (isDefaultSubSectionLoadedByFirstTime)
                                  OnAnyActionExecuted?.Invoke();

                              isDefaultSubSectionLoadedByFirstTime = true;
                          });

        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.SelectToggle(true);
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
    }
}