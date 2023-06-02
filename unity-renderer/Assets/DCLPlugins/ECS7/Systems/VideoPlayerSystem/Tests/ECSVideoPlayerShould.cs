using DCL;
using DCL.Components.Video.Plugin;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Shaders;
using ECSSystems.VideoPlayerSystem;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using VideoState = DCL.Components.Video.Plugin.VideoState;

namespace Tests
{
    public class ECSVideoPlayerSystemShould
    {
        private ECS7TestScene scene0;
        private ECS7TestScene scene1;
        private InternalECSComponents internalEcsComponents;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemsUpdate;
        private WebVideoPlayer videoPlayer;
        private IECSComponentWriter componentWriter;
        private SceneStateHandler sceneStateHandler;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            componentWriter = Substitute.For<IECSComponentWriter>();
            var executors = new Dictionary<int, ICRDTExecutor>();

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            var videoPlayerSystem = new ECSVideoPlayerSystem(
                internalEcsComponents.videoPlayerComponent,
                internalEcsComponents.videoMaterialComponent,
                internalEcsComponents.EngineInfo,
                componentWriter);

            systemsUpdate = () =>
            {
                internalEcsComponents.MarkDirtyComponentsUpdate();
                videoPlayerSystem.Update();
                internalEcsComponents.ResetDirtyComponentsUpdate();
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene0 = testUtils.CreateScene(666);
            scene1 = testUtils.CreateScene(678);

            videoPlayer = new WebVideoPlayer("test", "test.mp4", true, new VideoPluginWrapper_Mock());

            sceneStateHandler = new SceneStateHandler(
                Substitute.For<CRDTServiceContext>(),
                new Dictionary<int, IParcelScene>()
                {
                    { scene0.sceneData.sceneNumber, scene0 },
                    { scene1.sceneData.sceneNumber, scene1 }
                },
                internalEcsComponents.EngineInfo,
                internalEcsComponents.GltfContainerLoadingStateComponent);
            sceneStateHandler.InitializeEngineInfoComponent(scene0.sceneData.sceneNumber);
            sceneStateHandler.InitializeEngineInfoComponent(scene1.sceneData.sceneNumber);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils?.Dispose();
            videoPlayer?.Dispose();
            AssetPromiseKeeper_Material.i.Cleanup();
            AssetPromiseKeeper_Texture.i.Cleanup();
            sceneStateHandler.Dispose();
        }

        [UnityTest]
        public IEnumerator ApplyVideoMaterial()
        {
            var playerComponent = internalEcsComponents.videoPlayerComponent;
            var videoMaterialComponent = internalEcsComponents.videoMaterialComponent;

            ECS7TestEntity entity00 = scene0.CreateEntity(100);
            ECS7TestEntity entity01 = scene0.CreateEntity(101);
            ECS7TestEntity entity10 = scene1.CreateEntity(200);

            playerComponent.PutFor(scene0, entity00, new InternalVideoPlayer()
            {
                videoPlayer = videoPlayer,
                assignedMaterials = new List<InternalVideoPlayer.MaterialAssigned>(),
            });

            videoMaterialComponent.PutFor(scene0, entity00, new InternalVideoMaterial()
            {
                material = new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                videoTextureDatas = new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(100, ShaderUtils.BaseMap) },
            });

            videoMaterialComponent.PutFor(scene0, entity01, new InternalVideoMaterial()
            {
                material = new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                videoTextureDatas = new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(100, ShaderUtils.BaseMap) },
            });

