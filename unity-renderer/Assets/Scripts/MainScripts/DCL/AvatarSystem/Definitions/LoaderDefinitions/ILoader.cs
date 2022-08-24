using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface ILoader : IDisposable
    {
        public enum Status
        {
            Idle,
            Loading,
            Succeeded,
            Failed_Minor,
            Failed_Major
        }

        GameObject bodyshapeContainer { get; }
        SkinnedMeshRenderer combinedRenderer { get; }
        List<Renderer> facialFeaturesRenderers { get; }
        Status status { get; }

        UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, CancellationToken ct = default);
        UniTask Load(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth, List<WearableItem> wearables, AvatarSettings settings, SkinnedMeshRenderer bonesRenderers, CancellationToken ct = default);
        Transform[] GetBones();
        bool IsValidForBodyShape(WearableItem bodyshape, WearableItem eyes, WearableItem eyebrows, WearableItem mouth);
    }
}