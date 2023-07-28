using System;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationLoader : IEmoteAnimationLoader
    {
        private readonly IWearableRetriever retriever;
        public AnimationClip loadedAnimationClip { get; internal set; }

        public EmoteAnimationLoader(IWearableRetriever retriever)
        {
            this.retriever = retriever;
        }

        public async UniTask LoadEmote(GameObject container, WearableItem emote, string bodyShapeId, CancellationToken ct = default)
        {
            if (container == null)
                throw new NullReferenceException("Container cannot be null");

            if (emote == null)
                throw new NullReferenceException("Emote cannot be null");

            if (string.IsNullOrEmpty(bodyShapeId))
                throw new NullReferenceException("bodyShapeId cannot be null or empty");

            ct.ThrowIfCancellationRequested();

            Rendereable rendereable = await retriever.Retrieve(container, emote, bodyShapeId, ct);

            AnimationClip animationClip = null;
            var animation = rendereable.container.GetComponentInChildren<Animation>();

            if (animation != null)
            {
                animation.enabled = false;
                animationClip = animation.clip;
            }
            else
            {
                animationClip = rendereable.animationClips.First();
            }

            if (animationClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emote.id, rendereable.container);
                return;
            }

            //Setting animation name equal to emote id to avoid unity animation clip duplication on Animation.AddClip()
            this.loadedAnimationClip = animationClip;
            animationClip.name = emote.id;

        }

        public void Dispose()
        {
            retriever?.Dispose();
        }
    }
}
