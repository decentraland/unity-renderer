using System.Collections;
using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.WorldRuntime;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SceneResourcesLoadTrackerForECSShould
    {
        private SceneResourcesLoadTracker resourcesLoadTracker;
        private ECSBoxShapeComponentHandler hanlder;
        private IParcelScene parcelScene;
        private IDCLEntity entity;
        private GameObject gameObject;

        [SetUp]
        public void SetUp()
        {
            // Configure Scene
            parcelScene = Substitute.For<IParcelScene>();
            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "IdTest";
            parcelScene.Configure().sceneData.Returns(sceneData);
            
            // Configure entity
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            entity.Configure().gameObject.Returns(gameObject);
            entity.Configure().entityId.Returns(5555);
            
            // Create components
            resourcesLoadTracker = new SceneResourcesLoadTracker();
            resourcesLoadTracker.Track(sceneData.id);
            hanlder = new ECSBoxShapeComponentHandler();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObject);
            hanlder.OnComponentRemoved(parcelScene,entity);
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
            hanlder.OnComponentModelUpdated(parcelScene, entity,new ECSBoxShape());

            // Assert
            Assert.IsTrue(resourceLoaded);
        }

        [Test]
        public void IgnoreComponentAfterDisposed()
        {
            // Arrange
            hanlder.OnComponentCreated(parcelScene, entity);
            
            // Act
            hanlder.OnComponentRemoved(parcelScene, entity);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }
        
        [Test]
        public void WaitForAllComponentsToBeReady()
        {
            // Arrange
            var model = new ECSBoxShape();
            var model2 = new ECSBoxShape();

            // Act
            hanlder.OnComponentModelUpdated(parcelScene, entity,model);
            hanlder.OnComponentModelUpdated(parcelScene, entity,model2);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }
    }
}