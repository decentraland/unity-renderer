using DCL;
using DCL.Interface;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class ExploreEventsHelpers
{
    internal const string EVENT_CARD_MODAL_ID = "EventCard_Modal";
    internal const string LIVE_TAG_TEXT = "LIVE";
    internal const string EVENT_DETAIL_URL = "https://events.decentraland.org/event/?id={0}";

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
        }
    }

    public static List<BaseComponentView> InstantiateAndConfigureEventCards(
        List<EventCardComponentModel> events,
        Pool pool,
        Action<EventCardComponentModel> OnEventInfoClicked,
        Action<EventFromAPIModel> OnEventJumpInClicked,
        Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        List<BaseComponentView> instantiatedPlaces = new List<BaseComponentView>();

        foreach (EventCardComponentModel eventInfo in events)
        {
            EventCardComponentView eventGO = pool.Get().gameObject.GetComponent<EventCardComponentView>();
            ConfigureEventCard(eventGO, eventInfo, OnEventInfoClicked, OnEventJumpInClicked, OnEventSubscribeEventClicked, OnEventUnsubscribeEventClicked);
            instantiatedPlaces.Add(eventGO);
        }

        return instantiatedPlaces;
    }

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
        eventCard.onSubscribeClick?.RemoveAllListeners();
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.RemoveAllListeners();
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));
    }

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

    public static void JumpInToEvent(EventFromAPIModel eventFromAPI)
    {
        Vector2Int coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        string[] realmFromAPI = string.IsNullOrEmpty(eventFromAPI.realm) ? new string[] { "", "" } : eventFromAPI.realm.Split('-');
        string serverName = realmFromAPI[0];
        string layerName = realmFromAPI[1];

        if (string.IsNullOrEmpty(serverName))
            WebInterface.GoTo(coords.x, coords.y);
        else
            WebInterface.JumpIn(coords.x, coords.y, serverName, layerName);
    }
}