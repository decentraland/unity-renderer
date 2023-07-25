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
        IReadOnlyList<SkinnedMeshRenderer> originalVisibleRenderers { get; }
        List<Renderer> facialFeaturesRenderers { get; }
        Status status { get; }

        UniTask Load(BodyWearables bodyWearables, List<WearableItem> wearables, AvatarSettings settings, SkinnedMeshRenderer bonesRenderers = null, CancellationToken cancellationToken = default);
        Transform[] GetBones();
        bool IsValidForBodyShape(BodyWearables bodyWearables);
    }
}
