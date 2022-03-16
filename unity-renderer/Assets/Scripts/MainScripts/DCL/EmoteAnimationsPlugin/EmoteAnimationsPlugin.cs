using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Emotes
{
    public class EmoteAnimationsPlugin : IPlugin
    {
        internal readonly DataStore_Emotes dataStore;
        internal readonly EmoteAnimationLoaderFactory emoteAnimationLoaderFactory;
        internal readonly IWearableItemResolver wearableItemResolver;

        internal Dictionary<(string bodyshapeId, string emoteId), IEmoteAnimationLoader> loaders = new Dictionary<(string bodyshapeId, string emoteId), IEmoteAnimationLoader>();

        private CancellationTokenSource cts = new CancellationTokenSource();

        internal GameObject animationsModelsContainer;

        public EmoteAnimationsPlugin(DataStore_Emotes dataStore, EmoteAnimationLoaderFactory emoteAnimationLoaderFactory, IWearableItemResolver wearableItemResolver)
        {
            animationsModelsContainer = new GameObject("_EmoteAnimationsHolder");
            this.dataStore = dataStore;
            this.emoteAnimationLoaderFactory = emoteAnimationLoaderFactory;
            this.wearableItemResolver = wearableItemResolver;
            this.dataStore.animations.Clear();
            this.dataStore.emotesOnUse.OnRefCountUpdated += OnRefCountUpdated;

            InitializeEmbeddedEmotes();
            InitializeEmotes(this.dataStore.emotesOnUse.GetAllRefCounts());
        }

        private void InitializeEmbeddedEmotes()
        {
            //To avoid circular references in assemblies we hardcode this here instead of using WearableLiterals
            //Embedded Emotes are only temporary until they can be retrieved from the content server
            const string FEMALE = "urn:decentraland:off-chain:base-avatars:BaseFemale";
            const string MALE = "urn:decentraland:off-chain:base-avatars:BaseMale";

            EmbeddedEmotesSO embeddedEmotes = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");

            foreach (EmbeddedEmote embeddedEmote in embeddedEmotes.emotes)
            {
                if (embeddedEmote.maleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.maleAnimation.name = embeddedEmote.id;
                    dataStore.animations.Add((MALE, embeddedEmote.id), embeddedEmote.maleAnimation);
                    loaders.Add((MALE, embeddedEmote.id), emoteAnimationLoaderFactory.Get());
                }

                if (embeddedEmote.femaleAnimation != null)
                {
                    //We match the animation id with its name due to performance reasons
                    //Unity's Animation uses the name to play the clips.
                    embeddedEmote.femaleAnimation.name = embeddedEmote.id;
                    dataStore.animations.Add((FEMALE, embeddedEmote.id), embeddedEmote.femaleAnimation);
                    loaders.Add((FEMALE, embeddedEmote.id), emoteAnimationLoaderFactory.Get());
                }
            }
            CatalogController.i.EmbedWearables(embeddedEmotes.emotes);
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
                WearableItem emote = await wearableItemResolver.Resolve(emoteId, ct);

                IEmoteAnimationLoader animationLoader = emoteAnimationLoaderFactory.Get();
                loaders.Add((bodyShapeId, emoteId), animationLoader);
                await animationLoader.LoadEmote(animationsModelsContainer, emote, bodyShapeId, ct);

                dataStore.animations.Add((bodyShapeId, emoteId), animationLoader.animation);
            }
            catch
            {
                loaders.Remove((bodyShapeId, emoteId));
                throw;
            }
        }

        private void UnloadEmote(string bodyShapeId, string emoteId)
        {
            if (!loaders.TryGetValue((bodyShapeId, emoteId), out IEmoteAnimationLoader loader))
                return;

            dataStore.animations.Remove((bodyShapeId, emoteId));
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
    }
}