using DCL;
using DCL.Components.Video.Plugin;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.Shaders;
using ECSSystems.VideoPlayerSystem;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TestUtils;
using UnityEngine;
using UnityEngine.TestTools;
using VideoState = DCL.Components.Video.Plugin.VideoState;

namespace Tests.Systems.VideoPlayer
{
    public class ECSVideoPlayerSystemShould
    {
        private ECS7TestScene scene0;
        private ECS7TestScene scene1;
        private InternalECSComponents internalEcsComponents;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemsUpdate;
        private WebVideoPlayer videoPlayer;
        private DualKeyValueSet<long, int, WriteData> outgoingMessagesScene0;
        private DualKeyValueSet<long, int, WriteData> outgoingMessagesScene1;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            outgoingMessagesScene0 = new DualKeyValueSet<long, int, WriteData>();
            outgoingMessagesScene1 = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessagesScene0) },
                { 678, new ComponentWriter(outgoingMessagesScene1) }
            };

            var executors = new Dictionary<int, ICRDTExecutor>();

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            var videoPlayerSystem = new ECSVideoPlayerSystem(
                internalEcsComponents.videoPlayerComponent,
                internalEcsComponents.videoMaterialComponent,
                internalEcsComponents.EngineInfo,
                componentsWriter,
                new WrappedComponentPool<IWrappedComponent<PBVideoEvent>>(0, () => new ProtobufWrappedComponent<PBVideoEvent>(new PBVideoEvent())));

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

            internalEcsComponents.EngineInfo.PutFor(scene0, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo());
            internalEcsComponents.EngineInfo.PutFor(scene1, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo());
        }

        [TearDown]
        public void TearDown()
        {
            testUtils?.Dispose();
            videoPlayer?.Dispose();
            AssetPromiseKeeper_Material.i.Cleanup();
            AssetPromiseKeeper_Texture.i.Cleanup();
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

            videoMaterialComponent.PutFor(scene0, entity00, new InternalVideoMaterial(
                new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(100, ShaderUtils.BaseMap) }));

            videoMaterialComponent.PutFor(scene0, entity01, new InternalVideoMaterial(
                new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(100, ShaderUtils.BaseMap) }
            ));

            // Texture should not change on this
            videoMaterialComponent.PutFor(scene1, entity10, new InternalVideoMaterial(
                new Material(Shader.Find("DCL/Universal Render Pipeline/Lit")) { mainTexture = new Texture2D(1, 1), },
                new List<InternalVideoMaterial.VideoTextureData>() { new InternalVideoMaterial.VideoTextureData(200, ShaderUtils.BaseMap) }
            ));

            yield return new WaitUntil(() => videoPlayer.GetState() == VideoState.READY);
            videoPlayer.Update();
            Assert.IsNotNull(videoPlayer.texture);

            systemsUpdate();

            var videoPlayerComponent = playerComponent.GetFor(scene0, entity00);
            var videoMaterial00 = videoMaterialComponent.GetFor(scene0, entity00);
            var videoMaterial01 = videoMaterialComponent.GetFor(scene0, entity01);
            var videoMaterial10 = videoMaterialComponent.GetFor(scene1, entity10);

            Assert.True(ReferenceEquals(videoMaterial00.Value.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.Value.model.videoPlayer.texture));
            Assert.True(ReferenceEquals(videoMaterial01.Value.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.Value.model.videoPlayer.texture));
            Assert.False(ReferenceEquals(videoMaterial10.Value.model.material.GetTexture(ShaderUtils.BaseMap), videoPlayerComponent.Value.model.videoPlayer.texture));
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

            outgoingMessagesScene0.Append_Called<PBVideoEvent>(
                entity.entityId,
                ComponentID.VIDEO_EVENT,
                val =>
                    val.State == DCL.ECSComponents.VideoState.VsLoading && val.Timestamp == 1
            );

            yield return new WaitUntil(() => videoPlayer.GetState() == VideoState.READY);
            systemsUpdate();

            outgoingMessagesScene0.Append_Called<PBVideoEvent>(
                entity.entityId,
                ComponentID.VIDEO_EVENT,
                val =>
                    val.State == DCL.ECSComponents.VideoState.VsReady && val.Timestamp == 2
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

            outgoingMessagesScene0.Append_Called<PBVideoEvent>(
                entity.entityId,
                ComponentID.VIDEO_EVENT,
                val =>
                    val.State == DCL.ECSComponents.VideoState.VsLoading && val.Timestamp == 1
            );

            // remove video player
            internalEcsComponents.videoPlayerComponent.PutFor(scene0, entity, new InternalVideoPlayer()
            {
                removed = true
            });

            systemsUpdate();

            outgoingMessagesScene0.Remove_Called(
                entity.entityId,
                ComponentID.VIDEO_EVENT
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

            internalEcsComponents.EngineInfo.PutFor(scene0, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = 3
            });

            systemsUpdate();

            outgoingMessagesScene0.Append_Called<PBVideoEvent>(
                entity.entityId,
                ComponentID.VIDEO_EVENT,
                val =>
                    val.TickNumber == 3
            );
        }
    }
}
