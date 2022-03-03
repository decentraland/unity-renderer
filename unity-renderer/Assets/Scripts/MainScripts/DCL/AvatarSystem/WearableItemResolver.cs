using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;

namespace AvatarSystem
{
    public class WearableItemResolver : IWearableItemResolver
    {
        private CancellationTokenSource disposeCts = new CancellationTokenSource();
        private readonly Dictionary<string, WearableItem> wearablesRetrieved = new Dictionary<string, WearableItem>();

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

                Promise<WearableItem> promise = CatalogController.RequestWearable(wearableId);
                // AttachExternalCancellation is needed because a CustomYieldInstruction requires a frame to operate
                await promise.WithCancellation(linkedCts.Token).AttachExternalCancellation(linkedCts.Token);

                // Cancelling is irrelevant at this point,
                // either we have the wearable and we have to add it to forget it later
                // or it's null and we just return it
                if (promise.value != null)
                    wearablesRetrieved.Add(wearableId, promise.value);

                return promise.value;

            }
            catch (OperationCanceledException)
            {
                //No disposing required
                throw;
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
            CatalogController.RemoveWearablesInUse(wearableIds);
        }

        public void Forget(string wearableId) { CatalogController.RemoveWearablesInUse(new List<string> { wearableId }); }

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