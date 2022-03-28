using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Emotes
{
    public interface IEmoteAnimationLoader : IDisposable
    {
        AnimationClip animation { get; }
        UniTask LoadEmote(GameObject container, WearableItem emote, string bodyShapeId, CancellationToken ct = default);
    }
}