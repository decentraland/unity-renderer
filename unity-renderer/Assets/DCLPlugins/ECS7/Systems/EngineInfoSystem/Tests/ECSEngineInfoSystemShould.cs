using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
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
using TestUtils;
using UnityEngine;

namespace Tests
{
    public class ECSEngineInfoSystemShould
    {
        private ECSEngineInfoSystem system;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private InternalECSComponents internalComponents;
        private SceneStateHandler sceneStateHandler;
        private WrappedComponentPool<IWrappedComponent<PBEngineInfo>> componentPool;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;

        [SetUp]
        protected void SetUp()
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

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { scene.sceneData.sceneNumber, new ComponentWriter(outgoingMessages) }
            };

            componentPool = new WrappedComponentPool<IWrappedComponent<PBEngineInfo>>(0, () => new ProtobufWrappedComponent<PBEngineInfo>(new PBEngineInfo()));

            system = new ECSEngineInfoSystem(componentsWriter,componentPool, internalComponents.EngineInfo);
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
            sceneStateHandler.Dispose();
        }

        [UnityTest]
        public IEnumerator UpdateEngineInfoComponentCorrectly()
        {
            Assert.IsNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            sceneStateHandler.InitializeEngineInfoComponent(scene.sceneData.sceneNumber);

            Assert.IsNotNull(internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            sceneStateHandler.IncreaseSceneTick(scene.sceneData.sceneNumber);
            var engineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            uint sceneFrame = (uint)(Time.frameCount - engineInfo.Value.model.SceneInitialRunTime);
            system.Update();

            outgoingMessages.Put_Called<PBEngineInfo>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.ENGINE_INFO,
                (componentModel) =>
                    componentModel.TickNumber == 1
                    && componentModel.TotalRuntime >= 0
                    && componentModel.FrameNumber == sceneFrame
                );
            outgoingMessages.Clear_Calls();

            // Wait for the next frame
            yield return null;

            sceneStateHandler.IncreaseSceneTick(scene.sceneData.sceneNumber);
            system.Update();

            outgoingMessages.Put_Called<PBEngineInfo>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.ENGINE_INFO,
                (componentModel) =>
                    componentModel.TickNumber == 2
                    && componentModel.TotalRuntime > 0
                    && componentModel.FrameNumber == sceneFrame + 1
            );
        }
    }
}

