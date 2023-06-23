using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.UiCanvasInformationSystem;
using NUnit.Framework;
using System.Collections.Generic;
using TestUtils;

namespace Tests
{
    public class ECSUiCanvasInformationSystemShould
    {
        private ECSUiCanvasInformationSystem system;
        private Dictionary<int, ComponentWriter> componentsWriter;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private BaseList<IParcelScene> loadedScenes;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);

            scene = testUtils.CreateScene(666);
            loadedScenes = new BaseList<IParcelScene> { scene };

            system = new ECSUiCanvasInformationSystem(componentsWriter,
                new WrappedComponentPool<IWrappedComponent<PBUiCanvasInformation>>(0, () => new ProtobufWrappedComponent<PBUiCanvasInformation>(new PBUiCanvasInformation())),
                loadedScenes);
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

            outgoingMessages.Put_Called<PBUiCanvasInformation>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                e => e.Width != 0 && e.Height != 0
            );
        }

        [Test]
        public void UpdateUiCanvasInformationComponentOnNewScene()
        {
            system.Update();

            outgoingMessages.Put_Called<PBUiCanvasInformation>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                e => e.Width != 0 && e.Height != 0
            );

            outgoingMessages.Clear_Calls();

            DualKeyValueSet<long, int, WriteData> newSceneOutgoingMessages = new DualKeyValueSet<long, int, WriteData>();
            componentsWriter.Add(999, new ComponentWriter(newSceneOutgoingMessages));

            var newScene = testUtils.CreateScene(999);
            loadedScenes.Add(newScene);

            newSceneOutgoingMessages.Put_Called<PBUiCanvasInformation>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.UI_CANVAS_INFORMATION,
                e => e.Width != 0 && e.Height != 0
            );

            newSceneOutgoingMessages.Clear_Calls();
        }
    }
}
