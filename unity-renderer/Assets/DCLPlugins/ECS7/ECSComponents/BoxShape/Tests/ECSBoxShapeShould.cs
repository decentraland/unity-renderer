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
            boxShapeComponentHandler = new ECSBoxShapeComponentHandler(DataStore.i.ecs7);

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
            boxShapeComponentHandler.DisposeMesh(entity,scene);

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
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBBoxShape model = new PBBoxShape();
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
            PBBoxShape model = new PBBoxShape();
            model.Visible = visible;
            model.WithCollisions = withCollision;
            model.IsPointerBlocker = isPointerBlocker;
            float[] uvs = new float[]
            {
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1
            };
            model.Uvs.Add(uvs.ToList());

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.WithCollisions, newModel.WithCollisions);
            Assert.AreEqual(model.IsPointerBlocker, newModel.IsPointerBlocker);
            Assert.AreEqual(model.Uvs, newModel.Uvs);
        }

        private PBBoxShape SerializaAndDeserialize(PBBoxShape pbBox)
        {
            byte[] serialized;
            using(var memoryStream = new MemoryStream())
            {
                pbBox.WriteTo(memoryStream);
                serialized = memoryStream.ToArray();
            }

            return PBBoxShape.Parser.ParseFrom((byte[])serialized);
        }
    }
}
