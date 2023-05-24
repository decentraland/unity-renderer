using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using RPC.Context;
using System.Collections.Generic;
using DCL.ECS7;
using NUnit.Framework;

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
                Substitute.For<CRDTServiceContext>(),
                new Dictionary<int, IParcelScene>() { {scene.sceneData.sceneNumber, scene} },
                internalComponents.EngineInfo,
                internalComponents.GltfContainerLoadingStateComponent);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void IncreaseAndGetSceneTickCorrectly()
        {
            Assert.IsNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            uint newSceneTick = sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber);

            Assert.AreEqual(0, newSceneTick);
            var sceneEngineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsNotNull(sceneEngineInfo);
            Assert.AreEqual(0, sceneEngineInfo.model.SceneTick);

            sceneStateHandler.IncreaseSceneTick(scene.sceneData.sceneNumber);

            Assert.AreEqual(1, sceneEngineInfo.model.SceneTick);
            Assert.AreEqual(1, sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber));
        }
    }
}
