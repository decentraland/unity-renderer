using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.UiCanvasInformationSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tests
{
    public class ECSUiCanvasInformationSystemShould
    {
        private ECSUiCanvasInformationSystem system;
        private IECSComponentWriter componentWriter;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private BaseList<IParcelScene> loadedScenes;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            componentWriter = Substitute.For<IECSComponentWriter>();
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            loadedScenes = new BaseList<IParcelScene> { scene };
            system = new ECSUiCanvasInformationSystem(componentWriter, loadedScenes);
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void UpdateUiCanvasInformationComponent()
        {
            system.Update();

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                Arg.Is<PBUiCanvasInformation>(e => e.Width != 0 && e.Height != 0)
            );
            componentWriter.ClearReceivedCalls();
        }

        [Test]
        public void UpdateUiCanvasInformationComponentOnNewScene()
        {
            system.Update();

            componentWriter.Received(1).PutComponent(
                scene.sceneData.sceneNumber,
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                Arg.Is<PBUiCanvasInformation>(e => e.Width != 0 && e.Height != 0)
            );
            componentWriter.ClearReceivedCalls();

            var newScene = testUtils.CreateScene(999);
            loadedScenes.Add(newScene);

            componentWriter.Received(1).PutComponent(
                newScene.sceneData.sceneNumber,
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                Arg.Is<PBUiCanvasInformation>(e => e.Width != 0 && e.Height != 0)
            );
            componentWriter.ClearReceivedCalls();
        }
    }
}
