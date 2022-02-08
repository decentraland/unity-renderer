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
        Transform[] GetBones();
    }
}