using System;
using System.Collections.Generic;

public interface IEventsSubSectionComponentView: IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Number of events per row that fit with the current upcoming events grid configuration.
    /// </summary>
    int currentUpcomingEventsPerRow { get; }

    /// <summary>
    /// Number of going to events per row that fit with the current upcoming events grid configuration.
    /// </summary>
    int currentGoingEventsPerRow { get; }

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
    event Action OnShowMoreUpcomingEventsClicked;

    /// <summary>
    /// It will be triggered when the "Show More" for going events button is clicked.
    /// </summary>
    event Action OnShowMoreGoingEventsClicked;

    event Action OnConnectWallet;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnEventsSubSectionEnable;

    /// <summary>
    /// Set the featured events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetFeaturedEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetTrendingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the upcoming events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetUpcomingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Add a list of events in the events component.
    /// </summary>
    /// <param name="places">List of events (model) to be added.</param>
    void AddUpcomingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Add a list of events in the going events component.
    /// </summary>
    /// <param name="places">List of events (model) to be added.</param>
    void AddGoingEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Activates/Deactivates the "Show More" button.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreUpcomingEventsButtonActive(bool isActive);

    /// <summary>
    /// Activates/Deactivates the "Show More" button for going events section.
    /// </summary>
    /// <param name="isActive">True for activating it.</param>
    void SetShowMoreGoingEventsButtonActive(bool isActive);

    /// <summary>
    /// Set the going events component with a list of events.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetGoingEvents(List<EventCardComponentModel> events);

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

    void SetShowMoreButtonActive(bool isActive);
    void SetShowMoreGoingButtonActive(bool isActive);

    void SetIsGuestUser(bool isGuestUser);
}
