using System.Collections;
using DCL;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
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
            handler = new MeshRendererHandler(DataStore.i.ecs7, Substitute.For<IInternalECSComponent<InternalTexturizable>>(), Substitute.For<IInternalECSComponent<InternalRenderers>>());
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
            handler.OnComponentModelUpdated(parcelScene, entity, CreateBoxMesh());

            // Assert
            Assert.IsTrue(resourceLoaded);
        }

        [Test]
        public void IgnoreComponentAfterDisposed()
        {
            // Arrange
            handler.OnComponentCreated(parcelScene, entity);
            
            // Act
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
            var model = CreateBoxMesh();
            var model2 = CreateBoxMesh();

            // Act
            handler.OnComponentModelUpdated(parcelScene, entity,model);
            handler.OnComponentModelUpdated(parcelScene, entity,model2);

            // Assert
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }
        

        // Helper
        private PBMeshRenderer CreateBoxMesh()
        {
            var meshRenderer = new PBMeshRenderer();
            meshRenderer.Box = new PBMeshRenderer.Types.BoxMesh();
            return meshRenderer;
        }
    }
}