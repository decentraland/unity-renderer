using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine;

public class EmotesCatalogBridge : MonoBehaviour, IEmotesCatalogBridge
{
    [Serializable]
    private class AddEmotesResponse
    {
        public EmoteItem[] emotes;
        public string context;
    }

    public event Action<WearableItem[]> OnEmotesReceived;
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

        if (string.IsNullOrEmpty(request.context))
            OnEmotesReceived?.Invoke(request.emotes);
        else
            OnOwnedEmotesReceived?.Invoke(request.emotes, request.context);
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

    private void Update()
    {
        if (emotesToRequestThisFrame.Count == 0)
            return;

        WebInterface.RequestEmotes(
            ownedByUser: null,
            emoteIds: emotesToRequestThisFrame.ToArray(),
            collectionIds: null,
            context: null
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