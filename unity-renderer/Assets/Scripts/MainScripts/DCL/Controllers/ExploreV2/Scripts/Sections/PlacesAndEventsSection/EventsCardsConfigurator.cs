using System;
using System.Globalization;
using UnityEngine;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the events management in ExploreV2.
/// </summary>
public static class EventsCardsConfigurator
{
    internal const string LIVE_TAG_TEXT = "LIVE";

    /// <summary>
    /// Configure a event card with the given model.
    /// </summary>
    /// <param name="eventCard">Event card to configure.</param>
    /// <param name="eventInfo">Model to apply.</param>
    /// <param name="OnEventInfoClicked">Action to inform when the Info button has been clicked.</param>
    /// <param name="OnEventJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    /// <param name="OnEventSubscribeEventClicked">Action to inform when the Subscribe button has been clicked.</param>
    /// <param name="OnEventUnsubscribeEventClicked">Action to inform when the Unsubscribe button has been clicked.</param>
    public static EventCardComponentView Configure(EventCardComponentView eventCard, EventCardComponentModel eventInfo, Action<EventCardComponentModel> OnEventInfoClicked, Action<EventFromAPIModel> OnEventJumpInClicked, Action<string> OnEventSubscribeEventClicked,
        Action<string> OnEventUnsubscribeEventClicked)
    {
        eventCard.Configure(eventInfo);

        eventCard.onInfoClick?.RemoveAllListeners();
        eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
        eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));

        return eventCard;
    }

    public static EventCardComponentModel ConfigureFromAPIData(EventCardComponentModel cardModel, EventFromAPIModel eventFromAPI)
    {
        cardModel.eventId = eventFromAPI.id;
        cardModel.eventPictureUri = eventFromAPI.image;
        cardModel.isLive = eventFromAPI.live;
        cardModel.liveTagText = LIVE_TAG_TEXT;
        cardModel.eventDateText = FormatEventDate(eventFromAPI);
        cardModel.eventName = eventFromAPI.name;
        cardModel.eventDescription = eventFromAPI.description;
        cardModel.eventStartedIn = FormatEventStartDate(eventFromAPI);
        cardModel.eventStartsInFromTo = FormatEventStartDateFromTo(eventFromAPI);
        cardModel.eventOrganizer = FormatEventOrganized(eventFromAPI);
        cardModel.eventPlace = FormatEventPlace(eventFromAPI);
        cardModel.subscribedUsers = eventFromAPI.total_attendees;
        cardModel.isSubscribed = false;
        cardModel.coords = new Vector2Int(eventFromAPI.coordinates[0], eventFromAPI.coordinates[1]);
        cardModel.eventFromAPIInfo = eventFromAPI;

        return cardModel;
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

    internal static string FormatEventOrganized(EventFromAPIModel eventFromAPI) =>
        $"Public, Organized by {eventFromAPI.user_name}";

    internal static string FormatEventPlace(EventFromAPIModel eventFromAPI) =>
        string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name;
}
