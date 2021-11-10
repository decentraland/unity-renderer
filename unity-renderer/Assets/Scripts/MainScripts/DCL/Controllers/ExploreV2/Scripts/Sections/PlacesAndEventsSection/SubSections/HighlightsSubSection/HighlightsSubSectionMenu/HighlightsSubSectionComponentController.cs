using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static HotScenesController;

public interface IHighlightsSubSectionComponentController : IDisposable
{
    /// <summary>
    /// It will be triggered when the sub-section want to request to close the ExploreV2 main menu.
    /// </summary>
    event Action OnCloseExploreV2;

    /// <summary>
    /// Request all places and events from the API.
    /// </summary>
    void RequestAllPlacesAndEvents();

    /// <summary>
    /// Load the promoted places with the last requested ones.
    /// </summary>
    void LoadPromotedPlaces();

    /// <summary>
    /// Load the promoted places with the last requested ones.
    /// </summary>
    void LoadFeaturedPlaces();

    /// <summary>
    /// Load the live events with the last requested ones.
    /// </summary>
    void LoadLiveEvents();
}

public class HighlightsSubSectionComponentController : IHighlightsSubSectionComponentController
{
    public event Action OnCloseExploreV2;
    internal event Action OnPlacesFromAPIUpdated;
    internal event Action OnEventsFromAPIUpdated;

    internal const string NO_PLACE_DESCRIPTION_WRITTEN = "The author hasn't written a description yet.";
    internal const string LIVE_TAG_TEXT = "LIVE";
    internal const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";
    internal IHighlightsSubSectionComponentView view;
    internal IPlacesAPIController placesAPIApiController;
    internal IEventsAPIController eventsAPIApiController;
    internal FriendTrackerController friendsTrackerController;
    internal List<HotSceneInfo> placesFromAPI = new List<HotSceneInfo>();
    internal List<EventFromAPIModel> eventsFromAPI = new List<EventFromAPIModel>();
    internal bool reloadHighlights = false;

    public HighlightsSubSectionComponentController(
        IHighlightsSubSectionComponentView view,
        IPlacesAPIController placesAPI,
        IEventsAPIController eventsAPI,
        IFriendsController friendsController)
    {
        this.view = view;
        this.view.OnReady += FirstLoading;
        this.view.OnPlaceInfoClicked += ShowPlaceDetailedInfo;
        this.view.OnEventInfoClicked += ShowEventDetailedInfo;
        this.view.OnPlaceJumpInClicked += JumpInToPlace;
        this.view.OnEventJumpInClicked += JumpInToEvent;
        this.view.OnEventSubscribeEventClicked += SubscribeToEvent;
        this.view.OnEventUnsubscribeEventClicked += UnsubscribeToEvent;
        this.view.OnFriendHandlerAdded += View_OnFriendHandlerAdded;

        placesAPIApiController = placesAPI;
        eventsAPIApiController = eventsAPI;
        OnPlacesFromAPIUpdated += OnRequestedPlacesUpdated;
        OnEventsFromAPIUpdated += OnRequestedEventsUpdated;

        friendsTrackerController = new FriendTrackerController(friendsController, view.currentFriendColors);
    }

    internal void FirstLoading()
    {
        reloadHighlights = true;
        RequestAllPlacesAndEvents();

        view.OnHighlightsSubSectionEnable += RequestAllPlacesAndEvents;
        DataStore.i.exploreV2.isOpen.OnChange += OnExploreV2Open;
    }

    internal void OnExploreV2Open(bool current, bool previous)
    {
        if (current)
            return;

        reloadHighlights = true;
    }

    public void RequestAllPlacesAndEvents()
    {
        if (!reloadHighlights)
            return;

        view.RestartScrollViewPosition();
        view.SetPromotedPlacessAsLoading(true);
        view.SetFeaturedPlacesAsLoading(true);
        view.SetLiveAsLoading(true);
        RequestAllPlacesFromAPI();
        RequestAllEventsFromAPI();
        reloadHighlights = false;
    }

    internal void RequestAllPlacesFromAPI()
    {
        placesAPIApiController.GetAllPlaces(
            (placeList) =>
            {
                placesFromAPI = placeList;
                OnPlacesFromAPIUpdated?.Invoke();
            });
    }

    internal void RequestAllEventsFromAPI()
    {
        eventsAPIApiController.GetAllEvents(
            (eventList) =>
            {
                eventsFromAPI = eventList;
                OnEventsFromAPIUpdated?.Invoke();
            },
            (error) =>
            {
                Debug.LogError($"Error receiving events from the API: {error}");
            });
    }

