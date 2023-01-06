// unset:none
using System;

public interface IHighlightsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// It will be triggered when the sub-section want to request to go to the Events sub-section.
    /// </summary>
    event Action OnGoToEventsSubSection;

    /// <summary>
    /// Request all places and events from the API.
    /// </summary>
    void RequestAllPlacesAndEvents();

    /// <summary>
    /// Load the trending places and events with the last requested ones.
    /// </summary>
    void LoadTrendingPlacesAndEvents();

    /// <summary>
    /// Load the featured places with the last requested ones.
    /// </summary>
    void LoadFeaturedPlaces();

    /// <summary>
    /// Load the live events with the last requested ones.
    /// </summary>
    void LoadLiveEvents();
}
