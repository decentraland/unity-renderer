using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Emotes
{
    public class EmoteAnimationsTrackerShould
    {
        private EmoteAnimationsTracker tracker;
        private DataStore_Emotes dataStore;
        private EmoteAnimationLoaderFactory loaderFactory;
        private IEmotesCatalogService emoteCatalog;
        private IWearablesCatalogService wearablesCatalogService;

        [SetUp]
        public void SetUp()
        {
            wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
            dataStore = new DataStore_Emotes();
            loaderFactory = Substitute.ForPartsOf<EmoteAnimationLoaderFactory>();
            loaderFactory.Get().Returns(Substitute.For<IEmoteAnimationLoader>());

            emoteCatalog = Substitute.For<IEmotesCatalogService>();
            emoteCatalog.GetEmbeddedEmotes().Returns(GetEmbeddedEmotesSO());

            tracker = new EmoteAnimationsTracker(dataStore, loaderFactory, emoteCatalog, wearablesCatalogService);
        }

        private async UniTask<EmbeddedEmotesSO> GetEmbeddedEmotesSO()
        {
            EmbeddedEmotesSO embeddedEmotes = ScriptableObject.CreateInstance<EmbeddedEmotesSO>();
            embeddedEmotes.emotes = new EmbeddedEmote[] { };
            return embeddedEmotes;
        }

        [UnityTest]
        public IEnumerator InitializeEmbeddedEmotesOnConstructor()
        {
            UniTask<EmbeddedEmotesSO>.Awaiter embeddedEmotesTask = GetEmbeddedEmotesSO().GetAwaiter();
            yield return new WaitUntil(() => embeddedEmotesTask.IsCompleted);
            EmbeddedEmotesSO embeddedEmotesSo = embeddedEmotesTask.GetResult();

            foreach (EmbeddedEmote emote in embeddedEmotesSo.emotes)
            {
                Assert.AreEqual(dataStore.animations[(WearableLiterals.BodyShapes.FEMALE, emote.id)]?.clip, emote.femaleAnimation);
                Assert.AreEqual(dataStore.animations[(WearableLiterals.BodyShapes.MALE, emote.id)]?.clip, emote.maleAnimation);
                Assert.IsTrue(tracker.loaders.ContainsKey((WearableLiterals.BodyShapes.MALE, emote.id)));
            }

            wearablesCatalogService.Received(1).EmbedWearables(Arg.Any<WearableItem[]>());
        }

        [Test]
        public void ReactToEquipEmotesIncreasingReference()
        {
            AnimationClip animClip = new AnimationClip();
            EmoteItem emote = new EmoteItem() { id = "emote0", data = new WearableItem.Data() };
            emoteCatalog.RequestEmoteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new UniTask<WearableItem>(emote));
            IEmoteAnimationLoader loader = Substitute.For<IEmoteAnimationLoader>();
            loader.loadedAnimationClip.Returns(animClip);
            loaderFactory.Get().Returns(loader);

            dataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.FEMALE, "emote0"));

            loaderFactory.Received(1).Get();
            emoteCatalog.Received(1).RequestEmoteAsync("emote0", Arg.Any<CancellationToken>());
            loader.Received(1).LoadEmote(tracker.animationsModelsContainer, emote, WearableLiterals.BodyShapes.FEMALE, Arg.Any<CancellationToken>());
            var animKey = (WearableLiterals.BodyShapes.FEMALE, "emote0");
            Assert.AreEqual(animClip, dataStore.animations[animKey]?.clip);
        }

        [Test]
        public void ReactToEquipEmotesIncreasingReferenceWithExistentLoader()
        {
            loaderFactory.ClearReceivedCalls();
            tracker.loaders.Add((WearableLiterals.BodyShapes.FEMALE, "emote0"), Substitute.For<IEmoteAnimationLoader>());

            dataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.FEMALE, "emote0"));

            loaderFactory.DidNotReceive().Get();
        }

        [Test]
        public void ReactToEquipEmotesReferenceSetTo0()
        {
            string bodyshapeId = WearableLiterals.BodyShapes.FEMALE;

            var tikAnim = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Scripts/MainScripts/DCL/Components/Avatar/Animations/Addressables/tik.anim");
            dataStore.animations.Add((bodyshapeId, "emote0"), new EmoteClipData(tikAnim));
            IEmoteAnimationLoader loader = Substitute.For<IEmoteAnimationLoader>();
            tracker.loaders.Add((bodyshapeId, "emote0"), loader);

            dataStore.emotesOnUse.SetRefCount((bodyshapeId, "emote0"), 0);

            Assert.IsFalse(tracker.loaders.ContainsKey((bodyshapeId, "emote0")));
            loader.Received().Dispose();
            Assert.IsFalse(dataStore.animations.ContainsKey((bodyshapeId, "emote0")));
        }

        [UnityTest]
        public IEnumerator HandleMultipleLoadEmoteCallsGracefully() =>
            UniTask.ToCoroutine(async () =>
            {
                // Arrange
                EmoteItem emote = new EmoteItem() { id = "emote0", data = new WearableItem.Data() };

                async UniTask<WearableItem> GetDelayedEmoteItem()
                {
                    await UniTask.DelayFrame(1);
                    return emote;
                }

                AnimationClip animClip = new AnimationClip();
                emoteCatalog.RequestEmoteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((x) => GetDelayedEmoteItem());
                IEmoteAnimationLoader loader = Substitute.For<IEmoteAnimationLoader>();
                loader.loadedAnimationClip.Returns(animClip);
                loaderFactory.Configure().Get().Returns(loader);

                // Act
                dataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.FEMALE, "emote0"));
                dataStore.emotesOnUse.IncreaseRefCount((WearableLiterals.BodyShapes.FEMALE, "emote0"));
                await UniTask.DelayFrame(2); // Give time for GetDelayedEmoteItem to be performed.

                // Assert
                loaderFactory.Received(1).Get();
                emoteCatalog.Received(1).RequestEmoteAsync("emote0", Arg.Any<CancellationToken>());
                loader.Received(1).LoadEmote(tracker.animationsModelsContainer, emote, WearableLiterals.BodyShapes.FEMALE, Arg.Any<CancellationToken>());
                var animKey = (WearableLiterals.BodyShapes.FEMALE, "emote0");
                Assert.AreEqual(animClip, dataStore.animations[animKey]?.clip);
            });
    }
}
