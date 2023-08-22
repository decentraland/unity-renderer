using System;
using System.Collections.Generic;

public enum EventsType
{
    Upcoming,
    Featured,
    Trending,
    WantToGo,
}

public interface IEventsSubSectionComponentView: IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Number of events per row that fit with the current upcoming events grid configuration.
    /// </summary>
    int currentEventsPerRow { get; }

    /// <summary>
    /// Current event type filter (FEATURED, TRENDING or WANT TO GO) selected.
    /// </summary>
    EventsType SelectedEventType { get; }

    /// <summary>
    /// Current frequency filter (ALL, ONE TIME EVENT or RECURRING EVENT) selected.
    /// </summary>
    string SelectedFrequency { get; }

    /// <summary>
    /// Current category filter (ALL, ART & CULTURE, EDUCATION, etc.) selected.
    /// </summary>
    string SelectedCategory { get; }

    /// <summary>
    /// Current time (low value) filter (00:00 - 24:00)
    /// </summary>
    public float SelectedLowTime { get; }

    /// <summary>
    /// Current time (high value) filter (00:00 - 24:00)
    /// </summary>
    public float SelectedHighTime { get; }

    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the info button is clicked.
    /// </summary>
    event Action<EventCardComponentModel> OnInfoClicked;

    /// <summary>
    /// It will be triggered when the JumpIn button is clicked.
    /// </summary>
    event Action<EventFromAPIModel> OnJumpInClicked;

    /// <summary>
    /// It will be triggered when the subscribe event button is clicked.
    /// </summary>
    event Action<string> OnSubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    event Action<string> OnUnsubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the "Show More" button is clicked.
    /// </summary>
    event Action OnShowMoreEventsClicked;

    event Action OnConnectWallet;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnEventsSubSectionEnable;

    /// <summary>
    /// It will be triggered each time any event type filter is changed.
    /// </summary>
    event Action OnEventTypeFiltersChanged;

    /// <summary>
    /// It will be triggered each time the frequency filter is changed.
    /// </summary>
    event Action OnEventFrequencyFilterChanged;

    /// <summary>
    /// It will be triggered each time the category filter is changed.
    /// </summary>
    event Action OnEventCategoryFilterChanged;

    /// <summary>
    /// It will be triggered each time the time filter is changed.
    /// </summary>
    event Action OnEventTimeFilterChanged;

    /// <summary>
    /// Set the featured events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetFeaturedEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the upcoming events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Add a list of events in the events component.
    /// </summary>
    /// <param name="places">List of events (model) to be added.</param>
    void AddEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreEventsButtonActive(bool isActive);

    /// <summary>
    /// Shows the Event Card modal with the provided information.
    /// </summary>
    /// <param name="eventInfo">Event (model) to be loaded in the card.</param>
    void ShowEventModal(EventCardComponentModel eventInfo);

    /// <summary>
    /// Hides the Event Card modal.
    /// </summary>
    void HideEventModal();

    /// <summary>
    /// Configure the needed pools for the events instantiation.
    /// </summary>
    void ConfigurePools();

    /// <summary>
    /// Show loading bar for all events groups
    /// </summary>
    void SetAllEventGroupsAsLoading();

    void SetIsGuestUser(bool isGuestUser);

    void SetCategories(List<ToggleComponentModel> categories);
}
