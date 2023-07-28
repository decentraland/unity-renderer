using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.ECSEngineInfoSystem;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TestUtils;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Systems.EngineInfo
{
    public class ECSEngineInfoSystemShould
    {
        private Action systemUpdate;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private InternalECSComponents internalComponents;
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
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
            engineInfoComponent = internalComponents.EngineInfo;

            scene = testUtils.CreateScene(666);

            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { scene.sceneData.sceneNumber, new ComponentWriter(outgoingMessages) }
            };

            componentPool = new WrappedComponentPool<IWrappedComponent<PBEngineInfo>>(0, () => new ProtobufWrappedComponent<PBEngineInfo>(new PBEngineInfo()));

            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo()
                {
                    SceneTick = 0,
                    SceneInitialRunTime = Time.realtimeSinceStartup,
                    SceneInitialFrameCount = Time.frameCount
                });

            var system = new ECSEngineInfoSystem(componentsWriter, componentPool, internalComponents.EngineInfo);

            systemUpdate = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                system.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
        }

        [UnityTest]
        public IEnumerator UpdateEngineInfoComponentCorrectly()
        {
            void IncreaseSceneTick(IParcelScene scene)
            {
                var model = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
                model.SceneTick++;
                engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
            }

            var engineInfo = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            Assert.IsNotNull(engineInfo);

            IncreaseSceneTick(scene);
            uint sceneFrame = (uint)(Time.frameCount - engineInfo.Value.model.SceneInitialFrameCount);
            systemUpdate();

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

            IncreaseSceneTick(scene);
            systemUpdate();

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
