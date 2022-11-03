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
            var internalComponents = new InternalECSComponents(manager, factory);

            internalUiContainer = internalComponents.uiContainerComponent;

            sceneTestHelper = new ECS7TestUtilsScenesAndEntities(manager);
            scene = sceneTestHelper.CreateScene("temptation");
            entity = scene.CreateEntity(111);

            var initialModel = new InternalUiContainer();
            initialModel.rootElement.Add(new VisualElement());
            initialModel.components.Add(0);

            internalUiContainer.PutFor(scene, entity, initialModel);
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
        }

        [Test]
        public void NotRemoveComponentWhenComponentsLeft()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            //  model.components.Count == 1
            model.rootElement.Clear();
            internalUiContainer.PutFor(scene, entity, model);

            Assert.NotNull(internalUiContainer.GetFor(scene, entity));
        }

        [Test]
        public void NotRemoveComponentWhenChildrenLeft()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            model.components.Clear();
            //  model.rootElement.childCount == 1
            internalUiContainer.PutFor(scene, entity, model);

            Assert.NotNull(internalUiContainer.GetFor(scene, entity));
        }

        [Test]
        public void RemoveWhenNoChildrenAndNoComponents()
        {
            var model = internalUiContainer.GetFor(scene, entity).model;
            model.components.Clear();
            model.rootElement.Clear();
            internalUiContainer.PutFor(scene, entity, model);

            Assert.IsNull(internalUiContainer.GetFor(scene, entity));
            Assert.AreEqual(0, model.rootElement.childCount);
        }
    }
}