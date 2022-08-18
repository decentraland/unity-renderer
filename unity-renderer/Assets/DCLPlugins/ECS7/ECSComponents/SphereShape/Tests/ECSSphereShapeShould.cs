using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSSphereShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private GameObject gameObject;
        private ECSSphereShapeComponentHandler sphereShapeComponentHandler;
        private IInternalECSComponent<InternalTexturizable> texurizableInternalComponent;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            texurizableInternalComponent = Substitute.For<IInternalECSComponent<InternalTexturizable>>();
            sphereShapeComponentHandler = new ECSSphereShapeComponentHandler(DataStore.i.ecs7, texurizableInternalComponent);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
            sphereShapeComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            sphereShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();

            // Act
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            var meshesInfo = sphereShapeComponentHandler.meshesInfo;
            texurizableInternalComponent.Received(1).PutFor(scene, entity, 
                Arg.Is<InternalTexturizable>(x => meshesInfo.renderers.All(r => x.renderers.Contains(r))));            

            // Assert
            Assert.IsNotNull(sphereShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            texurizableInternalComponent.ClearReceivedCalls();

            // Act
            sphereShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
            texurizableInternalComponent.Received(1).PutFor(scene, entity, 
                Arg.Is<InternalTexturizable>(x => x.renderers.Count == 0));
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            sphereShapeComponentHandler.meshesInfo = null;

            // Act
            sphereShapeComponentHandler.DisposeMesh(entity, scene);

            // Assert
            Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
            Assert.IsNull(sphereShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            sphereShapeComponentHandler.primitiveMeshPromisePrimitive = null;

            // Act
            sphereShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
            Assert.IsNull(sphereShapeComponentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            sphereShapeComponentHandler.meshesInfo = null;

            // Act
            sphereShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
            Assert.IsNull(sphereShapeComponentHandler.rendereable);
            Assert.IsTrue(sphereShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            PBSphereShape model = new PBSphereShape();
            sphereShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);
            sphereShapeComponentHandler.rendereable = null;

            // Act
            sphereShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(sphereShapeComponentHandler.meshesInfo);
            Assert.IsNull(sphereShapeComponentHandler.rendereable);
            Assert.IsTrue(sphereShapeComponentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
    }
}
