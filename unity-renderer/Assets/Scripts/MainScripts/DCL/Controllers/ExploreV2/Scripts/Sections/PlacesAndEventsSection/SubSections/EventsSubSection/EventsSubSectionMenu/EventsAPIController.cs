using DCL;
using DCL.Interface;
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
    WebRequestAsyncOperation GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);
    void GetAllEventsSigned(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);

    /// <summary>
    /// Register/Unregister your user in a specific event.
    /// </summary>
    /// <param name="eventId">Event Id.</param>
    /// <param name="isRegistered">True for registering.</param>
    /// <param name="OnSuccess">It will be triggered if the operation finishes successfully.</param>
    /// <param name="OnFail">It will be triggered if the operation fails.</param>
    /// <returns></returns>
    UnityWebRequestAsyncOperation RegisterAttendEvent(string eventId, bool isRegistered, Action OnSuccess, Action<string> OnFail);
}

[ExcludeFromCodeCoverage]
public class EventsAPIController : IEventsAPIController
{
    internal const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    internal const string URL_POST_MESSAGE = "https://events.decentraland.org/api/message";
    internal const string ATTEND_EVENTS_MESSAGE_TYPE = "attend";

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    internal Action<List<EventFromAPIModel>> OnGetAllEventsSignedSuccess;
    internal Action<string> OnGetAllEventsSignedFail;

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

    public void GetAllEventsSigned(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail)
    {
        ExploreV2Bridge.OnDCLEventsReceived -= OnDCLEventsReceived;
        ExploreV2Bridge.OnDCLEventsReceived += OnDCLEventsReceived;

        OnGetAllEventsSignedSuccess = OnSuccess;
        OnGetAllEventsSignedFail = OnFail;
        WebInterface.RequestDCLEvents(URL_GET_ALL_EVENTS);
    }

    private void OnDCLEventsReceived(string eventsPayload)
    {
        ExploreV2Bridge.OnDCLEventsReceived -= OnDCLEventsReceived;

        EventListFromAPISignedModel requestResult = JsonUtility.FromJson<EventListFromAPISignedModel>(eventsPayload);
        if (requestResult.ok)
            OnGetAllEventsSignedSuccess?.Invoke(JsonUtility.FromJson<List<EventFromAPIModel>>(requestResult.data));
        else
            OnGetAllEventsSignedFail?.Invoke(requestResult.data);
    }

    public UnityWebRequestAsyncOperation RegisterAttendEvent(string eventId, bool isRegistered, Action OnSuccess, Action<string> OnFail)
    {
        AttendEventRequestModel data = new AttendEventRequestModel
        {
            address = ownUserProfile.userId,
            message = new AttendEventMessageModel
            {
                type = ATTEND_EVENTS_MESSAGE_TYPE,
                timestamp = DateTime.Now.ToLongDateString(),
                @event = eventId,
                attend = isRegistered
            },
            signature = ""
        };

        string json = JsonUtility.ToJson(data);
        var postWebRequest = new UnityWebRequest(URL_POST_MESSAGE, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        postWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        postWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        postWebRequest.SetRequestHeader("Content-Type", "application/json");

        UnityWebRequestAsyncOperation asyncOp = postWebRequest.SendWebRequest();
        asyncOp.completed += (asyncOp) =>
        {
            if (postWebRequest.result == UnityWebRequest.Result.Success)
                OnSuccess?.Invoke();
            else
                OnFail?.Invoke(postWebRequest.downloadHandler.text);
        };

        return asyncOp;
    }
}