    internal void OnRequestedPlacesUpdated()
    {
        friendsTrackerController.RemoveAllHandlers();

        LoadPromotedPlaces();
        LoadFeaturedPlaces();
    }

    internal void OnRequestedEventsUpdated() { LoadLiveEvents(); }

    public void LoadPromotedPlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI.Take(10).ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetPromotedPlaces(places);
        view.SetPromotedPlacessAsLoading(false);
    }

    public void LoadFeaturedPlaces()
    {
        List<PlaceCardComponentModel> places = new List<PlaceCardComponentModel>();
        List<HotSceneInfo> placesFiltered = placesFromAPI.Take(6).ToList();
        foreach (HotSceneInfo receivedPlace in placesFiltered)
        {
            PlaceCardComponentModel placeCardModel = CreatePlaceCardModelFromAPIPlace(receivedPlace);
            places.Add(placeCardModel);
        }

        view.SetFeaturedPlaces(places);
        view.SetFeaturedPlacesAsLoading(false);
    }

    public void LoadLiveEvents()
    {
        List<EventCardComponentModel> events = new List<EventCardComponentModel>();
        List<EventFromAPIModel> eventsFiltered = eventsFromAPI.Take(3).ToList();
        foreach (EventFromAPIModel receivedEvent in eventsFiltered)
        {
            EventCardComponentModel eventCardModel = CreateEventCardModelFromAPIEvent(receivedEvent);
            events.Add(eventCardModel);
        }

        view.SetLiveEvents(events);
        view.SetLiveAsLoading(false);
    }

    public void Dispose()
    {
        view.OnReady -= FirstLoading;
        view.OnPlaceInfoClicked -= ShowPlaceDetailedInfo;
        view.OnEventInfoClicked -= ShowEventDetailedInfo;
        view.OnPlaceJumpInClicked -= JumpInToPlace;
        view.OnEventJumpInClicked -= JumpInToEvent;
        view.OnEventSubscribeEventClicked -= SubscribeToEvent;
        view.OnEventUnsubscribeEventClicked -= UnsubscribeToEvent;
        view.OnFriendHandlerAdded -= View_OnFriendHandlerAdded;
    }

    internal PlaceCardComponentModel CreatePlaceCardModelFromAPIPlace(HotSceneInfo placeFromAPI)
    {
        PlaceCardComponentModel placeCardModel = new PlaceCardComponentModel();
        placeCardModel.placePictureUri = placeFromAPI.thumbnail;
        placeCardModel.placeName = placeFromAPI.name;
        placeCardModel.placeDescription = FormatDescription(placeFromAPI);
        placeCardModel.placeAuthor = FormatAuthorName(placeFromAPI);
        placeCardModel.numberOfUsers = placeFromAPI.usersTotalCount;
        placeCardModel.parcels = placeFromAPI.parcels;
        placeCardModel.coords = placeFromAPI.baseCoords;
        placeCardModel.hotSceneInfo = placeFromAPI;

        return placeCardModel;
    }

    internal string FormatDescription(HotSceneInfo placeFromAPI) { return string.IsNullOrEmpty(placeFromAPI.description) ? NO_PLACE_DESCRIPTION_WRITTEN : placeFromAPI.description; }

    internal string FormatAuthorName(HotSceneInfo placeFromAPI) { return $"Author <b>{placeFromAPI.creator}</b>"; }

    internal EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI)
    {
        EventCardComponentModel eventCardModel = new EventCardComponentModel();
        eventCardModel.eventId = eventFromAPI.id;
        eventCardModel.eventPictureUri = eventFromAPI.image;
        eventCardModel.isLive = eventFromAPI.live;
        eventCardModel.liveTagText = LIVE_TAG_TEXT;
        eventCardModel.eventDateText = FormatEventDate(eventFromAPI);
        eventCardModel.eventName = eventFromAPI.name;
        eventCardModel.eventDescription = eventFromAPI.description;
        eventCardModel.eventStartedIn = FormatEventStartDate(eventFromAPI);
        eventCardModel.eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI);
        eventCardModel.eventOrganizer = FormatEventOrganized(eventFromAPI);
        eventCardModel.eventPlace = FormatEventPlace(eventFromAPI);
        eventCardModel.subscribedUsers = eventFromAPI.total_attendees;
        eventCardModel.isSubscribed = false;
        eventCardModel.coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        eventCardModel.eventFromAPIInfo = eventFromAPI;

        // Card events
        return eventCardModel;
    }

    internal string FormatEventDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        return eventDateTime.ToString("MMMM d", new CultureInfo("en-US"));
    }

    internal string FormatEventStartDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        string formattedDate;
        if (eventFromAPI.live)
        {
            int daysAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalDays);
            int hoursAgo = (int)Math.Ceiling((DateTime.Now - eventDateTime).TotalHours);

            if (daysAgo > 0)
                formattedDate = $"{daysAgo} days ago";
            else
                formattedDate = $"{hoursAgo} hr ago";
        }
        else
        {
            int daysToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalDays);
            int hoursToStart = (int)Math.Ceiling((eventDateTime - DateTime.Now).TotalHours);

            if (daysToStart > 0)
                formattedDate = $"in {daysToStart} days";
            else
                formattedDate = $"in {hoursToStart} hours";
        }

        return formattedDate;
    }

    internal string FormatEventStartDateFromTo(EventFromAPIModel eventFromAPI)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        DateTime eventStartDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        DateTime eventEndDateTime = Convert.ToDateTime(eventFromAPI.finish_at).ToUniversalTime();
        string formattedDate = $"From {eventStartDateTime.ToString("dddd", cultureInfo)}, {eventStartDateTime.Day} {eventStartDateTime.ToString("MMM", cultureInfo)}" +
                               $" to {eventEndDateTime.ToString("dddd", cultureInfo)}, {eventEndDateTime.Day} {eventEndDateTime.ToString("MMM", cultureInfo)} UTC";

        return formattedDate;
    }

    internal string FormatEventOrganized(EventFromAPIModel eventFromAPI) { return $"Public, Organized by {eventFromAPI.user_name}"; }

    internal string FormatEventPlace(EventFromAPIModel eventFromAPI) { return string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name; }

    internal void ShowPlaceDetailedInfo(PlaceCardComponentModel placeModel) { view.ShowPlaceModal(placeModel); }

    internal void JumpInToPlace(HotSceneInfo placeFromAPI)
    {
        HotScenesController.HotSceneInfo.Realm realm = new HotScenesController.HotSceneInfo.Realm() { layer = null, serverName = null };
        placeFromAPI.realms = placeFromAPI.realms.OrderByDescending(x => x.usersCount).ToArray();

        for (int i = 0; i < placeFromAPI.realms.Length; i++)
        {
            bool isArchipelagoRealm = string.IsNullOrEmpty(placeFromAPI.realms[i].layer);

            if (isArchipelagoRealm || placeFromAPI.realms[i].usersCount < placeFromAPI.realms[i].maxUsers)
            {
                realm = placeFromAPI.realms[i];
                break;
            }
        }

        if (string.IsNullOrEmpty(realm.serverName))
            WebInterface.GoTo(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y);
        else
            WebInterface.JumpIn(placeFromAPI.baseCoords.x, placeFromAPI.baseCoords.y, realm.serverName, realm.layer);

        view.HidePlaceModal();
        OnCloseExploreV2?.Invoke();
    }

    internal void View_OnFriendHandlerAdded(FriendsHandler friendsHandler) { friendsTrackerController.AddHandler(friendsHandler); }

    internal void ShowEventDetailedInfo(EventCardComponentModel eventModel) { view.ShowEventModal(eventModel); }

    internal void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        Vector2Int coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new string[] { "", "" } : eventFromAPI.realm.Split('-');
        string serverName = realmFromAPI[0];
        string layerName = realmFromAPI[1];

        if (string.IsNullOrEmpty(serverName))
            WebInterface.GoTo(coords.x, coords.y);
        else
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);

        view.HideEventModal();
        OnCloseExploreV2?.Invoke();
    }

    internal void SubscribeToEvent(string eventId)
    {
        // TODO (Santi): Remove when the RegisterAttendEvent POST is available.
        WebInterface.OpenURL(string.Format(EVENT_DETAIL_URL, eventId));

        // TODO (Santi): Waiting for the new version of the Events API where we will be able to send a signed POST to register our user in an event.
        //eventsAPIApiController.RegisterAttendEvent(
        //    eventId,
        //    true,
        //    () =>
        //    {
        //        // ...
        //    },
        //    (error) =>
        //    {
        //        Debug.LogError($"Error posting 'attend' message to the API: {error}");
        //    });
    }

    internal void UnsubscribeToEvent(string eventId)
    {
        // TODO (Santi): Remove when the RegisterAttendEvent POST is available.
        WebInterface.OpenURL(string.Format(EVENT_DETAIL_URL, eventId));

        // TODO (Santi): Waiting for the new version of the Events API where we will be able to send a signed POST to unregister our user in an event.
        //eventsAPIApiController.RegisterAttendEvent(
        //    eventId,
        //    false,
        //    () =>
        //    {
        //        // ...
        //    },
        //    (error) =>
        //    {
        //        Debug.LogError($"Error posting 'attend' message to the API: {error}");
        //    });
    }
}