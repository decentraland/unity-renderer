using System;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationLoader : IEmoteAnimationLoader
    {
        private readonly IWearableRetriever retriever;
        public AnimationClip animation { get; internal set; }

        public EmoteAnimationLoader(IWearableRetriever retriever) { this.retriever = retriever; }

        public async UniTask LoadEmote(GameObject container, WearableItem emote, string bodyShapeId, CancellationToken ct = default)
        {
            if (container == null)
                throw new NullReferenceException("Container cannot be null");

            if (emote == null)
                throw new NullReferenceException("Emote cannot be null");

            if (string.IsNullOrEmpty(bodyShapeId))
                throw new NullReferenceException("bodyShapeId cannot be null or empty");

            ct.ThrowIfCancellationRequested();

            WearableItem.Representation representation = emote.GetRepresentation(bodyShapeId);
            if (representation == null)
            {
                throw new Exception($"No representation for {bodyShapeId} of emote: {emote.id}");
            }

            Rendereable rendereable = await retriever.Retrieve(container, emote.GetContentProvider(bodyShapeId), emote.baseUrlBundles, representation.mainFile, ct);

            animation = rendereable.container.GetComponentInChildren<Animation>()?.clip;

            //Setting animation name equal to emote id to avoid unity animation clip duplication on Animation.AddClip()
            animation.name = emote.id;
        }

        public void Dispose() { retriever?.Dispose(); }
    }
}