using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.Lambdas;
using System;
using UnityEngine;

public class EmotesCatalogService : IEmotesCatalogService
{
    [Serializable]
    internal class WearableRequest
    {
        public List<string> pointers;
    }

    internal readonly Dictionary<string, WearableItem> emotes = new ();
    internal readonly Dictionary<string, HashSet<Promise<WearableItem>>> promises = new ();
    internal readonly Dictionary<string, int> emotesOnUse = new ();
    internal readonly Dictionary<string, HashSet<Promise<WearableItem[]>>> ownedEmotesPromisesByUser = new ();

    private readonly IEmotesRequestSource emoteSource;
    private readonly IAddressableResourceProvider addressableResourceProvider;

    private EmbeddedEmotesSO embeddedEmotesSo;
    private CancellationTokenSource addressableCts;
    private int retryCount = 3;

    public EmotesCatalogService(IEmotesRequestSource emoteSource, IAddressableResourceProvider addressableResourceProvider)
    {
        this.emoteSource = emoteSource;
        this.addressableResourceProvider = addressableResourceProvider;

        InitializeAsyncEmbeddedEmotes().Forget();
    }

    private async UniTaskVoid InitializeAsyncEmbeddedEmotes()
    {
        try
        {
            addressableCts = new CancellationTokenSource();
            embeddedEmotesSo = await addressableResourceProvider.GetAddressable<EmbeddedEmotesSO>("EmbeddedEmotes.asset", addressableCts.Token);
        }
        catch (OperationCanceledException) { return; }
        catch (Exception e)
        {
            retryCount--;
            if (retryCount < 0)
            {
                embeddedEmotesSo = ScriptableObject.CreateInstance<EmbeddedEmotesSO>();
                embeddedEmotesSo.emotes = new EmbeddedEmote[] { };
                throw new Exception("Embedded Emotes retry limit reached, they wont work correctly. Please check the Essentials group is set up correctly");
            }

            Debug.LogWarning("Retrying embedded emotes addressables async request...");
            DisposeCT();
            InitializeAsyncEmbeddedEmotes().Forget();
        }

        EmbedEmotes();
    }

    public void Initialize()
    {
        emoteSource.OnEmotesReceived += OnEmotesReceived;
        emoteSource.OnEmoteRejected += OnEmoteRejected;
        emoteSource.OnOwnedEmotesReceived += OnOwnedEmotesReceived;
    }

    private void OnEmotesReceived(WearableItem[] receivedEmotes)
    {
        foreach (var t in receivedEmotes)
        {
            OnEmoteReceived(t);
        }
    }

    private void OnEmoteReceived(WearableItem emote)
    {
        if (!emotesOnUse.ContainsKey(emote.id) || emotesOnUse[emote.id] <= 0)
            return;

        emotes[emote.id] = emote;

        if (promises.TryGetValue(emote.id, out var emotePromises))
        {
            foreach (Promise<WearableItem> promise in emotePromises)
                promise.Resolve(emote);

            promises.Remove(emote.id);
        }
    }

    private void OnEmoteRejected(string emoteId, string errorMessage)
    {
        if (promises.TryGetValue(emoteId, out var setOfPromises))
        {
            foreach(var promise in setOfPromises)
                promise.Reject(errorMessage);

            promises.Remove(emoteId);
            emotesOnUse.Remove(emoteId);
            emotes.Remove(emoteId);
        }
    }

    private void OnOwnedEmotesReceived(WearableItem[] receivedEmotes, string userId)
    {
        if (!ownedEmotesPromisesByUser.TryGetValue(userId, out HashSet<Promise<WearableItem[]>> ownedEmotesPromises) || ownedEmotesPromises == null)
            ownedEmotesPromises = new HashSet<Promise<WearableItem[]>>();

        //Update emotes on use
        foreach (var emote in receivedEmotes)
        {
            emotesOnUse.TryAdd(emote.id, 0);
            emotesOnUse[emote.id] += ownedEmotesPromises.Count;
        }

        OnEmotesReceived(receivedEmotes);

        //Resolve ownedEmotesPromise
        ownedEmotesPromisesByUser.Remove(userId);
        foreach (Promise<WearableItem[]> promise in ownedEmotesPromises)
            promise.Resolve(receivedEmotes);
    }

    private void EmbedEmotes()
    {
        foreach (EmbeddedEmote embeddedEmote in embeddedEmotesSo.emotes)
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

        emoteSource.RequestOwnedEmotes(userId);

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
            await promise.WithCancellation(linkedCt.Token);
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

        emoteSource.RequestEmote(id);

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
            await promise.WithCancellation(linkedCt.Token);
        }
        catch (PromiseException ex)
        {
            Debug.LogWarning($"Emote with id:{id} was rejected");
            return null;
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

    public async UniTask<EmbeddedEmotesSO> GetEmbeddedEmotes()
    {
        if(embeddedEmotesSo == null)
            await UniTask.WaitUntil(() => embeddedEmotesSo != null);
        return embeddedEmotesSo;
    }

    public void Dispose()
    {
        emoteSource.OnEmotesReceived -= OnEmotesReceived;
        emoteSource.OnEmoteRejected -= OnEmoteRejected;
        emoteSource.OnOwnedEmotesReceived -= OnOwnedEmotesReceived;
        DisposeCT();
    }

    private void DisposeCT()
    {
        if (addressableCts != null)
        {
            addressableCts.Cancel();
            addressableCts.Dispose();
            addressableCts = null;
        }
    }
}
