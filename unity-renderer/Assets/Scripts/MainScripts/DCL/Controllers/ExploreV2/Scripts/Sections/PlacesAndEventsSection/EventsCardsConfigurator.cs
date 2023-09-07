using DCL;
using MainScripts.DCL.Controllers.HotScenes;
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
    private static Service<IHotScenesFetcher> hotScenesFetcher;

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
        eventCard.onBackgroundClick?.RemoveAllListeners();
        eventCard.onBackgroundClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
        eventCard.onJumpInClick?.RemoveAllListeners();
        eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
        eventCard.onSecondaryJumpInClick?.RemoveAllListeners();
        eventCard.onSecondaryJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
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
        cardModel.numberOfUsers = eventFromAPI.world ? GetNumberOfUsersInWorld(eventFromAPI.server) : GetNumberOfUsersInCoords(cardModel.coords);
        cardModel.worldAddress = eventFromAPI.world ? eventFromAPI.server : null;

        return cardModel;
    }

    private static int GetNumberOfUsersInCoords(Vector2Int coords)
    {
        var numberOfUsers = 0;

        if (hotScenesFetcher.Ref.ScenesInfo == null)
            return numberOfUsers;

        var sceneFound = false;
        foreach (var hotSceneInfo in hotScenesFetcher.Ref.ScenesInfo.Value)
        {
            foreach (Vector2Int hotSceneParcel in hotSceneInfo.parcels)
            {
                if (hotSceneParcel != coords)
                    continue;

                numberOfUsers = hotSceneInfo.usersTotalCount;
                sceneFound = true;
                break;
            }

            if (sceneFound)
                break;
        }

        return numberOfUsers;
    }

    private static int GetNumberOfUsersInWorld(string worldName)
    {
        var numberOfUsers = 0;

        if (hotScenesFetcher.Ref.WorldsInfo == null)
            return numberOfUsers;

        foreach (var worldInfo in hotScenesFetcher.Ref.WorldsInfo.Value)
        {
            if (worldInfo.worldName == worldName)
            {
                numberOfUsers = worldInfo.users;
                break;
            }
        }

        return numberOfUsers;
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
        string formattedDate = string.Empty;

        DateTime startTimeDT = Convert.ToDateTime(eventFromAPI.start_at).ToLocalTime();
        var startTime12Hour = startTimeDT.ToString("hh:mmtt", CultureInfo.InvariantCulture);
        startTime12Hour = startTime12Hour.Replace("AM", "am").Replace("PM", "pm");

        DateTime endTimeDT = Convert.ToDateTime(eventFromAPI.finish_at).ToLocalTime();
        var endTime12Hour = endTimeDT.ToString("hh:mmtt", CultureInfo.InvariantCulture);
        endTime12Hour = endTime12Hour.Replace("AM", "am").Replace("PM", "pm");

        TimeSpan startUtcOffset = TimeZoneInfo.Local.GetUtcOffset(startTimeDT);

        for (var i = 0; i < eventFromAPI.recurrent_dates.Length; i++)
        {
            DateTime recurrentDateDT = Convert.ToDateTime(eventFromAPI.recurrent_dates[i]).ToLocalTime();

            if (recurrentDateDT < DateTime.Today)
                continue;

            var formattedRecurrentDate = $"{recurrentDateDT.ToString("dddd", cultureInfo)}, {recurrentDateDT.ToString("MMM", cultureInfo)} {recurrentDateDT.Day:00}";
            if (i == eventFromAPI.recurrent_dates.Length - 1 && endTimeDT.DayOfYear > recurrentDateDT.DayOfYear)
            {
                var formattedEndDate = $"{endTimeDT.ToString("dddd", cultureInfo)}, {endTimeDT.ToString("MMM", cultureInfo)} {endTimeDT.Day:00}";

                formattedDate = string.Concat(
                    formattedDate,
                    $"{formattedRecurrentDate} from {startTime12Hour} to {formattedEndDate} at {endTime12Hour} (UTC{(startUtcOffset >= TimeSpan.Zero ? "+" : "-")}{startUtcOffset.Hours})\n");
            }
            else
            {
                formattedDate = string.Concat(
                    formattedDate,
                    $"{formattedRecurrentDate} from {startTime12Hour} to {endTime12Hour} (UTC{(startUtcOffset >= TimeSpan.Zero ? "+" : "-")}{startUtcOffset.Hours})\n");
            }
        }

        return formattedDate;
    }

    internal static string FormatEventOrganized(EventFromAPIModel eventFromAPI) =>
        $"Public, Organized by {eventFromAPI.user_name}";

    internal static string FormatEventPlace(EventFromAPIModel eventFromAPI) =>
        string.IsNullOrEmpty(eventFromAPI.scene_name) ? "Decentraland" : eventFromAPI.scene_name;
}
