using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
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
        private IInternalECSComponent<InternalTexturizable> texurizableInternalComponent;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            texurizableInternalComponent = Substitute.For<IInternalECSComponent<InternalTexturizable>>();
            planeShapeComponentHandler = new ECSPlaneShapeComponentHandler(DataStore.i.ecs7, texurizableInternalComponent);

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
            PBPlaneShape model = new PBPlaneShape();

            // Act
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            var meshesInfo = planeShapeComponentHandler.meshesInfo;
            texurizableInternalComponent.Received(1).PutFor(scene, entity, 
                Arg.Is<InternalTexturizable>(x => meshesInfo.renderers.All(r => x.renderers.Contains(r))));            

            // Assert
            Assert.IsNotNull(planeShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBPlaneShape model = new PBPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            texurizableInternalComponent.ClearReceivedCalls();

            // Act
            planeShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            texurizableInternalComponent.Received(1).PutFor(scene, entity, 
                Arg.Is<InternalTexturizable>(x => x.renderers.Count == 0));
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            PBPlaneShape model = new PBPlaneShape();
            planeShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            planeShapeComponentHandler.meshesInfo = null;

            // Act
            planeShapeComponentHandler.DisposeMesh(entity, scene);

            // Assert
            Assert.IsNull(planeShapeComponentHandler.meshesInfo);
            Assert.IsNull(planeShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            PBPlaneShape model = new PBPlaneShape();
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
            PBPlaneShape model = new PBPlaneShape();
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
            PBPlaneShape model = new PBPlaneShape();
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
