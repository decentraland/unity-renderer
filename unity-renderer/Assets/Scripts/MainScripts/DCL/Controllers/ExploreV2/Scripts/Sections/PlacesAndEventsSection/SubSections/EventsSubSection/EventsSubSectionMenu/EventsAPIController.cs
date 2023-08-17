using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
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

    UniTask<(List<EventFromAPIModel>, int total)> SearchEvents(string searchString, int pageNumber, int pageSize, CancellationToken ct);

    UniTask GetDetailedEvent(Action<EventFromAPIModel> OnSuccess, string eventId);

    UniTask RegisterParticipation(string eventId);

    UniTask RemoveParticipation(string eventId);

    UniTask GetCategories(Action<List<CategoryFromAPIModel>> onSuccess, Action<string> onFail);
}

[ExcludeFromCodeCoverage]
public class EventsAPIController : IEventsAPIController
{
    internal const string URL_GET_ALL_EVENTS = "https://events.decentraland.org/api/events";
    private const string URL_GET_DETAILED_EVENT = "https://events.decentraland.org/api/events/{event_id}";
    private const string URL_PARTICIPATE_EVENT = "https://events.decentraland.org/api/events/{event_id}/attendees";
    internal const string URL_GET_CATEGORIES = "https://events.decentraland.org/api/events/categories";

    internal UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private Service<IWebRequestController> webRequestController;

    public async UniTask<(List<EventFromAPIModel>, int total)> SearchEvents(string searchString, int pageNumber, int pageSize, CancellationToken ct)
    {
        const string URL = URL_GET_ALL_EVENTS + "?order=desc&search={0}&offset={1}&limit={2}";
        var result = await webRequestController.Ref.GetAsync(string.Format(URL, searchString.Replace(" ", "+"), pageNumber * pageSize, pageSize), cancellationToken: ct, isSigned: true);

        if (result.result != UnityWebRequest.Result.Success)
            throw new Exception($"Error fetching searched events:\n{result.error}");

        var response = Utils.SafeFromJson<EventListFromAPIModel>(result.downloadHandler.text);

        if (response == null)
            throw new Exception($"Error parsing searched events info:\n{result.downloadHandler.text}");

        if (response.data == null)
            throw new Exception($"No search info retrieved:\n{result.downloadHandler.text}");

        return (response.data, response.total);
    }

    public async UniTask GetAllEvents(Action<List<EventFromAPIModel>> OnSuccess, Action<string> OnFail)
    {
        UnityWebRequest result = await DCL.Environment.i.platform.webRequest.GetAsync(URL_GET_ALL_EVENTS, isSigned: true);
        EventListFromAPIModel eventListFromAPIModel = Utils.SafeFromJson<EventListFromAPIModel>(result.downloadHandler.text);
        OnSuccess?.Invoke(eventListFromAPIModel.data);
    }

    public async UniTask GetDetailedEvent(Action<EventFromAPIModel> OnSuccess, string eventId)
    {
        UnityWebRequest result = await DCL.Environment.i.platform.webRequest.GetAsync(URL_GET_DETAILED_EVENT);
        EventFromAPIModel eventFromAPIModel = Utils.SafeFromJson<EventFromAPIModel>(result.downloadHandler.text);
        OnSuccess?.Invoke(eventFromAPIModel);
    }

    public async UniTask RegisterParticipation(string eventId)
    {
        await DCL.Environment.i.platform.webRequest.PostAsync(URL_PARTICIPATE_EVENT.Replace("{event_id}", eventId), "", isSigned: true);
    }

    public async UniTask RemoveParticipation(string eventId)
    {
        await DCL.Environment.i.platform.webRequest.DeleteAsync(URL_PARTICIPATE_EVENT.Replace("{event_id}", eventId), isSigned: true);
    }

    public async UniTask GetCategories(Action<List<CategoryFromAPIModel>> onSuccess, Action<string> onFail)
    {
        UnityWebRequest result = await DCL.Environment.i.platform.webRequest.GetAsync(URL_GET_CATEGORIES);
        CategoryListFromAPIModel categoriesListFromAPIModel = Utils.SafeFromJson<CategoryListFromAPIModel>(result.downloadHandler.text);
        onSuccess?.Invoke(categoriesListFromAPIModel.data);
    }
}
