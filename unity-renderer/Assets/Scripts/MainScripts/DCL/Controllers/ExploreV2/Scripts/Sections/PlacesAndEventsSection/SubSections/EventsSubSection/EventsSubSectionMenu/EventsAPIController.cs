using DCL;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface IEventsAPIController
{
    WebRequestAsyncOperation GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);
    UnityWebRequestAsyncOperation RegisterAttendEvent(string eventId, bool isRegistered, Action OnSuccess, Action<string> OnFail);
}

public class EventsAPIController : IEventsAPIController
{
    internal const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    internal const string URL_POST_MESSAGE = "https://events.decentraland.org/api/message";

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

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

    public UnityWebRequestAsyncOperation RegisterAttendEvent(string eventId, bool isRegistered, Action OnSuccess, Action<string> OnFail)
    {
        AttendEventRequestModel data = new AttendEventRequestModel
        {
            address = ownUserProfile.userId,
            message = new AttendEventMessageModel
            {
                type = "attend",
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