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
using System.Collections.Generic;

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
            var keepEntityAliveComponent = new InternalECSComponent<InternalComponent>(
                0, componentsManager, componentsFactory, null,
                new KeyValueSet<ComponentIdentifier,ComponentWriteData>(),
                executors);
            componentWriter = Substitute.For<IECSComponentWriter>();

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);

            // sceneStateHandler = new SceneStateHandler(new Dictionary<int, IParcelScene>() { { scene.sceneData.sceneNumber, scene } },
            //     internalComponents.EngineInfo,
            //     internalComponents.GltfContainerLoadingStateComponent);
            // system = new ECSEngineInfoSystem(componentWriter, sceneStateHandler, internalComponents.EngineInfo);
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void UpdateEngineInfoComponentCorrectly()
        {
            // Assert.IsNull();

            // sceneStateHandler.GetSceneTick(scene.sceneData.sceneNumber);

            system.Update();

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.ENGINE_INFO,
                Arg.Any<PBEngineInfo>()
                // Arg.Is<PBEngineInfo>((componentModel) => componentModel.TickNumber == 1)
                    // componentModel.TickNumber == 1 &&
                    // componentModel.FrameNumber == 1 &&
                    // componentModel.TotalRuntime == 1)
            );
        }
    }
}
