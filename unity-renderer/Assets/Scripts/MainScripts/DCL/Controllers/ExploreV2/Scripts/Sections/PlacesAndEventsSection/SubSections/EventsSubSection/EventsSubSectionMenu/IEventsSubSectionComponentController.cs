// unset:none
using System;

public interface IEventsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all events from the API.
    /// </summary>
    void RequestAllEvents();

    /// <summary>
    /// Load the featured events with the last requested ones.
    /// </summary>
    void LoadFeaturedEvents();

    /// <summary>
    /// Load the trending events with the last requested ones.
    /// </summary>
    void LoadTrendingEvents();

    /// <summary>
    /// Load the upcoming events with the last requested ones.
    /// </summary>
    void LoadUpcomingEvents();

    /// <summary>
    /// Increment the number of upcoming events loaded.
    /// </summary>
    void ShowMoreUpcomingEvents();

    /// <summary>
    /// Load the going events with the last requested ones.
    /// </summary>
    void LoadGoingEvents();
}
