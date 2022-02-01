using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public interface IWearableItemResolver : IDisposable
    {
        UniTask<WearableItem[]> Resolve(IEnumerable<string> wearableId, CancellationToken ct = default);
        UniTask<WearableItem> Resolve(string wearableId, CancellationToken ct = default);

        void Forget(List<string> wearableIds);
        void Forget(string wearableId);
    }
}