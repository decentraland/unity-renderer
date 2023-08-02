using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NUnit.Framework;
using RPC.Context;
using System.Collections.Generic;

namespace Tests
{
    public class SceneStateHandlerShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private SceneStateHandler sceneStateHandler;
        private InternalECSComponents internalComponents;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);

            sceneStateHandler = new SceneStateHandler(
                new CRDTServiceContext(),
                new RestrictedActionsContext(),
                new Dictionary<int, IParcelScene>() { { scene.sceneData.sceneNumber, scene } },
                internalComponents.EngineInfo,
                internalComponents.GltfContainerLoadingStateComponent);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void InitializeSceneEngineInfoComponentForScenes()
        {
            Assert.IsNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            sceneStateHandler.InitializeEngineInfoComponent(scene.sceneData.sceneNumber);

            Assert.IsNotNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));
        }

        [Test]
        public void GetAndInitializeSceneTickCorrectly()
        {
            sceneStateHandler.InitializeEngineInfoComponent(scene.sceneData.sceneNumber);
            uint newSceneTick = sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber);

            Assert.AreEqual(0, newSceneTick);
            var sceneEngineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsNotNull(sceneEngineInfo);
            Assert.AreEqual(0, sceneEngineInfo.Value.model.SceneTick);

            // Copy struct as it cannot be manipulated
            InternalEngineInfo finalModel = sceneEngineInfo.Value.model;
            finalModel.SceneTick++;
            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, finalModel);

            Assert.AreEqual(1, sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber));
        }

        [Test]
        public void ReturnSceneGltfLoadingFinishedCorrectly()
        {
            Assert.IsTrue(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));

            IDCLEntity gltfEntity = scene.CreateEntity(512);
            var model = new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Loading };
            internalComponents.GltfContainerLoadingStateComponent.PutFor(scene, gltfEntity, model);

            Assert.IsFalse(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));

            model.LoadingState = LoadingState.Finished;
            internalComponents.GltfContainerLoadingStateComponent.PutFor(scene, gltfEntity, model);

            Assert.IsTrue(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));
        }
    }
}
