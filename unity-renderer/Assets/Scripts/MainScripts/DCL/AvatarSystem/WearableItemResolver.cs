using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;

namespace AvatarSystem
{
    public class WearableItemResolver : IWearableItemResolver
    {
        private CancellationTokenSource disposeCTS = new CancellationTokenSource();
        private readonly Dictionary<string, WearableItem> wearablesRetrieved = new Dictionary<string, WearableItem>();

        public async UniTask<WearableItem[]> Resolve(IEnumerable<string> wearableId, CancellationToken ct = default) { return await UniTask.WhenAll(wearableId.Select(x => Resolve(x, ct))); }

        public async UniTask<WearableItem> Resolve(string wearableId, CancellationToken ct = default)
        {
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, disposeCTS.Token);

            if (wearablesRetrieved.ContainsKey(wearableId))
                return wearablesRetrieved[wearableId];

            if (linkedCts.IsCancellationRequested)
                return null;

            Promise<WearableItem> promise = CatalogController.RequestWearable(wearableId);
            await promise.WithCancellation(linkedCts.Token);

            // Cancelling is irrelevant at this point,
            // either we have the wearable and we have to add it to forget it later
            // or it's null and we just return it
            if (promise.value != null)
                wearablesRetrieved.Add(wearableId, promise.value);

            return promise.value;
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
            disposeCTS.Cancel();
            disposeCTS = new CancellationTokenSource();
            Forget(wearablesRetrieved.Keys.ToList());
            wearablesRetrieved.Clear();
        }
    }
}