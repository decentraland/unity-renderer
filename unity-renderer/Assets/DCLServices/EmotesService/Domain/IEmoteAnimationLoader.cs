using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCLServices.EmotesService.Domain;
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

        UniTask LoadEmote(GameObject targetContainer, WearableItem emote, string bodyShapeId, CancellationToken ct = default);
    }
}
