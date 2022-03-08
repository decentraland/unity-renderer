using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public interface IAvatarCurator : IDisposable
    {
        UniTask<(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, List<WearableItem> emotes)> Curate(AvatarSettings settings, IEnumerable<string> wearablesId, CancellationToken ct = default);
    }
}