// using NUnit.Framework;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;

namespace Tests
{
    // TODO: RE-ENABLE TESTS AFTER INSTANT STEPS FIX
    public class SceneStateHandlerShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private SceneStateHandler sceneStateHandler;
        private InternalECSComponents internalComponents;

        // [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);

            sceneStateHandler = new SceneStateHandler(
                new Dictionary<int, IParcelScene>() { {scene.sceneData.sceneNumber, scene} },
                internalComponents.EngineInfo,
                internalComponents.GltfContainerLoadingStateComponent);
        }

        // [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        // [Test]
        public void IncreaseAndGetSceneTickCorrectly()
        {
            // Assert.IsNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            uint newSceneTick = sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber);

            // Assert.AreEqual(0, newSceneTick);
            var sceneEngineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            // Assert.IsNotNull(sceneEngineInfo);
            // Assert.AreEqual(0, sceneEngineInfo.model.SceneTick);

            sceneStateHandler.IncreaseSceneTick(scene.sceneData.sceneNumber);

            // Assert.AreEqual(1, sceneEngineInfo.model.SceneTick);
            // Assert.AreEqual(1, sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber));
        }

        // [Test]
        public void ReturnSceneGltfLoadingFinishedCorrectly()
        {
            // Assert.IsTrue(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));

            IDCLEntity gltfEntity = scene.CreateEntity(512);
            var model = new InternalGltfContainerLoadingState() { LoadingState = LoadingState.Loading };
            internalComponents.GltfContainerLoadingStateComponent.PutFor(scene, gltfEntity, model);

            // Assert.IsFalse(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));

            model.LoadingState = LoadingState.Finished;
            internalComponents.GltfContainerLoadingStateComponent.PutFor(scene, gltfEntity, model);

            // Assert.IsTrue(sceneStateHandler.IsSceneGltfLoadingFinished(scene.sceneData.sceneNumber));
        }
    }
}
