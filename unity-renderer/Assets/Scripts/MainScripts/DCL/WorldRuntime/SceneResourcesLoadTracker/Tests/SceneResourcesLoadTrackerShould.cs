using DCL.Controllers;
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
        private SceneResourcesLoadTracker resourcesLoadTracker;
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
            resourcesLoadTracker.OnResourcesLoaded += () =>
            {
                resourceLoaded = true;
            };
            DataStore.i.ecs7.AddPendingResource(parcelScene.sceneData.sceneNumber, testModel);

            // Act
            DataStore.i.ecs7.RemovePendingResource(parcelScene.sceneData.sceneNumber, testModel);

            // Assert
            Assert.IsTrue(resourceLoaded);
        }

        [Test]
        public void NotWaitIfNoResources()
        {
            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }

        [Test]
        public void WaitForAllResourcesToBeReady()
        {
            // Arrange
            string newModel = "NewModel";
            DataStore.i.ecs7.AddPendingResource(parcelScene.sceneData.sceneNumber, testModel);
            DataStore.i.ecs7.AddPendingResource(parcelScene.sceneData.sceneNumber, newModel);
            Assert.IsTrue(resourcesLoadTracker.ShouldWaitForPendingResources());
            
            // Act
            DataStore.i.ecs7.RemovePendingResource(parcelScene.sceneData.sceneNumber, testModel);
            Assert.IsTrue(resourcesLoadTracker.ShouldWaitForPendingResources());
            DataStore.i.ecs7.RemovePendingResource(parcelScene.sceneData.sceneNumber, newModel);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }
    }
}