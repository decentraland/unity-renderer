using DCL;
using DCL.Controllers;
using NSubstitute;
using NUnit.Framework;

namespace AvatarAssets.Tests
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
    }
}
