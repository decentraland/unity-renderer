using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.ScenesUiSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSScenesUiSystemShould
    {
        private ECS7TestUtilsScenesAndEntities sceneTestHelper;
        private UIDocument uiDocument;
        private IInternalECSComponent<InternalUiContainer> uiContainerComponent;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var internalComponents = new InternalECSComponents(manager, factory);
            uiContainerComponent = internalComponents.uiContainerComponent;

            sceneTestHelper = new ECS7TestUtilsScenesAndEntities(manager);
            uiDocument = Object.Instantiate(Resources.Load<UIDocument>("ScenesUI"));
        }

        [TearDown]
        public void TearDown()
        {
            sceneTestHelper.Dispose();
            Object.Destroy(uiDocument.gameObject);
        }

        [Test]
        public void GetCurrentScene()
        {
            IList<IParcelScene> loadedScenes = new List<IParcelScene>()
            {
                sceneTestHelper.CreateScene(666),
                sceneTestHelper.CreateScene(667),
                sceneTestHelper.CreateScene(668)
            };

            // not valid scene id
            Assert.IsNull(ECSScenesUiSystem.GetCurrentScene(-1, loadedScenes));

            // not loaded scene id
            Assert.IsNull(ECSScenesUiSystem.GetCurrentScene(0, loadedScenes));

            // loaded scene
            Assert.AreEqual(sceneTestHelper.GetScene(666),
                ECSScenesUiSystem.GetCurrentScene(666, loadedScenes));
        }

        [Test]
        public void ClearUICorrectly()
        {
            uiDocument.rootVisualElement.Add(new VisualElement());

            ECSScenesUiSystem.ClearUI(uiDocument);
            Assert.AreEqual(0, uiDocument.rootVisualElement.childCount);
        }

        [Test]
        public void ApplySceneUI()
        {
            ECS7TestScene scene = sceneTestHelper.CreateScene(666);

            // root scene ui component should not exist
            Assert.IsNull(uiContainerComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            // therefor scene ui should not apply
            Assert.IsFalse(ECSScenesUiSystem.ApplySceneUI(uiContainerComponent, uiDocument, scene));

            // but should be applied when component exist
            uiContainerComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalUiContainer());
            Assert.IsTrue(ECSScenesUiSystem.ApplySceneUI(uiContainerComponent, uiDocument, scene));
            Assert.AreEqual(1, uiDocument.rootVisualElement.childCount);
        }

        [Test]
        public void CreateSceneRootUIContainer()
        {
            ECS7TestScene scene = sceneTestHelper.CreateScene(666);

            // root scene ui component should not exist
            Assert.IsNull(uiContainerComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            // add ui element to scene
            const int entityId = 111;
            var model = new InternalUiContainer();
            model.components.Add(1);
            uiContainerComponent.PutFor(scene, entityId, model);

            // apply parenting
            ECSScenesUiSystem.ApplyParenting(uiContainerComponent);

            // root scene ui component should exist now
            Assert.IsNotNull(uiContainerComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY));

            var rootSceneModel = uiContainerComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model;
            var entityModel = uiContainerComponent.GetFor(scene, entityId).model;
            Assert.IsTrue(rootSceneModel.rootElement.Contains(entityModel.rootElement));
        }

        [Test]
        public void ApplyParenting()
        {
            ECS7TestScene scene = sceneTestHelper.CreateScene(666);

            const int childEntityId = 111;
            const int parentEntityId = 112;

            var childModel = new InternalUiContainer() { parentId = parentEntityId };
            childModel.components.Add(1);

            uiContainerComponent.PutFor(scene, childEntityId, childModel);

            // apply parenting
            ECSScenesUiSystem.ApplyParenting(uiContainerComponent);

            // parent doesnt exist yet, so it shouldn't be any parenting
            Assert.IsNull(uiContainerComponent.GetFor(scene, childEntityId).model.parentElement);

            // create parent container
            var parentModel = new InternalUiContainer();
            parentModel.components.Add(1);
            uiContainerComponent.PutFor(scene, parentEntityId, parentModel);

            // apply parenting
            ECSScenesUiSystem.ApplyParenting(uiContainerComponent);

            // parenting should be applied
            var parentEntityModel = uiContainerComponent.GetFor(scene, parentEntityId).model;
            var childEntityModel = uiContainerComponent.GetFor(scene, childEntityId).model;
            Assert.AreEqual(parentEntityModel.rootElement, childEntityModel.parentElement);
            Assert.IsTrue(parentEntityModel.rootElement.Contains(childEntityModel.rootElement));
        }

        [Test]
        public void ApplyUiOnAlreadyLoadedScene()
        {
            ECS7TestScene scene = sceneTestHelper.CreateScene(666);
            IWorldState worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneNumber().Returns(666);

            // create system
            var system = new ECSScenesUiSystem(
                uiDocument,
                uiContainerComponent,
                new BaseList<IParcelScene> { scene },
                worldState);

            // create root ui for scene
            InternalUiContainer rootSceneContainer = new InternalUiContainer();
            uiContainerComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, rootSceneContainer);

            // do system update
            system.Update();

            // ui document should have scene ui set
            Assert.IsTrue(uiDocument.rootVisualElement.Contains(rootSceneContainer.rootElement));
        }

        [Test]
        public void ApplyUiAsSceneLoads()
        {
            ECS7TestScene scene = sceneTestHelper.CreateScene(666);
            IWorldState worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneNumber().Returns(666);
            BaseList<IParcelScene> loadedScenes = new BaseList<IParcelScene>();

            // create system
            var system = new ECSScenesUiSystem(
                uiDocument,
                uiContainerComponent,
                loadedScenes,
                worldState);

            // create root ui for scene
            InternalUiContainer rootSceneContainer = new InternalUiContainer();
            uiContainerComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, rootSceneContainer);

            // do system update
            system.Update();

            // scene was not loaded yet
            Assert.IsFalse(uiDocument.rootVisualElement.Contains(rootSceneContainer.rootElement));

            // set scene as loaded
            loadedScenes.Add(scene);

            // do system update
            system.Update();

            // ui document should have scene ui set
            Assert.IsTrue(uiDocument.rootVisualElement.Contains(rootSceneContainer.rootElement));
        }

        [Test]
        public void ApplyUiOnSceneChanged()
        {
            const int scene1Number = 666;
            const int scene2Number = 667;
            const int scene_without_uiId = 668;

            ECS7TestScene scene1 = sceneTestHelper.CreateScene(scene1Number);
            ECS7TestScene scene2 = sceneTestHelper.CreateScene(scene2Number);
            ECS7TestScene scene_without_ui = sceneTestHelper.CreateScene(scene_without_uiId);

            IWorldState worldState = Substitute.For<IWorldState>();
            worldState.GetCurrentSceneNumber().Returns(scene1Number);
            BaseList<IParcelScene> loadedScenes = new BaseList<IParcelScene>();

            // create system
            var system = new ECSScenesUiSystem(
                uiDocument,
                uiContainerComponent,
                loadedScenes,
                worldState);

            // create root ui for scenes
            InternalUiContainer rootScene1Container = new InternalUiContainer();
            InternalUiContainer rootScene2Container = new InternalUiContainer();
            uiContainerComponent.PutFor(scene1, SpecialEntityId.SCENE_ROOT_ENTITY, rootScene1Container);
            uiContainerComponent.PutFor(scene2, SpecialEntityId.SCENE_ROOT_ENTITY, rootScene2Container);

            // do system update
            system.Update();

            // scene was not loaded yet
            Assert.IsFalse(uiDocument.rootVisualElement.Contains(rootScene1Container.rootElement));

            // set scenes as loaded and change current scene (just because)
            loadedScenes.Add(scene1);
            loadedScenes.Add(scene2);
            loadedScenes.Add(scene_without_ui);
            worldState.GetCurrentSceneNumber().Returns(scene2Number);

            // do system update
            system.Update();

            // ui document should have scene2 ui set
            Assert.IsTrue(uiDocument.rootVisualElement.Contains(rootScene2Container.rootElement));
            Assert.AreEqual(1, uiDocument.rootVisualElement.childCount);

            // change scene again
            worldState.GetCurrentSceneNumber().Returns(scene1Number);

            // do system update
            system.Update();

            // ui document should have scene1 ui set
            Assert.IsTrue(uiDocument.rootVisualElement.Contains(rootScene1Container.rootElement));
            Assert.AreEqual(1, uiDocument.rootVisualElement.childCount);

            // move to scene without ui
            worldState.GetCurrentSceneNumber().Returns(scene_without_uiId);

            // do system update
            system.Update();

            // prev scene ui should be removed
            Assert.IsFalse(uiDocument.rootVisualElement.Contains(rootScene1Container.rootElement));
            Assert.AreEqual(0, uiDocument.rootVisualElement.childCount);

            // back to scene1 (and then move to a not loaded scene)
            worldState.GetCurrentSceneNumber().Returns(scene1Number);
            system.Update();
            Assert.IsTrue(uiDocument.rootVisualElement.Contains(rootScene1Container.rootElement));
            Assert.AreEqual(1, uiDocument.rootVisualElement.childCount);

            // move to a not loaded scene
            worldState.GetCurrentSceneNumber().Returns(-1);

            // do system update
            system.Update();

            // prev scene ui should be removed
            Assert.IsFalse(uiDocument.rootVisualElement.Contains(rootScene1Container.rootElement));
            Assert.AreEqual(0, uiDocument.rootVisualElement.childCount);
        }
    }
}