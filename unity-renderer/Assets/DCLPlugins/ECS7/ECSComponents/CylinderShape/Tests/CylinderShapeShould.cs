using System.IO;
using DCL.Controllers;
using DCL.Models;
using Google.Protobuf;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ECSComponents.Test
{
    public class CylinderShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSCylinderShapeComponentHandler componentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            componentHandler = new ECSCylinderShapeComponentHandler(DataStore.i.ecs7);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
            
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
            PBCylinderShape model = new PBCylinderShape();
            
            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(componentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
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
            PBCylinderShape model = new PBCylinderShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.meshesInfo = null;

            // Act
            componentHandler.DisposeMesh(entity, scene);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullPromiseCorrectly()
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.primitiveMeshPromisePrimitive = null;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
        }
        
        [Test]
        public void DisposeMeshWithNullMeshInfoCorrectly()
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.meshesInfo = null;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
            Assert.IsTrue(componentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void DisposeMeshWithNullRendereableCorrectly()
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.rendereable = null;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(componentHandler.meshesInfo);
            Assert.IsNull(componentHandler.rendereable);
            Assert.IsTrue(componentHandler.primitiveMeshPromisePrimitive.isForgotten);
        }
        
        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
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
        
        [TestCase(false,false,false, 1f,2f)]
        [TestCase(false,true,false , 1f,1f)]
        [TestCase(false,false,true, 0f,0f)]
        public void SerializeAndDeserialzeCorrectly(bool visible, bool withCollision, bool isPointerBlocker, float radiusBottom, float radiusTop)
        {
            // Arrange
            PBCylinderShape model = new PBCylinderShape();
            model.Visible = visible;
            model.WithCollisions = withCollision;
            model.IsPointerBlocker = isPointerBlocker;
            model.RadiusBottom = radiusBottom;
            model.RadiusTop = radiusTop;

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.WithCollisions, newModel.WithCollisions);
            Assert.AreEqual(model.IsPointerBlocker, newModel.IsPointerBlocker);
            Assert.AreEqual(model.RadiusBottom, newModel.RadiusBottom);
            Assert.AreEqual(model.RadiusTop, newModel.RadiusTop);
        }

        private PBCylinderShape SerializaAndDeserialize(PBCylinderShape pb)
        {
            byte[] serialized;
            using(var memoryStream = new MemoryStream())
            {
                pb.WriteTo(memoryStream);
                serialized = memoryStream.ToArray();
            }

            return PBCylinderShape.Parser.ParseFrom((byte[])serialized);
        }
    }
}
