using System;
using System.Collections.Generic;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;

public interface IAvatarCurator : IDisposable
{
    UniTask<(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables)> Curate(AvatarSettings settings, IEnumerable<string> wearablesId, CancellationToken ct = default);
}