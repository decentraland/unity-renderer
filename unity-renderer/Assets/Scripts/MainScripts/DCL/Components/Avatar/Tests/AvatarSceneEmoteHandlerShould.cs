using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AvatarSceneEmoteHandlerShould
    {
        private BaseRefCountedCollection<(string bodyshapeId, string emoteId)> emotesOnUse;
        private BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animations;
        private AvatarSceneEmoteHandler handler;
        private IAvatar avatar;

        [SetUp]
        public void SetUp()
        {
            emotesOnUse = new BaseRefCountedCollection<(string bodyshapeId, string emoteId)>();
            animations = new BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData>();
            avatar = Substitute.For<IAvatar>();
            handler = new AvatarSceneEmoteHandler(avatar, animations, emotesOnUse);
        }

        [UnityTest]
        public IEnumerator LoadAndPlayEmote_Success()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            yield return UniTask.ToCoroutine(async () =>
            {
                emotesOnUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip());

                await handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsTrue(animations.ContainsKey(emoteData));
                Assert.AreEqual(1, emotesOnUse.GetRefCount(emoteData));
                avatar.Received(1).EquipEmote(EMOTE_ID, animations[emoteData]);
                avatar.Received(1).PlayEmote(EMOTE_ID, 0);
            });
        }

        [UnityTest]
        public IEnumerator LoadAndPlayEmote_Cancelled()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            yield return UniTask.ToCoroutine(async () =>
            {
                var task = handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);
                handler.cancellationTokenSource.Cancel();
                await task;

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsFalse(animations.ContainsKey(emoteData));
                Assert.AreEqual(0, emotesOnUse.GetRefCount(emoteData));
                avatar.DidNotReceive().EquipEmote(Arg.Any<string>(), Arg.Any<EmoteClipData>());
                avatar.DidNotReceive().PlayEmote(Arg.Any<string>(), Arg.Any<long>());
            });
        }

        [UnityTest]
        public IEnumerator LoadAndPlayEmote_LoadTwice()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID1 = "emoteId";
            const string EMOTE_ID2 = "emoteId2";

            yield return UniTask.ToCoroutine(async () =>
            {
                BaseDictionary<(string bodyshapeId, string emoteId), EmoteClipData> animationsToAdd = new ();
                emotesOnUse.OnRefCountUpdated += (tuple, i) => animationsToAdd[tuple] = new EmoteClipData(new AnimationClip());

                handler.SetExpressionLamportTimestamp(0);
                var load1 = handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID1);
                handler.SetExpressionLamportTimestamp(1);
                var load2 = handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID2);

                foreach (var toAdd in animationsToAdd)
                {
                    animations[toAdd.Key] = toAdd.Value;
                }

                await UniTask.WhenAll(load1, load2);

                avatar.DidNotReceive().EquipEmote(EMOTE_ID1, animations[(bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID1)]);
                avatar.DidNotReceive().PlayEmote(EMOTE_ID1, 0);
                avatar.Received(1).EquipEmote(EMOTE_ID2, animations[(bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID2)]);
                avatar.Received(1).PlayEmote(EMOTE_ID2, 1);
            });
        }

        [UnityTest]
        public IEnumerator CleanUp()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            yield return UniTask.ToCoroutine(async () =>
            {
                emotesOnUse.OnRefCountUpdated += (tuple, i) => animations[tuple] = new EmoteClipData(new AnimationClip());

                await handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID);

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.IsTrue(animations.ContainsKey(emoteData));
                Assert.AreEqual(1, emotesOnUse.GetRefCount(emoteData));
                avatar.Received(1).EquipEmote(EMOTE_ID, animations[emoteData]);
                avatar.Received(1).PlayEmote(EMOTE_ID, 0);

                handler.CleanUp();
                Assert.AreEqual(0, emotesOnUse.GetRefCount(emoteData));
            });
        }

        [UnityTest]
        public IEnumerator CleanUp_WhileLoadingEmote()
        {
            const string BODY_SHAPE = "nogender";
            const string EMOTE_ID = "emoteId";

            yield return UniTask.ToCoroutine(async () =>
            {
                handler.LoadAndPlayEmote(BODY_SHAPE, EMOTE_ID).Forget();

                var emoteData = (bodyshapeId: BODY_SHAPE, emoteId: EMOTE_ID);
                Assert.AreEqual(1, emotesOnUse.GetRefCount(emoteData));

                handler.CleanUp();
                Assert.AreEqual(0, emotesOnUse.GetRefCount(emoteData));
            });
        }
    }
}
