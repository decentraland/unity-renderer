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
    internal readonly Dictionary<string, HashSet<Promise<WearableItem[]>>> ownedEmotesPromisesByUser = new Dictionary<string, HashSet<Promise<WearableItem[]>>>();

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
            WearableItem emote = receivedEmotes[i];

            if (!emotesOnUse.ContainsKey(emote.id) || emotesOnUse[emote.id] <= 0)
                continue;

            emotes[emote.id] = emote;
            
            
            
            if (promises.TryGetValue(emote.id, out var emotePromises))
            {
                foreach (Promise<WearableItem> promise in emotePromises)
                {
                    promise.Resolve(emote);
                }
                promises.Remove(emote.id);
            }
        }
    }

    private void OnOwnedEmotesReceived(WearableItem[] receivedEmotes, string userId)
    {
        if (!ownedEmotesPromisesByUser.TryGetValue(userId, out HashSet<Promise<WearableItem[]>> ownedEmotesPromises) || ownedEmotesPromises == null)
            ownedEmotesPromises = new HashSet<Promise<WearableItem[]>>();

        //Update emotes on use
        for (var i = 0; i < receivedEmotes.Length; i++)
        {
            var emote = receivedEmotes[i];
            if (!emotesOnUse.ContainsKey(emote.id))
                emotesOnUse[emote.id] = 0;
            emotesOnUse[emote.id] += ownedEmotesPromises.Count;
        }

        OnEmotesReceived(receivedEmotes);

        //Resolve ownedEmotesPromise
        ownedEmotesPromisesByUser.Remove(userId);
        foreach (Promise<WearableItem[]> promise in ownedEmotesPromises)
        {
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
        if (!ownedEmotesPromisesByUser.ContainsKey(userId) || ownedEmotesPromisesByUser[userId] == null)
            ownedEmotesPromisesByUser[userId] = new HashSet<Promise<WearableItem[]>>();
        ownedEmotesPromisesByUser[userId].Add(promise);
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
        if (!emotesOnUse.ContainsKey(id))
            emotesOnUse[id] = 0;
        emotesOnUse[id]++;
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
        const int TIMEOUT = 45;
        CancellationTokenSource timeoutCTS = new CancellationTokenSource();
        var timeout = timeoutCTS.CancelAfterSlim(TimeSpan.FromSeconds(TIMEOUT));
        ct.ThrowIfCancellationRequested();
        Promise<WearableItem> promise = RequestEmote(id);
        try
        {
            var linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCTS.Token);
            await promise.WithCancellation(linkedCt.Token).AttachExternalCancellation(linkedCt.Token);
        }
        catch (OperationCanceledException ex)
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