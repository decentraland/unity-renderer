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
    public class ECSBoxShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSBoxShapeComponentHandler ecsBoxShapeComponentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            ecsBoxShapeComponentHandler = new ECSBoxShapeComponentHandler();

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            ecsBoxShapeComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            ecsBoxShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();

            // Act
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(ecsBoxShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            ecsBoxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(ecsBoxShapeComponentHandler.meshesInfo);
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            ecsBoxShapeComponentHandler.meshesInfo = null;

            // Act
            ecsBoxShapeComponentHandler.DisposeMesh(scene);

            // Assert
            Assert.IsNull(ecsBoxShapeComponentHandler.meshesInfo);
            Assert.IsNull(ecsBoxShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            ecsBoxShapeComponentHandler.primitiveMeshPromisePrimitive = null;

            // Act
            ecsBoxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(ecsBoxShapeComponentHandler.meshesInfo);
            Assert.IsNull(ecsBoxShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            ecsBoxShapeComponentHandler.meshesInfo = null;

            // Act
            ecsBoxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(ecsBoxShapeComponentHandler.meshesInfo);
            Assert.IsNull(ecsBoxShapeComponentHandler.rendereable);
            Assert.IsTrue(ecsBoxShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            ecsBoxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            ecsBoxShapeComponentHandler.rendereable = null;

            // Act
            ecsBoxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(ecsBoxShapeComponentHandler.meshesInfo);
            Assert.IsNull(ecsBoxShapeComponentHandler.rendereable);
            Assert.IsTrue(ecsBoxShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
    }
}
