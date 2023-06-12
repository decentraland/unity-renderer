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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.Systems.EngineInfo
{
    public class ECSEngineInfoSystemShould
    {
        private Action systemUpdate;
        private IECSComponentWriter componentWriter;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private InternalECSComponents internalComponents;
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;

        [SetUp]
        protected void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            componentWriter = Substitute.For<IECSComponentWriter>();
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            engineInfoComponent = internalComponents.EngineInfo;

            scene = testUtils.CreateScene(666);

            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo()
                {
                    SceneTick = 0,
                    SceneInitialRunTime = Time.realtimeSinceStartup,
                    SceneInitialFrameCount = Time.frameCount
                });

            var system = new ECSEngineInfoSystem(componentWriter, engineInfoComponent);

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

        [Test]
        public void UpdateEngineInfoComponentCorrectly()
        {
            void IncreaseSceneTick(IParcelScene scene)
            {
                var model = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model;
                model.SceneTick++;
                engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
            }

            Assert.IsNotNull(engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            IncreaseSceneTick(scene);
            systemUpdate();

            componentWriter.Received(1)
                           .PutComponent(
                                Arg.Is<int>((sceneNumberParam) => sceneNumberParam == scene.sceneData.sceneNumber),
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.ENGINE_INFO,
                                Arg.Is<PBEngineInfo>((componentModel) =>
                                    componentModel.TickNumber == 1
                                    && componentModel.TotalRuntime > 0
                                    && componentModel.FrameNumber > 0)
                            );

            componentWriter.ClearReceivedCalls();

            IncreaseSceneTick(scene);
            systemUpdate();

            componentWriter.Received(1)
                           .PutComponent(
                                Arg.Is<int>((sceneNumberParam) => sceneNumberParam == scene.sceneData.sceneNumber),
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.ENGINE_INFO,
                                Arg.Is<PBEngineInfo>((componentModel) =>
                                    componentModel.TickNumber == 2
                                    && componentModel.TotalRuntime > 0
                                    && componentModel.FrameNumber > 0)
                            );
        }
    }
}
