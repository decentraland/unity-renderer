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

        internal readonly string MISSING_CONTAINER_ERROR = "Container cannot be null";
        internal readonly string MISSING_EMOTE_ERROR = "Emote cannot be null";
        internal readonly string MISSING_BODYSHAPE_ERROR = "bodyShapeId cannot be null or empty";

        public async UniTask LoadEmote(GameObject container, WearableItem emote, string bodyShapeId, CancellationToken ct = default)
        {
            if (container == null)
                throw new NullReferenceException(MISSING_CONTAINER_ERROR);

            if (emote == null)
                throw new NullReferenceException(MISSING_EMOTE_ERROR);

            if (string.IsNullOrEmpty(bodyShapeId))
                throw new NullReferenceException(MISSING_BODYSHAPE_ERROR);

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