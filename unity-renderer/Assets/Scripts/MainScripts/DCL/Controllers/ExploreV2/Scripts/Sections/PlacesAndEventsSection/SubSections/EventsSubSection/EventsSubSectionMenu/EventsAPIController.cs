using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEventsAPIController
{
    WebRequestAsyncOperation GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);
    WebRequestAsyncOperation GetEventById(string id, Action<EventFromAPIModel> OnSuccess, Action<string> OnFail);
}

public class EventsAPIController : IEventsAPIController
{
    private const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    private const string URL_GET_EVENT_BY_ID = "https://events.decentraland.org/api/events/{0}";

    public WebRequestAsyncOperation GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail)
    {
        return DCL.Environment.i.platform.webRequest.Get(
            URL_GET_ALL_EVENTS,
            OnSuccess: (webRequestResult) =>
            {
                EventListFromAPIModel upcomingEventsResult = JsonUtility.FromJson<EventListFromAPIModel>(webRequestResult.downloadHandler.text);
                OnSuccess?.Invoke(upcomingEventsResult.data);
            },
            OnFail: (webRequestResult) =>
            {
                OnFail?.Invoke(webRequestResult.error);
            });
    }

    public WebRequestAsyncOperation GetEventById(string id, Action<EventFromAPIModel> OnSuccess, Action<string> OnFail)
    {
        return DCL.Environment.i.platform.webRequest.Get(
            string.Format(URL_GET_EVENT_BY_ID, id),
            OnSuccess: (webRequestResult) =>
            {
                EventDetailFromAPIModel eventResult = JsonUtility.FromJson<EventDetailFromAPIModel>(webRequestResult.downloadHandler.text);
                OnSuccess?.Invoke(eventResult.data);
            },
            OnFail: (webRequestResult) =>
            {
                OnFail?.Invoke(webRequestResult.error);
            });
    }
}