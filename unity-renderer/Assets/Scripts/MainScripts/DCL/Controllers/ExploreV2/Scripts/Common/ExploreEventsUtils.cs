using UnityEngine;
using Environment = DCL.Environment;

/// <summary>
/// Utils related to the events management in ExploreV2.
/// </summary>
public static class ExploreEventsUtils
{
    // /// <summary>
    // /// Configure a event card with the given model.
    // /// </summary>
    // /// <param name="eventCard">Event card to configure.</param>
    // /// <param name="eventInfo">Model to apply.</param>
    // /// <param name="OnEventInfoClicked">Action to inform when the Info button has been clicked.</param>
    // /// <param name="OnEventJumpInClicked">Action to inform when the JumpIn button has been clicked.</param>
    // /// <param name="OnEventSubscribeEventClicked">Action to inform when the Subscribe button has been clicked.</param>
    // /// <param name="OnEventUnsubscribeEventClicked">Action to inform when the Unsubscribe button has been clicked.</param>
    // public static void ConfigureEventCard(
    //     EventCardComponentView eventCard,
    //     EventCardComponentModel eventInfo,
    //     Action<EventCardComponentModel> OnEventInfoClicked,
    //     Action<EventFromAPIModel> OnEventJumpInClicked,
    //     Action<string> OnEventSubscribeEventClicked,
    //     Action<string> OnEventUnsubscribeEventClicked)
    // {
    //     eventCard.Configure(eventInfo);
    //
    //     eventCard.onInfoClick?.RemoveAllListeners();
    //     eventCard.onInfoClick?.AddListener(() => OnEventInfoClicked?.Invoke(eventInfo));
    //     eventCard.onJumpInClick?.RemoveAllListeners();
    //     eventCard.onJumpInClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
    //     eventCard.onJumpInForNotLiveClick?.RemoveAllListeners();
    //     eventCard.onJumpInForNotLiveClick?.AddListener(() => OnEventJumpInClicked?.Invoke(eventInfo.eventFromAPIInfo));
    //     eventCard.onSubscribeClick?.RemoveAllListeners();
    //     eventCard.onSubscribeClick?.AddListener(() => OnEventSubscribeEventClicked?.Invoke(eventInfo.eventId));
    //     eventCard.onUnsubscribeClick?.RemoveAllListeners();
    //     eventCard.onUnsubscribeClick?.AddListener(() => OnEventUnsubscribeEventClicked?.Invoke(eventInfo.eventId));
    // }

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
