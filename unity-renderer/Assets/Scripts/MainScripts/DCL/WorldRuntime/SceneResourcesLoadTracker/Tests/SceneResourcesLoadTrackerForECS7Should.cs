using System.Collections;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.WorldRuntime;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneResourcesLoadTrackerForECS7Should
    {
        private SceneResourcesLoadTracker resourcesLoadTracker;
        private MeshRendererHandler handler;

        private IParcelScene parcelScene;
        private IDCLEntity entity;
        private GameObject gameObject;

        [SetUp]
        public void SetUp()
        {
            // Configure Scene
            parcelScene = Substitute.For<IParcelScene>();
            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.sceneNumber = 666;
            parcelScene.Configure().sceneData.Returns(sceneData);

            // Configure entity
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            entity.Configure().gameObject.Returns(gameObject);
            entity.Configure().entityId.Returns(5555);

            // Create components
            resourcesLoadTracker = new SceneResourcesLoadTracker();
            resourcesLoadTracker.Track(sceneData.sceneNumber);

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            handler = new MeshRendererHandler(DataStore.i.ecs7, internalComponents.texturizableComponent, internalComponents.renderersComponent);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObject);
            handler.OnComponentRemoved(parcelScene,entity);
            DataStore.i.ecs7.pendingSceneResources.Clear();
        }

        [Test]
        public void DetectLoadOfComponentCorrectly()
        {
            // Arrange
            bool resourceLoaded = false;
            resourcesLoadTracker.OnResourcesLoaded += () =>
            {
                resourceLoaded = true;
            };

            // Act
            handler.OnComponentCreated(parcelScene, entity);
            handler.OnComponentModelUpdated(parcelScene, entity, new PBMeshRenderer() { Box = new PBMeshRenderer.Types.BoxMesh() });

            // Assert
            Assert.IsTrue(resourceLoaded);
        }

        [Test]
        public void IgnoreComponentAfterDisposed()
        {
            // Act
            handler.OnComponentCreated(parcelScene, entity);
            handler.OnComponentRemoved(parcelScene, entity);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }

        [Test]
        public void WaitForAllComponentsToBeReady()
        {
            // Arrange
            var model = new PBMeshRenderer() { Box = new PBMeshRenderer.Types.BoxMesh() };
            var model2 = new PBMeshRenderer() { Cylinder = new PBMeshRenderer.Types.CylinderMesh() };

            // Act
            handler.OnComponentCreated(parcelScene, entity);
            handler.OnComponentModelUpdated(parcelScene, entity, model);
            handler.OnComponentModelUpdated(parcelScene, entity, model2);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }
    }
}
