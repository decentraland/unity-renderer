using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the events management in ExploreV2.
/// </summary>
public static class ExploreEventsUtils
{
    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string LIVE_TAG_TEXT = "LIVE";
    internal const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";

    /// <summary>
    /// Instantiates (if does not already exists) a event card modal from the given prefab.
    /// </summary>
    /// <param name="eventCardModalPrefab">Prefab to instantiate.</param>
    /// <returns>An instance of a event card modal.</returns>
    public static EventCardComponentView ConfigureEventCardModal(EventCardComponentView eventCardModalPrefab)
    {
        EventCardComponentView eventModal = null;

        GameObject existingModal = GameObject.Find(EVENT_CARD_MODAL_ID);
        if (existingModal != null)
            eventModal = existingModal.GetComponent<EventCardComponentView>();
        else
        {
            eventModal = GameObject.Instantiate(eventCardModalPrefab);
            eventModal.name = EVENT_CARD_MODAL_ID;
        }

        eventModal.Hide(true);

        return eventModal;
    }

    /// <summary>
    /// Creates and configures a pool for event cards.
    /// </summary>
    /// <param name="pool">Pool to configure.</param>
    /// <param name="poolName">Name of the pool.</param>
    /// <param name="eventCardPrefab">Event card prefab to use by the pool.</param>
    /// <param name="maxPrewarmCount">Max number of pre-created cards.</param>
    public static void ConfigureEventCardsPool(out Pool pool, string poolName, EventCardComponentView eventCardPrefab, int maxPrewarmCount)
    {
        pool = PoolManager.i.GetPool(poolName);
        if (pool == null)
        {
            pool = PoolManager.i.AddPool(
                poolName,
                GameObject.Instantiate(eventCardPrefab).gameObject,
                maxPrewarmCount: maxPrewarmCount,
                isPersistent: true);

            pool.ForcePrewarm();
        }
    }

    /// <summary>
    /// Instantiates and configures a given list of events.
    /// </summary>
    /// <param name="events">List of events data.</param>
    /// <param name="pool">Pool to use.</param>
    /// <param name="OnEventInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnEventJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    /// <param name="OnEventSubscribeEventClicked">Action to inform when the Subscribe button has been clicked.</param>
    /// <param name="OnEventUnsubscribeEventClicked">Action to inform when the Unsubscribe button has been clicked.</param>
    /// <returns>A list of instances of events.</returns>
    public static List<BaseComponentView> InstantiateAndConfigureEventCards(List<EventCardComponentModel> events, Pool pool,
        Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked, Action<string> OnEventUnsubscribeEventClicked)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
            instantiatedPlaces.Add(
                InstantiateConfiguredEventCard(eventInfo, pool, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked)
                );

        return instantiatedPlaces;
    }
    
    public static EventCardComponentView InstantiateConfiguredEventCard(EventCardComponentModel eventInfo, Pool pool, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked, Action<string> OnEventUnsubscribeEventClicked)
    {
        EventCardComponentView eventGO = pool.Get().gameObject.GetComponent<EventCardComponentView>();
        ConfigureEventCard(eventGO, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
        return eventGO;
    }

    /// <summary>
    /// Configure a event card with the given model.
    /// </summary>
    /// <param name="eventCard">Event card to configure.</param>
    /// <param name="eventInfo">Model to apply.</param>
    /// <param name="OnEventInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnEventJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    /// <param name="OnEventSubscribeEventClicked">Action to inform when the Subscribe button has been clicked.</param>
    /// <param name="OnEventUnsubscribeEventClicked">Action to inform when the Unsubscribe button has been clicked.</param>
    public static void ConfigureEventCard(
        EventCardComponentView eventCard,
        EventCardComponentModel eventInfo,
        Action<EventCardComponentModel> OnEventInfoClicked,
        Action<EventFromAPIModel> OnEventJumpInClicked,
        Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        eventCard.Configure(eventInfo);
        eventCard.onInfoClick?.RemoveAllListeners();
        eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onJumpInForNotLiveClick?.RemoveAllListeners();
        eventCard.onJumpInForNotLiveClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSubscribeClick?.RemoveAllListeners();
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.RemoveAllListeners();
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));
    }

    /// <summary>
    /// Returs a event card model from the given API data.
    /// </summary>
    /// <param name="eventFromAPI">Data received from the API.</param>
    /// <returns>An event card model.</returns>
    public static EventCardComponentModel CreateEventCardModelFromAPIEvent(EventFromAPIModel eventFromAPI)
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

    internal static string FormatEventDate(EventFromAPIModel eventFromAPI)
    {
        DateTime eventDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        return eventDateTime.ToString("MMMM d", new CultureInfo("en-US"));
    }

    internal static string FormatEventStartDate(EventFromAPIModel eventFromAPI)
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

    internal static string FormatEventStartDateFromTo(EventFromAPIModel eventFromAPI)
    {
        CultureInfo cultureInfo = new CultureInfo("en-US");
        DateTime eventStartDateTime = Convert.ToDateTime(eventFromAPI.next_start_at).ToUniversalTime();
        DateTime eventEndDateTime = Convert.ToDateTime(eventFromAPI.finish_at).ToUniversalTime();
        string formattedDate = $"From {eventStartDateTime.ToString("dddd", cultureInfo)}, {eventStartDateTime.Day} {eventStartDateTime.ToString("MMM", cultureInfo)}" +
                               $" to {eventEndDateTime.ToString("dddd", cultureInfo)}, {eventEndDateTime.Day} {eventEndDateTime.ToString("MMM", cultureInfo)} UTC";

        return formattedDate;
    }

    internal static string FormatEventOrganized(EventFromAPIModel eventFromAPI) { return $"Public, Organized by {eventFromAPI.user_name}"; }

    internal static string FormatEventPlace(EventFromAPIModel eventFromAPI) { return string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name; }

    /// <summary>
    /// Makes a jump in to the event defined by the given place data from API.
    /// </summary>
    /// <param name="eventFromAPI">Event data from API.</param>
    public static void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        Vector2Int coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new string[] { "", "" } : eventFromAPI.realm.Split('-');
        string serverName = realmFromAPI[0];
        string layerName = realmFromAPI[1];

        if (string.IsNullOrEmpty(serverName))
            Environment.i.world.teleportController.Teleport(coords.x, coords.y);
        else
            Environment.i.world.teleportController.JumpIn(coords.x, coords.y, serverName, layerName);
    }
}