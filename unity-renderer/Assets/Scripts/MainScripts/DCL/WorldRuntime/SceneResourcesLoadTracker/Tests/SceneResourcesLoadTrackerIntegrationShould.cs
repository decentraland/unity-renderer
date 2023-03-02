using DCL.Controllers;
using DCL.Models;
using DCL.WorldRuntime;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.Tests
{
    public class SceneResourcesLoadTrackerIntegrationShould
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
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.Destroy(gameObject);
            DataStore.i.ecs7.pendingSceneResources.Clear();
        }

        [Test]
        public void ChangeSceneLoadTrackerCorrectly()
        {
            // Arrange
            resourcesLoadTracker = new SceneResourcesLoadTracker();
            resourcesLoadTracker.Track(new ECSComponentsManagerLegacy(parcelScene), Environment.i.world.state);
            
            // Act
            resourcesLoadTracker.Track(parcelScene.sceneData.sceneNumber);
            
            // Assert
            Assert.IsTrue(resourcesLoadTracker.tracker is ResourcesLoadTrackerECS);
        }
        
        [Test]
        public void MantainSceneLoadTrackerCorrectly()
        {
            // Arrange
            resourcesLoadTracker = new SceneResourcesLoadTracker();
            
            // Act
            resourcesLoadTracker.Track(new ECSComponentsManagerLegacy(parcelScene), Environment.i.world.state);
            
            // Assert
            Assert.IsTrue(resourcesLoadTracker.tracker is ResourcesLoadTrackerLegacyECS);
        }
    }
}