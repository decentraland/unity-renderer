using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSPlaneShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSPlaneShapeComponentHandler planeShapeComponentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            planeShapeComponentHandler = new ECSPlaneShapeComponentHandler();

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            planeShapeComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();

            // Act
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(planeShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            planeShapeComponentHandler.meshesInfo = null;

            // Act
            planeShapeComponentHandler.DisposeMesh(scene);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            Assert.IsNull(planeShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            planeShapeComponentHandler.primitiveMeshPromisePrimitive = null;

            // Act
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            Assert.IsNull(planeShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            planeShapeComponentHandler.meshesInfo = null;

            // Act
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            Assert.IsNull(planeShapeComponentHandler.rendereable);
            Assert.IsTrue(planeShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            ECSPlaneShape model = new ECSPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            planeShapeComponentHandler.rendereable = null;

            // Act
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            Assert.IsNull(planeShapeComponentHandler.rendereable);
            Assert.IsTrue(planeShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
    }
}
