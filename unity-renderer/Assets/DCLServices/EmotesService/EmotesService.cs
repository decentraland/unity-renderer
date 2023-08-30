using AvatarAssets;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Emotes
{
    public class EmotesService : IEmotesService
    {
        //To avoid circular references in assemblies we hardcode this here instead of using WearableLiterals
        //Embedded Emotes are only temporary until they can be retrieved from the content server
        private const string FEMALE = "urn:decentraland:off-chain:base-avatars:BaseFemale";
        private const string MALE = "urn:decentraland:off-chain:base-avatars:BaseMale";

        private readonly EmoteAnimationLoaderFactory emoteAnimationLoaderFactory;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly ICatalyst catalyst;
        private GameObject animationsModelsContainer;

        private readonly Dictionary<(string, string), IEmoteReference> embeddedEmotes = new ();

        public EmotesService(
            EmoteAnimationLoaderFactory emoteAnimationLoaderFactory,
            IEmotesCatalogService emotesCatalogService,
            IWearablesCatalogService wearablesCatalogService,
            ICatalyst catalyst)
        {
            this.emoteAnimationLoaderFactory = emoteAnimationLoaderFactory;
            this.emotesCatalogService = emotesCatalogService;
            this.wearablesCatalogService = wearablesCatalogService;
            this.catalyst = catalyst;
        }

        public void Initialize()
        {
            animationsModelsContainer = new GameObject("_EmotesAnimationContainer")
            {
                transform =
                {
                    position = EnvironmentSettings.MORDOR,
                },
            };
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            EmbeddedEmotesSO embeddedEmotes = await emotesCatalogService.GetEmbeddedEmotes();

            // early return for not configured emotesCatalogService substitutes in legacy test base
            if (embeddedEmotes == null) return;

            foreach (EmbeddedEmote embeddedEmote in embeddedEmotes.emotes)
            {
                if (embeddedEmote.maleAnimation != null)
                    SetupEmbeddedClip(embeddedEmote, embeddedEmote.maleAnimation, MALE);

                if (embeddedEmote.femaleAnimation != null)
                    SetupEmbeddedClip(embeddedEmote, embeddedEmote.femaleAnimation, FEMALE);
            }

            wearablesCatalogService.AddEmbeddedWearablesToCatalog(embeddedEmotes.emotes);
        }

        private void SetupEmbeddedClip(EmbeddedEmote embeddedEmote, AnimationClip clip, string urnPrefix)
        {
            clip.name = embeddedEmote.id;
            var clipData = new EmoteClipData(clip, embeddedEmote.emoteDataV0?.loop ?? false);
            embeddedEmotes.Add((urnPrefix, embeddedEmote.id), new EmbedEmoteReference(embeddedEmote, clipData));
        }

        public async UniTask<IEmoteReference> RequestEmote(string bodyShapeId, string emoteId, CancellationToken cancellationToken = default)
        {
            if (embeddedEmotes.ContainsKey((bodyShapeId, emoteId)))
                return embeddedEmotes[(bodyShapeId, emoteId)];

            try
            {
                var emote = await FetchEmote(bodyShapeId, emoteId, cancellationToken);

                if (emote == null)
                {
                    Debug.LogError($"Unexpected null emote when requesting {bodyShapeId} {emoteId}");
                    return null;
                }

                IEmoteAnimationLoader animationLoader = emoteAnimationLoaderFactory.Get();
                await animationLoader.LoadEmote(animationsModelsContainer, emote, bodyShapeId, cancellationToken);

                if (animationLoader.mainClip == null)
                    Debug.LogError("Emote animation failed to load for emote " + emote.id);

                bool loop = emote is EmoteItem newEmoteItem ? newEmoteItem.data.loop : emote.emoteDataV0?.loop ?? false;
                IEmoteReference emoteReference = new NftEmoteReference(emote, animationLoader, loop);
                return emoteReference;
            }
            catch (Exception e)
            {
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        private UniTask<WearableItem> FetchEmote(string bodyShapeId, string emoteId, CancellationToken ct)
        {
            if (!SceneEmoteHelper.IsSceneEmote(emoteId))
                return emotesCatalogService.RequestEmoteAsync(emoteId, ct);

            WearableItem emoteItem = SceneEmoteHelper.TryGetDataFromEmoteId(emoteId, out string emoteHash, out bool loop) ? new EmoteItem(bodyShapeId, emoteId, emoteHash, catalyst.contentUrl, loop) : null;

            return new UniTask<WearableItem>(emoteItem);
        }

        public void Dispose()
        {
            Object.Destroy(animationsModelsContainer);
        }
    }
}
