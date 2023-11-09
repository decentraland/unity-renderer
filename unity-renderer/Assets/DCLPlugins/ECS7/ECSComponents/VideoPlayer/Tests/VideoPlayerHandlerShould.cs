using DCL;
using DCL.Components;
using DCL.Components.Video.Plugin;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.SettingsCommon;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using AudioSettings = DCL.SettingsCommon.AudioSettings;
using Environment = DCL.Environment;
using VideoState = DCL.Components.Video.Plugin.VideoState;

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
        private DataStore_LoadingScreen loadingScreenDataStore;
        private DataStore_VirtualAudioMixer virtualAudioMixerDatastore;
        private ISettingsRepository<AudioSettings> audioSettingsRepository;
        private IntVariable currentPlayerSceneNumber;

        [SetUp]
        public void SetUp()
        {
            loadingScreenDataStore = new DataStore_LoadingScreen();
            IVideoPluginWrapper pluginWrapper = new VideoPluginWrapper_Mock();
            originalVideoPluginBuilder = DCLVideoTexture.videoPluginWrapperBuilder;
            DCLVideoTexture.videoPluginWrapperBuilder = () => pluginWrapper;

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            internalVideoPlayerComponent = internalComponents.videoPlayerComponent;

            virtualAudioMixerDatastore = new DataStore_VirtualAudioMixer();
            virtualAudioMixerDatastore.sceneSFXVolume.Set(1);

            audioSettingsRepository = Substitute.For<ISettingsRepository<AudioSettings>>();

            audioSettingsRepository.Data.Returns(new AudioSettings
            {
                masterVolume = 1,
                sceneSFXVolume = 1,
            });

            currentPlayerSceneNumber = ScriptableObject.CreateInstance<IntVariable>();
            currentPlayerSceneNumber.Set(666);

            videoPlayerHandler = new VideoPlayerHandler(
                internalVideoPlayerComponent,
                loadingScreenDataStore.decoupledLoadingHUD,
                audioSettingsRepository,
                virtualAudioMixerDatastore,
                currentPlayerSceneNumber);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1000);

            scene.contentProvider.contents.Add(new ContentServerUtils.MappingPair() { file = "video.mp4", hash = "video.mp4" });
            scene.contentProvider.contents.Add(new ContentServerUtils.MappingPair() { file = "other-video.mp4", hash = "other-video.mp4" });
            scene.contentProvider.BakeHashes();

            Environment.Setup(ServiceLocatorFactory.CreateDefault());

            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(false);
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
            videoPlayerHandler.isRendererActive = true;
            videoPlayerHandler.hadUserInteraction = true;
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4",
                Playing = true
            };

            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(true, videoPlayerHandler.videoPlayer.playing);
        }

        [UnityTest]
        public IEnumerator VideoDefaultValues()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4"
            };

            videoPlayerHandler.OnComponentCreated(scene, entity);
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, false);
            Assert.AreEqual(videoPlayerHandler.videoPlayer.volume, 1.0f);
        }

        [UnityTest]
        public IEnumerator VideoUpdateOnRuntime()
        {
            videoPlayerHandler.isRendererActive = true;
            videoPlayerHandler.hadUserInteraction = true;
            videoPlayerHandler.OnComponentCreated(scene, entity);
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

        [UnityTest]
        public IEnumerator VolumeUpdatesWhenSettingsAreChanged()
        {
            PBVideoPlayer model = new PBVideoPlayer
            {
                Src = "video.mp4",
            };

            videoPlayerHandler.OnComponentCreated(scene, entity);
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(videoPlayerHandler.videoPlayer.playing, false);
            Assert.AreEqual(videoPlayerHandler.videoPlayer.volume, 1.0f);

            audioSettingsRepository.OnChanged += Raise.Event<Action<AudioSettings>>(new AudioSettings
            {
                masterVolume = 0.5f,
                sceneSFXVolume = 1,
            });

            Assert.AreEqual(0.5f, videoPlayerHandler.videoPlayer.volume);

            audioSettingsRepository.OnChanged += Raise.Event<Action<AudioSettings>>(new AudioSettings
            {
                masterVolume = 0.25f,
                sceneSFXVolume = 1,
            });

            Assert.AreEqual(0.25f, videoPlayerHandler.videoPlayer.volume);

            audioSettingsRepository.OnChanged += Raise.Event<Action<AudioSettings>>(new AudioSettings
            {
                masterVolume = 1,
                sceneSFXVolume = 1,
            });
            virtualAudioMixerDatastore.sceneSFXVolume.Set(0.1f, true);

            Assert.AreEqual(0.1f, videoPlayerHandler.videoPlayer.volume);
        }

        [Test]
        public void CreateInternalComponentCorrectly()
        {
            Assert.IsNull(internalVideoPlayerComponent.GetFor(scene, entity));

            videoPlayerHandler.OnComponentCreated(scene, entity);
            videoPlayerHandler.hadUserInteraction = true;
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, new PBVideoPlayer()
            {
                Src = "other-video.mp4",
                Playing = true,
                Volume = 0.5f,
            });

            Assert.NotNull(internalVideoPlayerComponent.GetFor(scene, entity));

            // The internal component is removed with a default model flagged as removed to
            // be able to remove video events in ECSVideoPlayerSystem
            videoPlayerHandler.OnComponentRemoved(scene, entity);
            Assert.IsTrue(internalVideoPlayerComponent.GetFor(scene, entity).Value.model.removed);
        }

        [Test]
        [Explicit]
        public void NotAllowExternalVideoWithoutPermissionsSet()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "http://fake/video.mp4",
                Playing = true,
            };

            scene.sceneData.allowedMediaHostnames = new[] { "fake" };

            LogAssert.Expect(LogType.Error, "External media asset url error: 'allowedMediaHostnames' missing in scene.json file.");

            string outputUrl = model.GetVideoUrl(scene.contentProvider,
                scene.sceneData.requiredPermissions,
                scene.sceneData.allowedMediaHostnames);

            Assert.AreEqual(string.Empty, outputUrl);
        }

        [Test]
        [Explicit]
        public void AllowExternalVideoWithRightPermissionsSet()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "http://fake/video.mp4",
                Playing = true,
            };

            scene.sceneData.allowedMediaHostnames = new[] { "fake" };
            scene.sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

            string outputUrl = model.GetVideoUrl(scene.contentProvider,
                scene.sceneData.requiredPermissions,
                scene.sceneData.allowedMediaHostnames);

            Assert.AreEqual(model.Src, outputUrl);
        }

        [Test]
        [Explicit]
        public void NotAllowExternalVideoWithWrongHostName()
        {
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "http://fake/video.mp4",
                Playing = true,
            };

            scene.sceneData.allowedMediaHostnames = new[] { "fakes" };
            scene.sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

            LogAssert.Expect(LogType.Error, $"External media asset url error: '{model.Src}' host name is not in 'allowedMediaHostnames' in scene.json file.");

            string outputUrl = model.GetVideoUrl(scene.contentProvider,
                scene.sceneData.requiredPermissions,
                scene.sceneData.allowedMediaHostnames);

            Assert.AreEqual(string.Empty, outputUrl);
        }

        [UnityTest]
        public IEnumerator StopVideoIfRenderingIsDisabled()
        {
            videoPlayerHandler.OnComponentCreated(scene, entity);
            videoPlayerHandler.hadUserInteraction = true;
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4",
                Playing = true
            };
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(true, videoPlayerHandler.videoPlayer.playing);

            // change rendering bool
            loadingScreenDataStore.decoupledLoadingHUD.visible.Set(true);

            Assert.AreEqual(false, videoPlayerHandler.videoPlayer.playing);

            videoPlayerHandler.OnComponentRemoved(scene, entity);
        }

        [UnityTest]
        public IEnumerator StartVideoAfterUserInteraction()
        {
            videoPlayerHandler.OnComponentCreated(scene, entity);
            videoPlayerHandler.hadUserInteraction = false;
            PBVideoPlayer model = new PBVideoPlayer()
            {
                Src = "video.mp4",
                Playing = true
            };
            videoPlayerHandler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => videoPlayerHandler.videoPlayer.GetState() == VideoState.READY);
            videoPlayerHandler.videoPlayer.Update();

            Assert.AreEqual(false, videoPlayerHandler.videoPlayer.playing);

            // change user interaction bool
            DCL.Helpers.Utils.LockCursor();

            Assert.AreEqual(true, videoPlayerHandler.videoPlayer.playing);

            videoPlayerHandler.OnComponentRemoved(scene, entity);
        }
    }
}
