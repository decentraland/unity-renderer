using DCL;
using DCL.Components.Video.Plugin;
using DCL.CRDT;
using System;
using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Shaders;
using ECSSystems.VideoPlayerSystem;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

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

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            var videoPlayerSystem = new ECSVideoPlayerSystem(internalEcsComponents.videoPlayerComponent,
                internalEcsComponents.videoMaterialComponent);

            systemsUpdate = () =>
            {
                videoPlayerSystem.Update();
                internalEcsComponents.MarkDirtyComponentsUpdate();
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene0 = testUtils.CreateScene(666);
            scene1 = testUtils.CreateScene(678);

            videoPlayer = new WebVideoPlayer("test", "test.mp4", true, new VideoPluginWrapper_Mock());
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

            yield return new WaitUntil(() => videoPlayer.GetState() == VideoState.READY);
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
    }
}
