using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.IncreaseSceneTickSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests.Systems.IncreaseSceneTick
{
    public class IncreaseSceneTickSystemShould
    {
        private Action systemUpdate;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private IInternalECSComponent<InternalIncreaseTickTagComponent> increaseSceneTickTagComponent;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            var componentWriter = Substitute.For<IECSComponentWriter>();
            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            engineInfoComponent = internalComponents.EngineInfo;
            increaseSceneTickTagComponent = internalComponents.IncreaseSceneTick;

            scene = testUtils.CreateScene(666);

            var system = new IncreaseSceneTickSystem(increaseSceneTickTagComponent, engineInfoComponent);

            systemUpdate = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                system.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void IncreaseSceneTick()
        {
            engineInfoComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY,
                new InternalEngineInfo()
                {
                    SceneTick = 0,
                });

            systemUpdate();
            Assert.AreEqual(0, engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick);

            increaseSceneTickTagComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalIncreaseTickTagComponent());
            systemUpdate();
            Assert.AreEqual(1, engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick);

            systemUpdate();
            Assert.AreEqual(1, engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick);

            increaseSceneTickTagComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalIncreaseTickTagComponent());
            systemUpdate();
            Assert.AreEqual(2, engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick);
        }
    }
}
