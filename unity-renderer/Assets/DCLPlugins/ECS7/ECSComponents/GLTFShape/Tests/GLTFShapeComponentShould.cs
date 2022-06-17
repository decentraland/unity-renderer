using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Controllers;
using DCL.Models;
using Google.Protobuf;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class GLTFShapeComponentShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private GLTFShapeComponentHandler componentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            componentHandler = new GLTFShapeComponentHandler(DataStore.i.ecs7);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            ContentProvider contentProvider = new ContentProvider();
            scene.Configure().contentProvider.Returns(contentProvider);
            
            componentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        protected void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            componentHandler.meshesInfo = new MeshesInfo();
            componentHandler.meshesInfo.meshRootGameObject = entity.gameObject;
            componentHandler.meshesInfo.colliders = new List<Collider>();

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(componentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
        }
        
        [Test]
        public void DisposeMeshorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.meshesInfo = null;

            // Act
            componentHandler.DisposeMesh(scene);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
        }

        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.meshesInfo = null;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.rendereable = null;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
        }
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            byte[] byteArray;
            
            // Act
            using(var memoryStream = new MemoryStream())
            {
                model.WriteTo(memoryStream);
                byteArray = memoryStream.ToArray();
            }

            // Assert
            Assert.IsNotNull(byteArray);
        }
        
        [TestCase(false,false,false)]
        [TestCase(false,true,false)]
        [TestCase(false,false,true)]
        public void SerializeAndDeserialzeCorrectly(bool visible, bool withCollision, bool isPointerBlocker)
        {
            // Arrange
            PBGLTFShape model = new PBGLTFShape();
            model.Visible = visible;
            model.WithCollisions = withCollision;
            model.IsPointerBlocker = isPointerBlocker;
            model.Src = "TestSrc";

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.WithCollisions, newModel.WithCollisions);
            Assert.AreEqual(model.IsPointerBlocker, newModel.IsPointerBlocker);
            Assert.AreEqual(model.Src, newModel.Src);
        }

        private PBGLTFShape SerializaAndDeserialize(PBGLTFShape pbBox)
        {
            byte[] serialized;
            using(var memoryStream = new MemoryStream())
            {
                pbBox.WriteTo(memoryStream);
                serialized = memoryStream.ToArray();
            }

            return PBGLTFShape.Parser.ParseFrom((byte[])serialized);
        }
    }
}
