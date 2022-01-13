using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AvatarSystem
{
    public interface IFacialFeatureRetriever : IDisposable
    {
        UniTask<(Texture main, Texture mask)> Retrieve(WearableItem facialFeature, string bodyshapeId, CancellationToken ct = default);
    }
}