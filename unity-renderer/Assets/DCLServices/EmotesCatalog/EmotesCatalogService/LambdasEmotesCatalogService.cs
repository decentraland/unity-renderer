using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.EmotesCatalog;
using DCLServices.Lambdas;
using DCLServices.WearablesCatalogService;
using System;
using UnityEngine;

public class LambdasEmotesCatalogService : IEmotesCatalogService
{
    [Serializable]
    internal class WearableRequest
    {
        public List<string> pointers;
    }

    [Serializable]
    private class EmoteCollectionResponse
    {
        public EmoteEntityDto[] entities;
    }

    internal readonly Dictionary<string, WearableItem> emotes = new ();
    internal readonly Dictionary<string, HashSet<Promise<WearableItem>>> promises = new ();
    internal readonly Dictionary<string, int> emotesOnUse = new ();
    internal readonly Dictionary<string, HashSet<Promise<IReadOnlyList<WearableItem>>>> ownedEmotesPromisesByUser = new ();

    private readonly IEmotesRequestSource emoteSource;
    private readonly IAddressableResourceProvider addressableResourceProvider;
    private readonly ICatalyst catalyst;
    private readonly ILambdasService lambdasService;
    private readonly DataStore dataStore;

    private EmbeddedEmotesSO embeddedEmotesSo;
    private CancellationTokenSource addressableCts;
    private int retryCount = 3;

    private string assetBundlesUrl => dataStore.featureFlags.flags.Get().IsFeatureEnabled("ab-new-cdn") ? "https://ab-cdn.decentraland.org/" : "https://content-assets-as-bundle.decentraland.org/";

    public LambdasEmotesCatalogService(IEmotesRequestSource emoteSource,
        IAddressableResourceProvider addressableResourceProvider,
        ICatalyst catalyst,
        ILambdasService lambdasService,
        DataStore dataStore)
    {
        this.emoteSource = emoteSource;
        this.addressableResourceProvider = addressableResourceProvider;
        this.catalyst = catalyst;
        this.lambdasService = lambdasService;
        this.dataStore = dataStore;
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
                embeddedEmotesSo.Clear();
                throw new Exception("Embedded Emotes retry limit reached, they wont work correctly. Please check the Essentials group is set up correctly");
            }

