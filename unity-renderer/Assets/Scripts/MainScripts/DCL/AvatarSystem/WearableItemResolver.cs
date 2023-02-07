using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;

namespace AvatarSystem
{
    public class WearableItemResolver : IWearableItemResolver
    {
        private CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly Dictionary<string, WearableItem> wearablesRetrieved = new Dictionary<string, WearableItem>();

        public async UniTask<(List<WearableItem> wearables, List<WearableItem> emotes)> ResolveAndSplit(IEnumerable<string> wearableIds, CancellationToken ct = default)
        {
            try
            {
                WearableItem[] allItems = await Resolve(wearableIds, ct);

                List<WearableItem> wearables = new List<WearableItem>();
                List<WearableItem> emotes = new List<WearableItem>();

                for (int i = 0; i < allItems.Length; i++)
                {
                    if (allItems[i] == null)
                        continue;

                    if (allItems[i].IsEmote())
                        emotes.Add(allItems[i]);
                    else
                        wearables.Add(allItems[i]);
                }

                return (
                    wearables,
                    emotes
                );
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }
        }

        public async UniTask<WearableItem[]> Resolve(IEnumerable<string> wearableId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            try
            {

                return await UniTask.WhenAll(wearableId.Select(x => Resolve(x, ct)));
            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
            }
        }

        public async UniTask<WearableItem> Resolve(string wearableId, CancellationToken ct = default)
        {
            if (disposeCts == null)
                disposeCts = new CancellationTokenSource();
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCts.Token);

            linkedCts.Token.ThrowIfCancellationRequested();

            try
            {
                if (wearablesRetrieved.ContainsKey(wearableId))
                    return wearablesRetrieved[wearableId];

                var wearable = await DCL.Environment.i.serviceLocator.Get<IWearablesCatalogService>().RequestWearableAsync(wearableId, linkedCts.Token);

                if (wearable != null)
                    wearablesRetrieved.Add(wearableId, wearable);

                return wearable;

            }
            catch (Exception ex) when (ex is OperationCanceledException or PromiseException)
            {
                wearablesRetrieved.Remove(wearableId);
                return null;
            }
            finally
            {
                disposeCts?.Dispose();
                disposeCts = null;
            }
        }

        public void Forget(List<string> wearableIds)
        {
            foreach (string wearableId in wearableIds)
            {
                wearablesRetrieved.Remove(wearableId);
            }
            DCL.Environment.i.serviceLocator.Get<IWearablesCatalogService>().RemoveWearablesInUse(wearableIds);
        }

        public void Forget(string wearableId) { DCL.Environment.i.serviceLocator.Get<IWearablesCatalogService>().RemoveWearablesInUse(new List<string> { wearableId }); }

        public void Dispose()
        {
            disposeCts?.Cancel();
            disposeCts?.Dispose();
            disposeCts = null;
            Forget(wearablesRetrieved.Keys.ToList());
            wearablesRetrieved.Clear();
        }
    }
}
