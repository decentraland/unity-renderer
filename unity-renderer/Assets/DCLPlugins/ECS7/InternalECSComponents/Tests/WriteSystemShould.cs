using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NUnit.Framework;

namespace Tests
{
    public class WriteSystemShould
    {
        private Action writeSystem;
        private IList<InternalComponentWriteData> writeComponentList;
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
            var internalComponents = new InternalECSComponents(manager, factory);
            writeComponentList = internalComponents.scheduledWrite;

            renderersComponent = internalComponents.renderersComponent;
            visibilityComponent = internalComponents.visibilityComponent;

            testUtils = new ECS7TestUtilsScenesAndEntities(manager);
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(1111);
            writeSystem = internalComponents.WriteSystemUpdate;
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void HandleDirtyComponents()
        {
            var model = new InternalVisibility();
            visibilityComponent.PutFor(scene, entity, model);
            Assert.IsTrue(visibilityComponent.GetFor(scene, entity).model.dirty);
            Assert.AreEqual(model, writeComponentList[0].data);
            Assert.AreEqual(scene, writeComponentList[0].scene);
            Assert.AreEqual(entity.entityId, writeComponentList[0].entityId);
            Assert.AreEqual((int)InternalECSComponentsId.VISIBILITY, writeComponentList[0].componentId);

            writeSystem();
            Assert.IsFalse(visibilityComponent.GetFor(scene, entity).model.dirty);
            Assert.AreEqual(0, writeComponentList.Count);
        }

        [Test]
        public void ScheduleRemoveComponentWithDefault()
        {
            var rendererComponent = new InternalRenderers();
            renderersComponent.PutFor(scene, entity, rendererComponent);
            visibilityComponent.PutFor(scene, entity, new InternalVisibility());
            visibilityComponent.RemoveFor(scene, entity, new InternalVisibility());

            Assert.AreEqual(3, writeComponentList.Count);
            Assert.AreEqual(rendererComponent, writeComponentList[0].data);
            Assert.IsNull(writeComponentList[2].data);
            Assert.AreEqual(scene, writeComponentList[2].scene);
            Assert.AreEqual(entity.entityId, writeComponentList[2].entityId);
            Assert.AreEqual((int)InternalECSComponentsId.VISIBILITY, writeComponentList[2].componentId);

            writeSystem();
            Assert.IsNull(visibilityComponent.GetFor(scene, entity));
            Assert.AreEqual(0, writeComponentList.Count);
        }

        [Test]
        public void RemoveComponentWithoutDefault()
        {
            visibilityComponent.PutFor(scene, entity, new InternalVisibility());
            visibilityComponent.RemoveFor(scene, entity);

            Assert.AreEqual(1, writeComponentList.Count);
            Assert.IsNull(visibilityComponent.GetFor(scene, entity));

            writeSystem();
            Assert.AreEqual(0, writeComponentList.Count);
        }
    }
}