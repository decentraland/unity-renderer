using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.ECSEngineInfoSystem;
using NSubstitute;
using NUnit.Framework;
using RPC.Context;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;

namespace Tests
{
    public class ECSEngineInfoSystemShould
    {
        private ECSEngineInfoSystem system;
        private IECSComponentWriter componentWriter;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private InternalECSComponents internalComponents;
        private SceneStateHandler sceneStateHandler;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            componentWriter = Substitute.For<IECSComponentWriter>();
            // var writeComponentSubscriber = Substitute.For<IDummyEventSubscriber<int, long, int, byte[], int, ECSComponentWriteType, CrdtMessageType>>();
            // componentWriter = new ECSComponentWriter(writeComponentSubscriber.React);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);

            sceneStateHandler = new SceneStateHandler(
                Substitute.For<CRDTServiceContext>(),
                new Dictionary<int, IParcelScene>() { {scene.sceneData.sceneNumber, scene} },
                internalComponents.EngineInfo,
                internalComponents.GltfContainerLoadingStateComponent);
            system = new ECSEngineInfoSystem(componentWriter, internalComponents.EngineInfo);
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void UpdateEngineInfoComponentCorrectly()
        {
            sceneStateHandler.InitializeEngineInfoComponent(scene.sceneData.sceneNumber);

            Assert.IsNotNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            sceneStateHandler.IncreaseSceneTick(scene.sceneData.sceneNumber);

            uint currentSceneTick = sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber);
            system.Update();

            // componentWriter.ReceivedCalls();
            componentWriter.ReceivedWithAnyArgs().PutComponent(
                Arg.Any<int>(),
                Arg.Any<long>(),
                Arg.Any<int>(),
                Arg.Any<PBEngineInfo>()
            );

            /*componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.ENGINE_INFO,
                Arg.Is<PBEngineInfo>((componentModel) => componentModel.TickNumber == currentSceneTick)
                // Arg.Any<PBEngineInfo>()
            );*/
        }
    }
}
