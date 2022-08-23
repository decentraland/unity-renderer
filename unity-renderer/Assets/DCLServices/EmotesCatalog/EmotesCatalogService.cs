using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;

public class EmotesCatalogService : IEmotesCatalogService
{
    private readonly IEmotesCatalogBridge bridge;

    internal readonly Dictionary<string, WearableItem> emotes = new Dictionary<string, WearableItem>();
    internal readonly Dictionary<string, HashSet<Promise<WearableItem>>> promises = new Dictionary<string, HashSet<Promise<WearableItem>>>();
    internal readonly Dictionary<string, int> emotesOnUse = new Dictionary<string, int>();
    internal readonly Dictionary<string, HashSet<Promise<WearableItem[]>>> ownedEmotesPromises = new Dictionary<string, HashSet<Promise<WearableItem[]>>>();

    public EmotesCatalogService(IEmotesCatalogBridge bridge, WearableItem[] embeddedEmotes)
    {
        this.bridge = bridge;
        EmbedEmotes(embeddedEmotes);
    }

    public void Initialize()
    {
        bridge.OnEmotesReceived += OnEmotesReceived;
        bridge.OnOwnedEmotesReceived += OnOwnedEmotesReceived;
    }

    private void OnEmotesReceived(WearableItem[] receivedEmotes)
    {
        for (var i = 0; i < receivedEmotes.Length; i++)
        {
            var emote = receivedEmotes[i];
            //If we dont have promises waiting, this emote is not needed and can be ignored
            if (!promises.TryGetValue(emote.id, out var emotePromises) || emotePromises.Count == 0)
            {
                emotesOnUse.Remove(emote.id);
                continue;
            }

            emotes[emote.id] = emote;
            emotesOnUse[emote.id] = emotePromises.Count;
            foreach (Promise<WearableItem> promise in emotePromises)
            {
                promise.Resolve(emote);
            }
            promises.Remove(emote.id);
        }
    }

    private void OnOwnedEmotesReceived(WearableItem[] receivedEmotes, string userId)
    {
        if (!ownedEmotesPromises.TryGetValue(userId, out var ownedWearablePromises))
            return;

        if (ownedWearablePromises == null)
            return;

        ownedEmotesPromises.Remove(userId);
        foreach (Promise<WearableItem[]> promise in ownedWearablePromises)
        {
            for (int i = 0; i < receivedEmotes.Length; i++)
            {
                var emote = receivedEmotes[i];
                string id = emote.id;

                emotes[id] = emote;
                if (!emotesOnUse.ContainsKey(id))
                    emotesOnUse[id] = 0;
                emotesOnUse[id]++;

                if (promises.TryGetValue(emote.id, out var emotePromises))
                {
                    //Resolve pending promises for this emote
                    foreach (Promise<WearableItem> otherPromise in emotePromises)
                    {
                        otherPromise.Resolve(emote);
                        emotesOnUse[id]++;
                    }
                    promises.Remove(emote.id);
                }
            }
            promise.Resolve(receivedEmotes);
        }
    }

    private void EmbedEmotes(WearableItem[] embeddedEmotes)
    {
        foreach (WearableItem embeddedEmote in embeddedEmotes)
        {
            emotes[embeddedEmote.id] = embeddedEmote;
            emotesOnUse[embeddedEmote.id] = 5000;
        }
    }

    public bool TryGetLoadedEmote(string id, out WearableItem emote) { return emotes.TryGetValue(id, out emote); }

    public Promise<WearableItem[]> RequestOwnedEmotes(string userId)
    {
        var promise = new Promise<WearableItem[]>();
        if (!ownedEmotesPromises.ContainsKey(userId) || ownedEmotesPromises[userId] == null)
            ownedEmotesPromises[userId] = new HashSet<Promise<WearableItem[]>>();
        ownedEmotesPromises[userId].Add(promise);
        bridge.RequestOwnedEmotes(userId);

        return promise;
    }

    public async UniTask<WearableItem[]> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default)
    {
        const int TIMEOUT = 60;
        CancellationTokenSource timeoutCTS = new CancellationTokenSource();
        var timeout = timeoutCTS.CancelAfterSlim(TimeSpan.FromSeconds(TIMEOUT));
        var promise = RequestOwnedEmotes(userId);
        try
        {
            ct.ThrowIfCancellationRequested();
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCTS.Token);
            await promise.WithCancellation(linkedCt.Token).AttachExternalCancellation(linkedCt.Token);
        }
        catch (OperationCanceledException e)
        {
            return null;
        }
        finally
        {
            timeout?.Dispose();
            timeoutCTS?.Dispose();
        }

        return promise.value;
    }

    public Promise<WearableItem> RequestEmote(string id)
    {
        var promise = new Promise<WearableItem>();
        if (emotes.TryGetValue(id, out var emote))
        {
            promise.Resolve(emote);
            return promise;
        }

        if (!promises.ContainsKey(id) || promises[id] == null)
            promises[id] = new HashSet<Promise<WearableItem>>();
        promises[id].Add(promise);
        bridge.RequestEmote(id);
        return promise;
    }

    public List<Promise<WearableItem>> RequestEmotes(IList<string> ids)
    {
        List<Promise<WearableItem>> requestedPromises = new List<Promise<WearableItem>>(ids.Count);

        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            requestedPromises.Add(RequestEmote(id));
        }

        return requestedPromises;
    }

    public async UniTask<WearableItem> RequestEmoteAsync(string id, CancellationToken ct = default)
    {
        const int TIMEOUT = 60;
        CancellationTokenSource timeoutCTS = new CancellationTokenSource();
        var timeout = timeoutCTS.CancelAfterSlim(TimeSpan.FromSeconds(TIMEOUT));
        ct.ThrowIfCancellationRequested();
        Promise<WearableItem> promise = RequestEmote(id);
        try
        {
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCTS.Token);
            await promise.WithCancellation(linkedCt.Token).AttachExternalCancellation(linkedCt.Token);
        }
        catch (OperationCanceledException e)
        {
            if (promises.ContainsKey(id))
            {
                promises[id].Remove(promise);
                if (promises[id].Count == 0)
                    promises.Remove(id);
            }

            return null;
        }
        finally
        {
            timeout?.Dispose();
            timeoutCTS?.Dispose();
        }

        return promise.value;
    }

    public async UniTask<WearableItem[]> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var tasks = ids.Select(x => RequestEmoteAsync(x, ct));
            return await UniTask.WhenAll(tasks).AttachExternalCancellation(ct);
        }
        catch (OperationCanceledException e)
        {
            return null;
        }
    }

    public void ForgetEmote(string id)
    {
        if (emotesOnUse.ContainsKey(id))
        {
            emotesOnUse[id]--;
            if (emotesOnUse[id] > 0) //We are still using this emote
                return;
            emotesOnUse.Remove(id);
        }

        if (!emotes.TryGetValue(id, out WearableItem emote))
            return;

        emotes.Remove(id);
    }

    public void ForgetEmotes(IList<string> ids)
    {
        for (int i = 0; i < ids.Count; i++)
        {
            string id = ids[i];
            ForgetEmote(id);
        }
    }

    public void Dispose()
    {
        bridge.OnEmotesReceived -= OnEmotesReceived;
        bridge.OnOwnedEmotesReceived -= OnOwnedEmotesReceived;
    }
}