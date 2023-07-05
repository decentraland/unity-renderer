using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace AvatarSystem
{
    public interface IAvatarCurator : IDisposable
    {
        UniTask<(BodyWearables bodyWearables, List<WearableItem> wearables, List<WearableItem> emotes)> Curate(AvatarSettings settings, IEnumerable<string> wearablesId, IEnumerable<string> emoteIds, CancellationToken ct = default);
    }
}
