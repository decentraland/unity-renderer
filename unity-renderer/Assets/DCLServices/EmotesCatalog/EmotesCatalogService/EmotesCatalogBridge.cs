using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class EmotesCatalogBridge : MonoBehaviour
{
    [Serializable]
    private class AddEmotesResponse
    {
        public EmoteItem[] emotes;
        public string context;
    }

    public event Action<WearableItem[]> OnEmotesReceived;
    public event EmoteRejectedDelegate OnEmoteRejected;
    public event Action<WearableItem[], string> OnOwnedEmotesReceived;

    private readonly HashSet<string> emotesToRequestThisFrame = new HashSet<string>();
    private bool isAlreadyDestroyed;

    public static EmotesCatalogBridge GetOrCreate()
    {
        var brigeGO = SceneReferences.i?.bridgeGameObject;
        if (SceneReferences.i?.bridgeGameObject == null)
            return new GameObject("Bridge").AddComponent<EmotesCatalogBridge>();

        return brigeGO.GetOrCreateComponent<EmotesCatalogBridge>();
    }

    public void RequestEmote(string emoteId) { emotesToRequestThisFrame.Add(emoteId); }

    // Alex: If at some point base emotes are not embedded in the client but sent by kernel
    // this call wouldn't be listened by any Promise and wont be processed.
    // This issue can be solved easily by adding a different call AddBaseEmotes and a different event
    public void AddEmotesToCatalog(string payload)
    {
        AddEmotesResponse request = null;

        try
        {
            request = JsonUtility.FromJson<AddEmotesResponse>(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"Fail to parse emote json {e}");
        }

        if (request == null)
            return;

        var context = request.context;
        if (string.IsNullOrEmpty(context))
        {
            Debug.LogError("EmotesCatalogBridge error: empty context is not supposed to be in the wearables request");
        }
        else
        {
            if (request.context.StartsWith("emotes:"))
            {
                ResolveMissingEmotesRejection(request.context, request.emotes);
                OnEmotesReceived?.Invoke(request.emotes);
            }
            else
            {
                OnOwnedEmotesReceived?.Invoke(request.emotes, request.context);
            }
        }
    }

    private void ResolveMissingEmotesRejection(string requestContext, EmoteItem[] emotes)
    {
        var emoteIdsFromContext = ContextStringToEmoteIds(requestContext);
        var loadedEmoteIds = new HashSet<string>();
        foreach (var emote in emotes)
        {
            loadedEmoteIds.Add(emote.id);
        }

        foreach(var emoteId in emoteIdsFromContext)
            if (!loadedEmoteIds.Contains(emoteId))
            {
                OnEmoteRejected?.Invoke(emoteId, "Emote from context not found in response: ");
            }
    }

    public void RequestOwnedEmotes(string userId)
    {
        WebInterface.RequestEmotes(
            ownedByUser: userId,
            emoteIds: null,
            collectionIds: null,
            context: $"{userId}"
        );
    }

    private static string EmoteIdsToContextString(IEnumerable<string> emoteIds)
    {
        var commaJoinedEmoteIds = string.Join(",", emoteIds);
        return "emotes:" + commaJoinedEmoteIds;
    }

    private static string[] ContextStringToEmoteIds(string context)
    {
        context = context.Replace("emotes:", "");
        return context.Split(',');
    }

    private void Update()
    {
        if (emotesToRequestThisFrame.Count == 0)
            return;

        var commaJoinedEmoteIds = string.Join(",", emotesToRequestThisFrame);

        WebInterface.RequestEmotes(
            ownedByUser: null,
            emoteIds: emotesToRequestThisFrame.ToArray(),
            collectionIds: null,
            context: EmoteIdsToContextString(emotesToRequestThisFrame)
        );


        emotesToRequestThisFrame.Clear();
    }

    public void Dispose()
    {
        if (isAlreadyDestroyed)
            return;
        Destroy(gameObject);
    }

    private void OnDestroy() { isAlreadyDestroyed = true; }
}
