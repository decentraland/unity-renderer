using System.Collections.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Tests
{
    public class UiContainerHandlerShould
    {
        private ECS7TestEntity entity;
        private ECS7TestScene scene;
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;
        private IInternalECSComponent<InternalUiContainer> internalUiContainer;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);

            internalUiContainer = new InternalECSComponent<InternalUiContainer>(
                0,
                manager,
                factory,
                () => new UiContainerHandler(() => internalUiContainer),
                new List<InternalComponentWriteData>());

            sceneTestHelper = new ECS7TestUtilsScenesAndEntities(manager);
            scene = sceneTestHelper.CreateScene("temptation");
            entity = scene.CreateEntity(111);

            var initialModel = new InternalUiContainer();
            initialModel.childElements.Add(new VisualElement());
            initialModel.hasTransform = true;

            internalUiContainer.PutFor(scene, entity, initialModel);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void NotRemoveComponentWhenTransformLeft()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            model.childElements.Clear();
            internalUiContainer.PutFor(scene, entity, model);

            Assert.NotNull(internalUiContainer.GetFor(scene, entity));
        }

        [Test]
        public void NotRemoveComponentWhenChildrenLeft()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            model.hasTransform = false;
            internalUiContainer.PutFor(scene, entity, model);

            Assert.NotNull(internalUiContainer.GetFor(scene, entity));
        }

        [Test]
        public void RemoveWhenNoChildrenAndNoTransform()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            model.hasTransform = false;
            model.childElements.Clear();
            internalUiContainer.PutFor(scene, entity, model);

            Assert.IsNull(internalUiContainer.GetFor(scene, entity));
            Assert.AreEqual(0, model.rootElement.childCount);
        }
    }
}