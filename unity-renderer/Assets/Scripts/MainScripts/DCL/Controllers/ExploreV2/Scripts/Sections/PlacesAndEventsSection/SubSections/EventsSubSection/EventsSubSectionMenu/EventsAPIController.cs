using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Networking;

public interface IEventsAPIController
{
    /// <summary>
    /// Request all events from the server.
    /// </summary>
    /// <param name="OnSuccess">It will be triggered if the operation finishes successfully.</param>
    /// <param name="OnFail">It will be triggered if the operation fails.</param>
    /// <returns></returns>
    UniTask GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);

    UniTask RegisterParticipation(string eventId);

    UniTask RemoveParticipation(string eventId);
}

[ExcludeFromCodeCoverage]
public class EventsAPIController : IEventsAPIController
{
    internal const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    private const string URL_PARTICIPATE_EVENT = "https://events.decenraland.org/api/events/{event_id}/attendees";

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    public async UniTask GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail)
    {
        UnityWebRequest result = await DCL.Environment.i.platform.webRequest.GetAsync(URL_GET_ALL_EVENTS, isSigned: true);
        EventListFromAPIModel eventListFromAPIModel = Utils.SafeFromJson<EventListFromAPIModel>(result.downloadHandler.text);
        OnSuccess?.Invoke(eventListFromAPIModel.data);
    }

    public async UniTask RegisterParticipation(string eventId)
    {
        await DCL.Environment.i.platform.webRequest.PostAsync(URL_PARTICIPATE_EVENT, "{}", isSigned: true);
    }

    public async UniTask RemoveParticipation(string eventId)
    {
        //await DCL.Environment.i.platform.webRequest.DeleteAsync(URL_PARTICIPATE_EVENT, "{}", isSigned: true);
    }
}
