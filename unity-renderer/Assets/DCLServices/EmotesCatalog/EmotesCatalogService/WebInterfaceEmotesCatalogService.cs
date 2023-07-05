using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;

namespace DCLServices.EmotesCatalog.EmotesCatalogService
{
    public class WebInterfaceEmotesCatalogService : IEmotesCatalogService
    {
        private EmotesCatalogBridge bridge;

        internal readonly Dictionary<string, WearableItem> emotes = new ();
        internal readonly Dictionary<string, HashSet<Promise<WearableItem>>> promises = new ();
        internal readonly Dictionary<string, int> emotesOnUse = new ();
        internal readonly Dictionary<string, HashSet<Promise<IReadOnlyList<WearableItem>>>> ownedEmotesPromisesByUser = new ();

        private IAddressableResourceProvider addressableResourceProvider;
        private EmbeddedEmotesSO embeddedEmotesSO;
        private CancellationTokenSource addressableCTS;
        private int retryCount = 3;

        public WebInterfaceEmotesCatalogService(EmotesCatalogBridge bridge, IAddressableResourceProvider addressableResourceProvider)
        {
            this.bridge = bridge;
            this.addressableResourceProvider = addressableResourceProvider;
        }

        private async UniTaskVoid InitializeAsyncEmbeddedEmotes()
        {
            try
            {
                addressableCTS = new CancellationTokenSource();
                embeddedEmotesSO = await addressableResourceProvider.GetAddressable<EmbeddedEmotesSO>("EmbeddedEmotes.asset", addressableCTS.Token);
            }
            catch (OperationCanceledException) { return; }
            catch (Exception e)
            {
                retryCount--;
                if (retryCount < 0)
                {
                    embeddedEmotesSO = ScriptableObject.CreateInstance<EmbeddedEmotesSO>();
                    embeddedEmotesSO.emotes = new EmbeddedEmote[] { };
                    throw new Exception("Embedded Emotes retry limit reached, they wont work correctly. Please check the Essentials group is set up correctly");
                }

                Debug.LogWarning("Retrying embedded emotes addressables async request...");
                DisposeCT();
                InitializeAsyncEmbeddedEmotes();
            }

            EmbedEmotes();
        }

        public void Initialize()
        {
            InitializeAsyncEmbeddedEmotes();
            bridge.OnEmotesReceived += OnEmotesReceived;
            bridge.OnEmoteRejected += OnEmoteRejected;
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
            if (!ownedEmotesPromisesByUser.TryGetValue(userId, out HashSet<Promise<IReadOnlyList<WearableItem>>> ownedEmotesPromises) || ownedEmotesPromises == null)
                ownedEmotesPromises = new HashSet<Promise<IReadOnlyList<WearableItem>>>();

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
            foreach (Promise<IReadOnlyList<WearableItem>> promise in ownedEmotesPromises)
            {
                promise.Resolve(receivedEmotes);
            }
        }

        private void EmbedEmotes()
        {
            foreach (WearableItem embeddedEmote in embeddedEmotesSO.emotes)
            {
                emotes[embeddedEmote.id] = embeddedEmote;
                emotesOnUse[embeddedEmote.id] = 5000;
            }
        }

        public bool TryGetLoadedEmote(string id, out WearableItem emote) { return emotes.TryGetValue(id, out emote); }

        public Promise<IReadOnlyList<WearableItem>> RequestOwnedEmotes(string userId)
        {
            var promise = new Promise<IReadOnlyList<WearableItem>>();
            if (!ownedEmotesPromisesByUser.ContainsKey(userId) || ownedEmotesPromisesByUser[userId] == null)
                ownedEmotesPromisesByUser[userId] = new HashSet<Promise<IReadOnlyList<WearableItem>>>();
            ownedEmotesPromisesByUser[userId].Add(promise);
            bridge.RequestOwnedEmotes(userId);

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

        public async UniTask<IReadOnlyList<WearableItem>> RequestEmotesAsync(IList<string> ids, CancellationToken ct = default)
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
            if(embeddedEmotesSO == null)
                await UniTask.WaitUntil(() => embeddedEmotesSO != null);
            return embeddedEmotesSO;
        }

        public void Dispose()
        {
            bridge.OnEmotesReceived -= OnEmotesReceived;
            bridge.OnEmoteRejected -= OnEmoteRejected;
            bridge.OnOwnedEmotesReceived -= OnOwnedEmotesReceived;
            DisposeCT();
        }

        private void DisposeCT()
        {
            if (addressableCTS != null)
            {
                addressableCTS.Cancel();
                addressableCTS.Dispose();
                addressableCTS = null;
            }
        }

    }
}
