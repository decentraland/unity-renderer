using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;

namespace AvatarSystem
{
    public interface IWearableRetriever : IDisposable
    {
        Rendereable rendereable { get; }
        UniTask<Rendereable> Retrieve(GameObject container, WearableItem wearable, string bodyShapeId, CancellationToken ct = default);
    }
}
