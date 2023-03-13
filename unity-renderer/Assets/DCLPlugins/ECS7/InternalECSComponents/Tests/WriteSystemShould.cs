using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Tests
{
    public class WriteSystemShould
    {
        private Action updateMarkComponentsAsDirty;
        private Action updateRemoveComponentsAsDirty;
        private IReadOnlyKeyValueSet<ComponentIdentifier, ComponentWriteData> markAsDirtyComponent;
        private IDCLEntity entity;
        private IParcelScene scene;
        private IInternalECSComponent<InternalRenderers> renderersComponent;
        private IInternalECSComponent<InternalVisibility> visibilityComponent;
        private ECS7TestUtilsScenesAndEntities testUtils;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(manager, factory, executors);
            markAsDirtyComponent = internalComponents.markAsDirtyComponents;

            renderersComponent = internalComponents.renderersComponent;
            visibilityComponent = internalComponents.visibilityComponent;

            testUtils = new ECS7TestUtilsScenesAndEntities(manager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1111);
            updateMarkComponentsAsDirty = internalComponents.MarkDirtyComponentsUpdate;
            updateRemoveComponentsAsDirty = internalComponents.ResetDirtyComponentsUpdate;
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void HandleDirtyComponents()
        {
            ComponentIdentifier componentIdentifier = new ComponentIdentifier(scene.sceneData.sceneNumber,
                entity.entityId,
                visibilityComponent.ComponentId);

            var model = new InternalVisibility();
            visibilityComponent.PutFor(scene, entity, model);

            Assert.AreEqual(model, markAsDirtyComponent[componentIdentifier].Data);

            updateMarkComponentsAsDirty();

            Assert.IsTrue(visibilityComponent.GetFor(scene, entity).model.dirty);

            updateRemoveComponentsAsDirty();
            Assert.IsFalse(visibilityComponent.GetFor(scene, entity).model.dirty);
            Assert.AreEqual(0, markAsDirtyComponent.Count);
        }

        [Test]
        public void ScheduleRemoveComponentWithDefault()
        {
            var defaultVisibility = new InternalVisibility();
            renderersComponent.PutFor(scene, entity, new InternalRenderers());
            visibilityComponent.PutFor(scene, entity, new InternalVisibility() { visible = !defaultVisibility.visible });
            visibilityComponent.RemoveFor(scene, entity, defaultVisibility);

            ComponentIdentifier visibilityComponentIdentifier = new ComponentIdentifier(scene.sceneData.sceneNumber,
                entity.entityId,
                visibilityComponent.ComponentId);

            // Expected `1` since renderersComponent should auto-remove itself when model contains no renderer
            // we are keeping it there to test for any possible race condition with that behavior
            Assert.AreEqual(1, markAsDirtyComponent.Count);
            Assert.AreEqual(defaultVisibility, markAsDirtyComponent[visibilityComponentIdentifier].Data);
            Assert.IsTrue(markAsDirtyComponent[visibilityComponentIdentifier].IsDelayedRemoval);

            updateMarkComponentsAsDirty();
            updateRemoveComponentsAsDirty();

            Assert.IsNull(visibilityComponent.GetFor(scene, entity));
            Assert.AreEqual(0, markAsDirtyComponent.Count);
        }

        [Test]
        public void RemoveComponentWithoutDefault()
        {
            visibilityComponent.PutFor(scene, entity, new InternalVisibility());
            Assert.AreEqual(1, markAsDirtyComponent.Count);

            visibilityComponent.RemoveFor(scene, entity);
            Assert.AreEqual(0, markAsDirtyComponent.Count);
            Assert.IsNull(visibilityComponent.GetFor(scene, entity));
        }
    }
}
