using System;
using System.Collections.Generic;
using UnityEngine;
using MainScripts.DCL.Controllers.HotScenes;

public interface IHighlightsSubSectionComponentView: IPlacesAndEventsSubSectionComponentView
{
    /// <summary>
    /// Colors used for the background of the friends heads.
    /// </summary>
    Color[] currentFriendColors { get; }

    /// <summary>
    /// It will be triggered when all the UI components have been fully initialized.
    /// </summary>
    event Action OnReady;

    /// <summary>
    /// It will be triggered when the place info button is clicked.
    /// </summary>
    event Action<PlaceCardComponentModel> OnPlaceInfoClicked;

    /// <summary>
    /// It will be triggered when the event info button is clicked.
    /// </summary>
    event Action<EventCardComponentModel> OnEventInfoClicked;

    /// <summary>
    /// It will be triggered when the place JumpIn button is clicked.
    /// </summary>
    event Action<IHotScenesController.PlaceInfo> OnPlaceJumpInClicked;

    /// <summary>
    /// It will be triggered when the place favorite button is clicked.
    /// </summary>
    event Action<string, bool> OnFavoriteClicked;

    /// <summary>
    /// It will be triggered when the event JumpIn button is clicked.
    /// </summary>
    event Action<EventFromAPIModel> OnEventJumpInClicked;

    /// <summary>
    /// It will be triggered when the subscribe event button is clicked.
    /// </summary>
    event Action<string> OnEventSubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the unsubscribe event button is clicked.
    /// </summary>
    event Action<string> OnEventUnsubscribeEventClicked;

    /// <summary>
    /// It will be triggered when the view all events button is clicked.
    /// </summary>
    event Action OnViewAllEventsClicked;

    /// <summary>
    /// It will be triggered when a new friend handler is added by a place card.
    /// </summary>
    event Action<FriendsHandler> OnFriendHandlerAdded;

    /// <summary>
    /// It will be triggered each time the view is enabled.
    /// </summary>
    event Action OnHighlightsSubSectionEnable;

    /// <summary>
    /// Set the trending places/events component with a list of places and events.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetTrendingPlacesAndEvents(List<PlaceCardComponentModel> places, List<EventCardComponentModel> events);

    /// <summary>
    /// Set the trending places and events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetTrendingPlacesAndEventsAsLoading(bool isVisible);

    /// <summary>
    /// Set the featured places component with a list of places.
    /// </summary>
    /// <param name="places">List of places (model) to be loaded.</param>
    void SetFeaturedPlaces(List<PlaceCardComponentModel> places);

    /// <summary>
    /// Set the featured places component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetFeaturedPlacesAsLoading(bool isVisible);

    /// <summary>
    /// Set the live events component with a list of places.
    /// </summary>
    /// <param name="events">List of events (model) to be loaded.</param>
    void SetLiveEvents(List<EventCardComponentModel> events);

    /// <summary>
    /// Set the live events component in loading mode.
    /// </summary>
    /// <param name="isVisible">True for activating the loading mode.</param>
    void SetLiveAsLoading(bool isVisible);

    /// <summary>
    /// Shows the Place Card modal with the provided information.
    /// </summary>
    /// <param name="placeInfo">Place (model) to be loaded in the card.</param>
    void ShowPlaceModal(PlaceCardComponentModel placeInfo);

    /// <summary>
    /// Hides the Place Card modal.
    /// </summary>
    void HidePlaceModal();

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
    /// Configure the needed pools for the places and events instantiation.
    /// </summary>
    void ConfigurePools();
}