            Debug.LogWarning("Retrying embedded emotes addressables async request...");
            DisposeAddressableCancellationToken();
            InitializeAsyncEmbeddedEmotes().Forget();
        }

        EmbedEmotes();
    }

    public void Initialize()
    {
        InitializeAsyncEmbeddedEmotes().Forget();
        emoteSource.OnEmotesReceived += OnEmotesReceived;
        emoteSource.OnEmoteRejected += OnEmoteRejected;
        emoteSource.OnOwnedEmotesReceived += OnOwnedEmotesReceived;
    }

    public void Dispose()
    {
        emoteSource.OnEmotesReceived -= OnEmotesReceived;
        emoteSource.OnEmoteRejected -= OnEmoteRejected;
        emoteSource.OnOwnedEmotesReceived -= OnOwnedEmotesReceived;
        DisposeAddressableCancellationToken();
    }

    public bool TryGetLoadedEmote(string id, out WearableItem emote)
    {
        return emotes.TryGetValue(id, out emote);
    }

    public async UniTask<WearableItem> RequestEmoteFromBuilderAsync(string emoteId, CancellationToken cancellationToken)
    {
        const string TEMPLATE_URL = "https://builder-api.decentraland.org/v1/items/:emoteId/";
        string url = TEMPLATE_URL.Replace(":emoteId", emoteId);

        try
        {
            (WearableItemResponseFromBuilder response, bool success) = await lambdasService.GetFromSpecificUrl<WearableItemResponseFromBuilder>(
                TEMPLATE_URL, url,
                isSigned: true,
                cancellationToken: cancellationToken);

            if (!success)
                throw new Exception($"The request of wearables from builder '{emoteId}' failed!");

            WearableItem wearable = response.data.ToWearableItem(
                "https://builder-api.decentraland.org/v1/storage/contents/",
                assetBundlesUrl);

            if (!wearable.IsEmote()) return null;

            OnEmoteReceived(wearable);

            return wearable;
        }
        catch (UnityWebRequestException ex)
        {
            if (ex.ResponseCode == 404)
                Debug.LogWarning($"Emote with id: {emoteId} does not exist");
            else
                Debug.LogException(ex);

            return null;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }

    public Promise<IReadOnlyList<WearableItem>> RequestOwnedEmotes(string userId)
    {
        var promise = new Promise<IReadOnlyList<WearableItem>>();

        if (!ownedEmotesPromisesByUser.ContainsKey(userId) || ownedEmotesPromisesByUser[userId] == null)
            ownedEmotesPromisesByUser[userId] = new HashSet<Promise<IReadOnlyList<WearableItem>>>();

        ownedEmotesPromisesByUser[userId].Add(promise);

        emoteSource.RequestOwnedEmotes(userId);

        return promise;
    }

    public async UniTask<IReadOnlyList<WearableItem>> RequestOwnedEmotesAsync(string userId, CancellationToken ct = default)
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
        catch (OperationCanceledException e) { return null; }
        finally
        {
            timeout?.Dispose();
            timeoutCTS?.Dispose();
        }

        return promise.value;
    }

    public async UniTask<IReadOnlyList<WearableItem>> RequestEmoteCollectionAsync(IEnumerable<string> collectionIds,
        CancellationToken cancellationToken, List<WearableItem> emoteBuffer = null)
    {
        List<WearableItem> emotes = emoteBuffer ?? new List<WearableItem>();
        var templateURL = $"{catalyst.contentUrl}entities/active/collections/:collectionId";

        foreach (string collectionId in collectionIds)
        {
            string url = templateURL.Replace(":collectionId", collectionId);

            (EmoteCollectionResponse response, bool success) = await lambdasService.GetFromSpecificUrl<EmoteCollectionResponse>(
                templateURL, url,
                cancellationToken: cancellationToken);

            if (!success)
                throw new Exception($"The request for collection of emotes '{collectionId}' failed!");

            foreach (EmoteEntityDto dto in response.entities)
            {
                var contentUrl = $"{catalyst.contentUrl}contents/";
                var wearable = dto.ToWearableItem(contentUrl);
                wearable.baseUrl = contentUrl;
                wearable.baseUrlBundles = assetBundlesUrl;
                emotes.Add(wearable);
            }
        }

        OnEmotesReceived(emotes);

        return emotes;
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
        catch (UnityWebRequestException ex)
        {
            Debug.LogWarning($"Emote with id:{id} does not exist or connection failed");
            return null;
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
        catch (Exception ex)
        {
            Debug.LogException(ex);
            return null;
        }
        finally
        {
            timeout?.Dispose();
            timeoutCTS?.Dispose();
        }

        return promise.value;
    }

    public async UniTask<IReadOnlyList<WearableItem>> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            var tasks = ids.Select(x => RequestEmoteAsync(x, ct));
            return await UniTask.WhenAll(tasks).AttachExternalCancellation(ct);
        }
        catch (OperationCanceledException e) { return null; }
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
        if (embeddedEmotesSo == null)
            await UniTask.WaitUntil(() => embeddedEmotesSo != null);

        return embeddedEmotesSo;
    }

    public async UniTask<IReadOnlyList<WearableItem>> RequestEmoteCollectionInBuilderAsync(IEnumerable<string> collectionIds,
        CancellationToken cancellationToken, List<WearableItem> emoteBuffer = null)
    {
        const string TEMPLATE_URL = "https://builder-api.decentraland.org/v1/collections/:collectionId/items/";

        var emotes = emoteBuffer ?? new List<WearableItem>();

        var queryParams = new[]
        {
            ("page", "1"),
            ("limit", "5000"),
        };

        foreach (string collectionId in collectionIds)
        {
            string url = TEMPLATE_URL.Replace(":collectionId", collectionId);

            (WearableCollectionResponseFromBuilder response, bool success) = await lambdasService.GetFromSpecificUrl<WearableCollectionResponseFromBuilder>(
                TEMPLATE_URL, url,
                cancellationToken: cancellationToken,
                isSigned: true,
                urlEncodedParams: queryParams);

            if (!success)
                throw new Exception($"The request for collection of emotes '{collectionId}' failed!");

            foreach (BuilderWearable bw in response.data.results)
            {
                var wearable = bw.ToWearableItem("https://builder-api.decentraland.org/v1/storage/contents/",
                    assetBundlesUrl);
                if (!wearable.IsEmote()) continue;
                emotes.Add(wearable);
            }
        }

        OnEmotesReceived(emotes);

        return emotes;
    }

    private void DisposeAddressableCancellationToken()
    {
        if (addressableCts == null) return;
        addressableCts.Cancel();
        addressableCts.Dispose();
        addressableCts = null;
    }

    private void OnEmotesReceived(IEnumerable<WearableItem> receivedEmotes)
    {
        foreach (var t in receivedEmotes)
            OnEmoteReceived(t);
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
        if (!promises.TryGetValue(emoteId, out var setOfPromises)) return;

        foreach (var promise in setOfPromises)
            promise.Reject(errorMessage);

        promises.Remove(emoteId);
        emotesOnUse.Remove(emoteId);
        emotes.Remove(emoteId);
    }

    private void OnOwnedEmotesReceived(IReadOnlyList<WearableItem> receivedEmotes, string userId)
    {
        if (!ownedEmotesPromisesByUser.TryGetValue(userId, out HashSet<Promise<IReadOnlyList<WearableItem>>> ownedEmotesPromises) || ownedEmotesPromises == null)
            ownedEmotesPromises = new HashSet<Promise<IReadOnlyList<WearableItem>>>();

        //Update emotes on use
        foreach (var emote in receivedEmotes)
        {
            emotesOnUse.TryAdd(emote.id, 0);
            emotesOnUse[emote.id] += ownedEmotesPromises.Count;
        }

        OnEmotesReceived(receivedEmotes);

        //Resolve ownedEmotesPromise
        ownedEmotesPromisesByUser.Remove(userId);

        foreach (Promise<IReadOnlyList<WearableItem>> promise in ownedEmotesPromises)
            promise.Resolve(receivedEmotes);
    }

    private void EmbedEmotes()
    {
        foreach (EmbeddedEmote embeddedEmote in embeddedEmotesSo.GetAllEmotes())
        {
            emotes[embeddedEmote.id] = embeddedEmote;
            emotesOnUse[embeddedEmote.id] = 5000;
        }
    }
}
