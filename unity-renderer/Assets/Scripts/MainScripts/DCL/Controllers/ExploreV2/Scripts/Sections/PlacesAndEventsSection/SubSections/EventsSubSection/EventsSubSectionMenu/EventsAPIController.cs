using DCL;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public interface IEventsAPIController
{
    WebRequestAsyncOperation GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail);
}

public class EventsAPIController : IEventsAPIController
{
    private const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    private const string URL_POST_MESSAGE = "https://events.decentraland.org/api/message";

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

    //void RegisterAttendEvent(string eventId, bool isRegistered)
    //{
    //    EventPostMessageModel data = new EventPostMessageModel
    //    {
    //        type = "attend",
    //        timestamp = DateTime.Now.ToString(),
    //        @event = eventId,
    //        attend = isRegistered
    //    };

    //    StartCoroutine(PostRequest(URL_POST_MESSAGE, JsonUtility.ToJson(data)));
    //}

    //IEnumerator PostRequest(string url, string json)
    //{
    //    var postWebRequest = new UnityWebRequest(url, "POST");
    //    byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
    //    postWebRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
    //    postWebRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
    //    postWebRequest.SetRequestHeader("Content-Type", "application/json");

    //    // Send the request then wait here until it returns
    //    yield return postWebRequest.SendWebRequest();

    //    if (postWebRequest.result == UnityWebRequest.Result.ConnectionError)
    //    {
    //        Debug.Log("Error While Sending: " + postWebRequest.error);
    //    }
    //    else
    //    {
    //        Debug.Log("Received: " + postWebRequest.downloadHandler.text);
    //    }
    //}
}