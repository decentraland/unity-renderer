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
        private ECSBoxShapeComponentHandler boxShapeComponentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            boxShapeComponentHandler = new ECSBoxShapeComponentHandler();

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            boxShapeComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();

            // Act
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(boxShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            boxShapeComponentHandler.meshesInfo = null;

            // Act
            boxShapeComponentHandler.DisposeMesh(scene);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
            Assert.IsNull(boxShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            boxShapeComponentHandler.primitiveMeshPromisePrimitive = null;

            // Act
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
            Assert.IsNull(boxShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            boxShapeComponentHandler.meshesInfo = null;

            // Act
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
            Assert.IsNull(boxShapeComponentHandler.rendereable);
            Assert.IsTrue(boxShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            boxShapeComponentHandler.rendereable = null;

            // Act
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
            Assert.IsNull(boxShapeComponentHandler.rendereable);
            Assert.IsTrue(boxShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
    }
}
