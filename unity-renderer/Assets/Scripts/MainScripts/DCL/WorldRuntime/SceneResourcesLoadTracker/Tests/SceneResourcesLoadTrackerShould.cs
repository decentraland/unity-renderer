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
    public class SceneResourcesLoadTrackerShould
    {
        private IECSComponentsManagerLegacy componentsManager;
        private SceneLoadTracker loadTracker;
        private BaseCollection<IECSResourceLoaderTracker> baseList;
        private BoxShapeComponentHandler hanlder;
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
            componentsManager = new ECSComponentsManagerLegacy(parcelScene);
            loadTracker = new SceneLoadTracker();
            baseList = new BaseCollection<IECSResourceLoaderTracker>();
            loadTracker.Track(baseList);
            hanlder = new BoxShapeComponentHandler();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObject);
            hanlder.OnComponentRemoved(parcelScene,entity);
            DataStore.i.ecs7.RemoveResourceTracker("IdTest", hanlder);
        }

        [Test]
        public void DetectLoadOfResourcesCorrectly()
        {
            // Arrange
            baseList.Add(hanlder);
            bool resourceLoaded = false;
            loadTracker.OnResourcesLoaded += () =>
            {
                resourceLoaded = true;
            };
            hanlder.OnComponentCreated(parcelScene, entity);
            
            // Act
            hanlder.OnComponentModelUpdated(parcelScene, entity,new ECSBoxShape());

            // Assert
            Assert.IsTrue(resourceLoaded);
        }

        [Test]
        public void NotWaitIfNoResources()
        {
            // Assert
            Assert.IsFalse(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
        }

        [Test]
        public void IgnoreResourcesAfterDisposed()
        {
            // Arrange
            hanlder.OnComponentCreated(parcelScene, entity);
            
            // Act
            hanlder.OnComponentRemoved(parcelScene, entity);

            // Assert
            Assert.IsFalse(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
        }
        
        [Test]
        public void WaitForAllComponentsToBeReady()
        {
            // Arrange
            baseList.Add(hanlder);
            hanlder.OnComponentCreated(parcelScene, entity);
            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());
            
            // Act
            hanlder.OnComponentModelUpdated(parcelScene, entity,new ECSBoxShape());

            // Assert
            Assert.IsFalse(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
        }
    }
}