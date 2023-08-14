using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCLServices.WearablesCatalogService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Emotes
{
    public class EmoteAnimationsTracker : IDisposable
    {
        internal readonly DataStore_Emotes dataStore;
        internal readonly EmoteAnimationLoaderFactory emoteAnimationLoaderFactory;
        private readonly IEmotesCatalogService emotesCatalogService;
        private readonly IWearablesCatalogService wearablesCatalogService;
        private readonly ICatalyst catalyst;

        internal Dictionary<(string bodyshapeId, string emoteId), IEmoteAnimationLoader> loaders = new Dictionary<(string bodyshapeId, string emoteId), IEmoteAnimationLoader>();

        private CancellationTokenSource cts = new CancellationTokenSource();

        internal GameObject animationsModelsContainer;

        public EmoteAnimationsTracker(
            DataStore_Emotes dataStore,
            EmoteAnimationLoaderFactory emoteAnimationLoaderFactory,
            IEmotesCatalogService emotesCatalogService,
            IWearablesCatalogService wearablesCatalogService,
            ICatalyst catalyst)
        {
            animationsModelsContainer = new GameObject("_EmoteAnimationsHolder");
            animationsModelsContainer.transform.position = EnvironmentSettings.MORDOR;
            this.dataStore = dataStore;
            this.emoteAnimationLoaderFactory = emoteAnimationLoaderFactory;
            this.emotesCatalogService = emotesCatalogService;
            this.wearablesCatalogService = wearablesCatalogService;
            this.dataStore.animations.Clear();
            this.catalyst = catalyst;

            AsyncInitialization();
        }

        private async UniTaskVoid AsyncInitialization()
        {
            //To avoid circular references in assemblies we hardcode this here instead of using WearableLiterals
            //Embedded Emotes are only temporary until they can be retrieved from the content server
            const string FEMALE = "urn:decentraland:off-chain:base-avatars:BaseFemale";
            const string MALE = "urn:decentraland:off-chain:base-avatars:BaseMale";

            EmbeddedEmotesSO embeddedEmotes = await emotesCatalogService.GetEmbeddedEmotes();

            foreach (EmbeddedEmote embeddedEmote in embeddedEmotes.emotes)
            {
                if (embeddedEmote.maleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.maleAnimation.name = embeddedEmote.id;
                    dataStore.emotesOnUse.SetRefCount((MALE, embeddedEmote.id), 5000);
                    var clipData = new EmoteClipData(embeddedEmote.maleAnimation, embeddedEmote.emoteDataV0);
                    dataStore.animations.Add((MALE, embeddedEmote.id), clipData);
                    loaders.Add((MALE, embeddedEmote.id), emoteAnimationLoaderFactory.Get());
                }

                if (embeddedEmote.femaleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.femaleAnimation.name = embeddedEmote.id;
                    dataStore.emotesOnUse.SetRefCount((FEMALE, embeddedEmote.id), 5000);
                    var emoteClipData = new EmoteClipData(embeddedEmote.femaleAnimation, embeddedEmote.emoteDataV0);
                    dataStore.animations.Add((FEMALE, embeddedEmote.id), emoteClipData);
                    loaders.Add((FEMALE, embeddedEmote.id), emoteAnimationLoaderFactory.Get());
                }
            }

            wearablesCatalogService.EmbedWearables(embeddedEmotes.emotes);
            InitializeEmotes(this.dataStore.emotesOnUse.GetAllRefCounts());
            this.dataStore.emotesOnUse.OnRefCountUpdated += OnRefCountUpdated;
        }

        private void OnRefCountUpdated((string bodyshapeId, string emoteId) value, int refCount)
        {
            if (refCount > 0)
                LoadEmote(value.bodyshapeId, value.emoteId, cts.Token);
            else
                UnloadEmote(value.bodyshapeId, value.emoteId);
        }

        private void InitializeEmotes(IEnumerable<KeyValuePair<(string bodyshapeId, string emoteId), int>> refCounts)
        {
            foreach (KeyValuePair<(string bodyshapeId, string emoteId), int> keyValuePair in refCounts)
            {
                LoadEmote(keyValuePair.Key.bodyshapeId, keyValuePair.Key.emoteId, cts.Token);
            }
        }

        private async UniTaskVoid LoadEmote(string bodyShapeId, string emoteId, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (loaders.ContainsKey((bodyShapeId, emoteId)))
                return;

            try
            {
                loaders.Add((bodyShapeId, emoteId), null);
                var emote = await RequestEmote(bodyShapeId, emoteId, ct);

                if (emote == null)
                    return;

                IEmoteAnimationLoader animationLoader = emoteAnimationLoaderFactory.Get();
                loaders[(bodyShapeId, emoteId)] = animationLoader;
                await animationLoader.LoadEmote(animationsModelsContainer, emote, bodyShapeId, ct);

                EmoteClipData emoteClipData;

                if (emote is EmoteItem newEmoteItem)
                    emoteClipData = new EmoteClipData(animationLoader.loadedAnimationClip, newEmoteItem.data.loop);
                else
                    emoteClipData = new EmoteClipData(animationLoader.loadedAnimationClip, emote.emoteDataV0);

                dataStore.animations.Add((bodyShapeId, emoteId), emoteClipData);

                if (animationLoader.loadedAnimationClip == null)
                {
                    Debug.LogError("Emote animation failed to load for emote " + emote.id);
                }
            }
            catch (Exception e)
            {
                loaders.Remove((bodyShapeId, emoteId));
                dataStore.animations.Remove((bodyShapeId, emoteId));
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
        }

        private void UnloadEmote(string bodyShapeId, string emoteId)
        {
            dataStore.animations.Remove((bodyShapeId, emoteId));

            if (!loaders.TryGetValue((bodyShapeId, emoteId), out IEmoteAnimationLoader loader))
                return;

            loaders.Remove((bodyShapeId, emoteId));
            loader?.Dispose();
        }

        public void Dispose()
        {
            dataStore.emotesOnUse.OnRefCountUpdated -= OnRefCountUpdated;
            (string bodyshapeId, string emoteId)[] keys = loaders.Keys.ToArray();

            foreach ((string bodyshapeId, string emoteId) in keys)
            {
                UnloadEmote(bodyshapeId, emoteId);
            }

            loaders.Clear();
            cts.Cancel();
            Object.Destroy(animationsModelsContainer);
        }

        private UniTask<WearableItem> RequestEmote(string bodyShapeId, string emoteId, CancellationToken ct)
        {
            if (SceneEmoteHelper.IsSceneEmote(emoteId))
            {
                return new UniTask<WearableItem>(GenerateSceneEmoteItem(bodyShapeId, emoteId, catalyst.contentUrl));
            }

            return emotesCatalogService.RequestEmoteAsync(emoteId, ct);
        }

        private static WearableItem GenerateSceneEmoteItem(string bodyShapeId, string emoteId, string contentLambdaUrl)
        {
            if (!SceneEmoteHelper.TryGetDataFromEmoteId(emoteId, out string emoteHash, out bool loop))
            {
                return null;
            }

            return new EmoteItem()
            {
                data = new WearableItem.Data()
                {
                    representations = new[]
                    {
                        new WearableItem.Representation()
                        {
                            bodyShapes = new[] { bodyShapeId },
                            contents = new[]
                            {
                                new WearableItem.MappingPair()
                                {
                                    hash = emoteHash, key = emoteHash
                                }
                            },
                            mainFile = emoteHash,
                        }
                    },
                    loop = loop
                },
                emoteDataV0 = new EmoteDataV0() { loop = loop },
                id = emoteId,
                baseUrl = $"{contentLambdaUrl}contents/"
            };
        }
    }
}
