using System.Collections;
using DCL;
using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class VideoPlayerHandlerShould
    {
        private Func<IVideoPluginWrapper> originalVideoPluginBuilder;
        private VideoPlayerHandler videoPlayerHandler;
        private IInternalECSComponent<InternalVideoPlayer> internalVideoPlayerComponent;
        private ICatalyst catalyst;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;

        [SetUp]
        public void SetUp()
        {
            IVideoPluginWrapper pluginWrapper = new VideoPluginWrapper_Mock();
            originalVideoPluginBuilder = DCLVideoTexture.videoPluginWrapperBuilder;
            DCLVideoTexture.videoPluginWrapperBuilder = () => pluginWrapper;

            internalVideoPlayerComponent = Substitute.For<IInternalECSComponent<InternalVideoPlayer>>();

            videoPlayerHandler = new VideoPlayerHandler(internalVideoPlayerComponent);
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            Environment.Setup(ServiceLocatorFactory.CreateDefault());
        }

        [TearDown]
        public void TearDown()
        {
            DCLVideoTexture.videoPluginWrapperBuilder = originalVideoPluginBuilder;
            testUtils.Dispose();
            AssetPromiseKeeper_Material.i.Cleanup();
            AssetPromiseKeeper_Texture.i.Cleanup();
            Environment.Dispose();
        }

        [UnityTest]
        public IEnumerator TryToStartVideo()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4",
                Playing = true,
            };

            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, true);
        }

        [UnityTest]
        public IEnumerator VideoDefaultValues()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4"
            };

            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, false);
            Assert.AreEqual(videoPlayerHandler.videoPlayer.volume, 1.0f);
        }

        [UnityTest]
        public IEnumerator VideoUpdateOnRuntime()
        {
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, new PBVideoPlayer()
            {
                Src = "video.mp4"
            });

            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, false);
            Assert.AreEqual(videoPlayerHandler.videoPlayer.volume, 1.0f);

            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            videoPlayerHandler.OnComponentModelUpdated(scene, entity, new PBVideoPlayer()
            {
                Src = "other-video.mp4",
                Playing = true,
                Volume = 0.5f,
            });

            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(videoPlayerHandler.videoPlayer.url, "other-video.mp4");
            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, true);
            Assert.AreEqual(videoPlayerHandler.videoPlayer.volume, 0.5f);
        }
    }
}