            // Texture should not change on this
            videoMaterialComponent.PutFor(scene1, entity10, new InternalVideoMaterial()
            {
                material = new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                videoTextureDatas = new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(200, ShaderUtils.BaseMap) },
            });

            yield return new UnityEngine.WaitUntil(() => videoPlayer.GetState() == VideoState.READY);
            videoPlayer.Update();
            Assert.IsNotNull(videoPlayer.texture);

            systemsUpdate();

            var videoPlayerComponent = playerComponent.GetFor(scene0, entity00);
            var videoMaterial00 = videoMaterialComponent.GetFor(scene0, entity00);
            var videoMaterial01 = videoMaterialComponent.GetFor(scene0, entity01);
            var videoMaterial10 = videoMaterialComponent.GetFor(scene1, entity10);

            Assert.True(ReferenceEquals(videoMaterial00.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.model.videoPlayer.texture));
            Assert.True(ReferenceEquals(videoMaterial01.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.model.videoPlayer.texture));
            Assert.False(ReferenceEquals(videoMaterial10.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.model.videoPlayer.texture));
        }

        [UnityTest]
        public IEnumerator UpdateVideoEventComponentWithVideoState()
        {
            ECS7TestEntity entity = scene0.CreateEntity(100);

            // This internal component normally gets automatically added by the VideoPlayerHandler
            internalEcsComponents.videoPlayerComponent.PutFor(scene0, entity, new InternalVideoPlayer()
            {
                videoPlayer = videoPlayer
            });

            systemsUpdate();

            componentWriter.Received(1).AppendComponent(
                Arg.Is<int>(val => val == scene0.sceneData.sceneNumber),
                Arg.Is<long>(val => val == entity.entityId),
                Arg.Is<int>(val => val == ComponentID.VIDEO_EVENT),
                Arg.Is<PBVideoEvent>(val =>
                    val.State == DCL.ECSComponents.VideoState.VsLoading && val.Timestamp == 1),
                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
                );

            yield return new UnityEngine.WaitUntil(() => videoPlayer.GetState() == VideoState.READY);
            systemsUpdate();

            componentWriter.Received(1).AppendComponent(
                Arg.Is<int>(val => val == scene0.sceneData.sceneNumber),
                Arg.Is<long>(val => val == entity.entityId),
                Arg.Is<int>(val => val == ComponentID.VIDEO_EVENT),
                Arg.Is<PBVideoEvent>(val =>
                    val.State == DCL.ECSComponents.VideoState.VsReady && val.Timestamp == 2),
                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
            );
        }

        [Test]
        public void RemoveVideoEventComponent()
        {
            ECS7TestEntity entity = scene0.CreateEntity(100);

            // This internal component normally gets automatically added by the VideoPlayerHandler
            internalEcsComponents.videoPlayerComponent.PutFor(scene0, entity, new InternalVideoPlayer()
            {
                videoPlayer = videoPlayer
            });

            systemsUpdate();

            componentWriter.Received(1).AppendComponent(
                Arg.Is<int>(val => val == scene0.sceneData.sceneNumber),
                Arg.Is<long>(val => val == entity.entityId),
                Arg.Is<int>(val => val == ComponentID.VIDEO_EVENT),
                Arg.Is<PBVideoEvent>(val =>
                    val.State == DCL.ECSComponents.VideoState.VsLoading && val.Timestamp == 1),
                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
            );

            // remove video player
            internalEcsComponents.videoPlayerComponent.PutFor(scene0, entity, new InternalVideoPlayer()
            {
                removed = true
            });
            systemsUpdate();

            componentWriter.Received(1).RemoveComponent(
                Arg.Is<int>(val => val == scene0.sceneData.sceneNumber),
                Arg.Is<long>(val => val == entity.entityId),
                Arg.Is<int>(val => val == ComponentID.VIDEO_EVENT)
            );
        }

        [Test]
        public void StoreSceneTickInVideoEvent()
        {
            ECS7TestEntity entity = scene0.CreateEntity(100);

            // This internal component normally gets automatically added by the VideoPlayerHandler
            internalEcsComponents.videoPlayerComponent.PutFor(scene0, entity, new InternalVideoPlayer()
            {
                videoPlayer = videoPlayer
            });

            sceneStateHandler.IncreaseSceneTick(scene0.sceneData.sceneNumber);
            sceneStateHandler.IncreaseSceneTick(scene0.sceneData.sceneNumber);
            sceneStateHandler.IncreaseSceneTick(scene0.sceneData.sceneNumber);

            systemsUpdate();

            componentWriter.Received(1).AppendComponent(
                Arg.Is<int>(val => val == scene0.sceneData.sceneNumber),
                Arg.Is<long>(val => val == entity.entityId),
                Arg.Is<int>(val => val == ComponentID.VIDEO_EVENT),
                Arg.Is<PBVideoEvent>(val =>
                    val.TickNumber == 3),
                Arg.Is<ECSComponentWriteType>(val => val == (ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY))
            );
        }
    }
}
