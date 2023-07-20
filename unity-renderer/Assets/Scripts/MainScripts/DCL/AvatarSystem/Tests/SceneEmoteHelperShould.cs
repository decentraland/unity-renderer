using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Emotes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace Test.AvatarSystem
{
    public class SceneEmoteHelperShould
    {
        [Test]
        public void TryGenerateEmoteId()
        {
            const string HASH = "00172";
            const string FILE = "test";
            IParcelScene scene = Substitute.For<IParcelScene>();

            var contentProvider = new ContentProvider()
            {
                contents =
                {
                    new ContentServerUtils.MappingPair() { file = FILE, hash = HASH }
                }
            };

            contentProvider.BakeHashes();
            scene.contentProvider.Returns(contentProvider);

            string emoteIdResult;
            Assert.IsTrue(SceneEmoteHelper.TryGenerateEmoteId(scene, FILE, true, out emoteIdResult));
            Assert.AreEqual($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}{HASH}-true", emoteIdResult);

            Assert.IsTrue(SceneEmoteHelper.TryGenerateEmoteId(scene, FILE, false, out emoteIdResult));
            Assert.AreEqual($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}{HASH}-false", emoteIdResult);
        }

        [Test]
        public void TryGetDataFromEmoteId()
        {
            const string HASH = "00172";
            Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}{HASH}-true", out string hash, out bool loop));
            Assert.AreEqual(HASH, hash);
            Assert.IsTrue(loop);

            Assert.IsTrue(SceneEmoteHelper.TryGetDataFromEmoteId($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}{HASH}-false", out hash, out loop));
            Assert.AreEqual(HASH, hash);
            Assert.IsFalse(loop);

            Assert.IsFalse(SceneEmoteHelper.TryGetDataFromEmoteId($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}{HASH}", out hash, out loop));
        }

        [Test]
        public void IsSceneEmote()
        {
            Assert.IsTrue(SceneEmoteHelper.IsSceneEmote($"{SceneEmoteHelper.SCENE_EMOTE_PREFIX}0-false"));
            Assert.IsFalse(SceneEmoteHelper.IsSceneEmote($"otherstring"));
        }

        [UnityTest]
        public IEnumerator RequestLoadSceneEmote_Success()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesOnUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
            BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();
            HashSet<(string bodyshapeId, string emoteId)> currentScenePendingSceneEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
            HashSet<(string bodyshapeId, string emoteId)> currentSceneEquippedEmotes = new HashSet<(string bodyshapeId, string emoteId)>();

            yield return UniTask.ToCoroutine(async () =>
            {
                emotesOnUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip(), false);

                await SceneEmoteHelper.RequestLoadSceneEmote(
                    BODY_SHAPE,
                    EMOTE_ID,
                    animations,
                    emotesOnUse,
                    currentScenePendingSceneEmotes,
                    currentSceneEquippedEmotes,
                    default
                );

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsTrue(currentSceneEquippedEmotes.Contains(emoteData));
                Assert.IsTrue(animations.ContainsKey(emoteData));
            });
        }

        [UnityTest]
        public IEnumerator RequestLoadSceneEmote_Cancelled()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesOnUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
            BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();
            HashSet<(string bodyshapeId, string emoteId)> currentScenePendingSceneEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
            HashSet<(string bodyshapeId, string emoteId)> currentSceneEquippedEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            yield return UniTask.ToCoroutine(async () =>
            {
                try
                {
                    // schedule cancellation after request starts
                    UniTask.Create(async () =>
                    {
                        await UniTask.Yield();
                        cancellationTokenSource.Cancel();
                    });

                    await SceneEmoteHelper.RequestLoadSceneEmote(
                        BODY_SHAPE,
                        EMOTE_ID,
                        animations,
                        emotesOnUse,
                        currentScenePendingSceneEmotes,
                        currentSceneEquippedEmotes,
                        cancellationTokenSource.Token
                    );
                }
                catch (OperationCanceledException _)
                {
                    // Ignored
                }

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsFalse(currentSceneEquippedEmotes.Contains(emoteData));
                Assert.IsFalse(currentScenePendingSceneEmotes.Contains(emoteData));
                Assert.IsFalse(animations.ContainsKey(emoteData));
                Assert.AreEqual(0, emotesOnUse.GetRefCount(emoteData));
            });
        }

        [UnityTest]
        public IEnumerator RequestLoadSceneEmote_LoadedTwice()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesOnUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
            BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();
            HashSet<(string bodyshapeId, string emoteId)> currentScenePendingSceneEmotes = new HashSet<(string bodyshapeId, string emoteId)>();
            HashSet<(string bodyshapeId, string emoteId)> currentSceneEquippedEmotes = new HashSet<(string bodyshapeId, string emoteId)>();

            yield return UniTask.ToCoroutine(async () =>
            {
                emotesOnUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip(), false);

                var load1 = SceneEmoteHelper.RequestLoadSceneEmote(
                    BODY_SHAPE,
                    EMOTE_ID,
                    animations,
                    emotesOnUse,
                    currentScenePendingSceneEmotes,
                    currentSceneEquippedEmotes,
                    default
                );

                var load2 = SceneEmoteHelper.RequestLoadSceneEmote(
                    BODY_SHAPE,
                    EMOTE_ID,
                    animations,
                    emotesOnUse,
                    currentScenePendingSceneEmotes,
                    currentSceneEquippedEmotes,
                    default
                );

                await UniTask.WhenAll(load1, load2);

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsTrue(currentSceneEquippedEmotes.Contains(emoteData));
                Assert.IsFalse(currentScenePendingSceneEmotes.Contains(emoteData));
                Assert.IsTrue(animations.ContainsKey(emoteData));
                Assert.AreEqual(1, emotesOnUse.GetRefCount(emoteData));
            });
        }
    }
}
