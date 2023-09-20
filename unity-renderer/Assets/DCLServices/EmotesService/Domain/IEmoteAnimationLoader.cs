using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Emotes
{
    public interface IEmoteAnimationLoader : IDisposable
    {
        AnimationClip mainClip { get; }
        GameObject container { get; }
        AudioSource audioSource { get; }
        bool IsSequential { get; }
        AnimationSequence GetSequence();
        UniTask LoadRemoteEmote(GameObject targetContainer, WearableItem emote, string bodyShapeId, CancellationToken ct = default);
        UniTask LoadLocalEmote(GameObject targetContainer, ExtendedEmote embeddedEmote, CancellationToken ct = default);
    }
}
