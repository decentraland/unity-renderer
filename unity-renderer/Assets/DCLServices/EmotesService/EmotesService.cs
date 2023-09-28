using AvatarAssets;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
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

        private readonly Dictionary<EmoteBodyId, IEmoteReference> embeddedEmotes = new ();
        private readonly Dictionary<EmoteBodyId, ExtendedEmote> extendedEmotes = new ();

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
            EmbeddedEmotesSO embedEmotes = await emotesCatalogService.GetEmbeddedEmotes();

            // early return for not configured emotesCatalogService substitutes in legacy test base
            if (embedEmotes == null) return;

            foreach (EmbeddedEmote embeddedEmote in embedEmotes.GetEmbeddedEmotes())
            {
                if (embeddedEmote.maleAnimation != null)
                    SetupEmbeddedClip(embeddedEmote, embeddedEmote.maleAnimation, MALE);

                if (embeddedEmote.femaleAnimation != null)
                    SetupEmbeddedClip(embeddedEmote, embeddedEmote.femaleAnimation, FEMALE);
            }

            foreach (ExtendedEmote embeddedEmote in embedEmotes.GetExtendedEmbeddedEmotes())
            {
                SetupEmbeddedExtendedEmote(embeddedEmote);
            }

            wearablesCatalogService.AddEmbeddedWearablesToCatalog(embedEmotes.GetAllEmotes());
        }

        private void SetupEmbeddedExtendedEmote(ExtendedEmote embeddedEmote)
        {
            extendedEmotes.Add(new EmoteBodyId(MALE, embeddedEmote.id), embeddedEmote);
            extendedEmotes.Add(new EmoteBodyId(FEMALE, embeddedEmote.id), embeddedEmote);
        }

        private void SetupEmbeddedClip(EmbeddedEmote embeddedEmote, AnimationClip clip, string urnPrefix)
        {
            clip.name = embeddedEmote.id;
            var clipData = new EmoteClipData(clip, embeddedEmote.emoteDataV0?.loop ?? false);
            embeddedEmotes.Add(new EmoteBodyId(urnPrefix, embeddedEmote.id), new EmbedEmoteReference(embeddedEmote, clipData));
        }

        public async UniTask<IEmoteReference> RequestEmote(EmoteBodyId emoteBodyId, CancellationToken cancellationToken = default)
        {
            if (embeddedEmotes.TryGetValue(emoteBodyId, out var value))
                return value;

            if (extendedEmotes.TryGetValue(emoteBodyId, out var extendedEmote))
            {
                IEmoteAnimationLoader loader = emoteAnimationLoaderFactory.Get();
                await loader.LoadLocalEmote(animationsModelsContainer, extendedEmote, cancellationToken);
                return new NftEmoteReference(extendedEmote, loader, extendedEmote.emoteDataV0?.loop ?? false);
            }

            try
            {
                var emote = await FetchEmote(emoteBodyId, cancellationToken);

                if (emote == null)
                {
                    Debug.LogError($"Unexpected null emote when requesting {emoteBodyId}");
                    return null;
                }

                // Loader disposal is being handled by the emote reference
                IEmoteAnimationLoader animationLoader = emoteAnimationLoaderFactory.Get();
                await animationLoader.LoadRemoteEmote(animationsModelsContainer, emote, emoteBodyId.BodyShapeId, cancellationToken);

                if (animationLoader.mainClip == null)
                    Debug.LogError("Emote animation failed to load for emote " + emote.id);

                bool loop = emote is EmoteItem newEmoteItem ? newEmoteItem.data.loop : emote.emoteDataV0?.loop ?? false;
                IEmoteReference emoteReference = new NftEmoteReference(emote, animationLoader, loop);
                return emoteReference;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private UniTask<WearableItem> FetchEmote(EmoteBodyId emoteBodyId, CancellationToken ct)
        {
            string emoteId = emoteBodyId.EmoteId;

            if (!SceneEmoteHelper.IsSceneEmote(emoteId))
                return emotesCatalogService.RequestEmoteAsync(emoteId, ct);

            if (!emoteId.StartsWith("urn"))
                return emotesCatalogService.RequestEmoteFromBuilderAsync(emoteId, ct);

            WearableItem emoteItem = SceneEmoteHelper.TryGetDataFromEmoteId(emoteId, out string emoteHash, out bool loop) ? new EmoteItem(emoteId, emoteBodyId.BodyShapeId, emoteHash, catalyst.contentUrl, loop) : null;

            return new UniTask<WearableItem>(emoteItem);
        }

        public void Dispose()
        {
            Object.Destroy(animationsModelsContainer);
        }
    }
}
