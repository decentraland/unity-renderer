using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using DCL.WorldRuntime;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Test
{
    public class SceneResourcesLoadTrackerShould
    {
        private SceneLoadTracker loadTracker;
        private IParcelScene parcelScene;
        private IDCLEntity entity;
        private GameObject gameObject;
        private const string testModel = "TestModel";

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
            loadTracker = new SceneLoadTracker();
            loadTracker.Track(sceneData.id);
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObject);
            DataStore.i.ecs7.pendingSceneResources.Clear();
        }

        [Test]
        public void DetectLoadOfResourcesCorrectly()
        {
            // Arrange
            bool resourceLoaded = false;
            loadTracker.OnResourcesLoaded += () =>
            {
                resourceLoaded = true;
            };
            DataStore.i.ecs7.pendingSceneResources.IncreaseRefCount((parcelScene.sceneData.id, testModel));

            // Act
            DataStore.i.ecs7.pendingSceneResources.DecreaseRefCount((parcelScene.sceneData.id, testModel));

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
        public void WaitForAllResourcesToBeReady()
        {
            // Arrange
            string newModel = "NewModel";
            DataStore.i.ecs7.pendingSceneResources.IncreaseRefCount((parcelScene.sceneData.id, testModel));
            DataStore.i.ecs7.pendingSceneResources.IncreaseRefCount((parcelScene.sceneData.id, newModel));
            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());
            
            // Act
            DataStore.i.ecs7.pendingSceneResources.DecreaseRefCount((parcelScene.sceneData.id, testModel));
            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());
            DataStore.i.ecs7.pendingSceneResources.DecreaseRefCount((parcelScene.sceneData.id, newModel));

            // Assert
            Assert.IsFalse(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
        }
    }
}