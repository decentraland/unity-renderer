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
using Assert = UnityEngine.Assertions.Assert;

namespace DCL.ECSComponents.Test
{
    public class ECSBoxShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSNFTShapeComponentHandler componentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            componentHandler = new ECSNFTShapeComponentHandler();

            var meshInfo = new MeshesInfo();
            entity.meshesInfo.Returns(meshInfo);
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
            var model = CreateModel();

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(entity.meshesInfo);
            Assert.IsNotNull(entity.meshesInfo.meshRootGameObject);
        }
        
        [Test]
        public void CreateLoaderCorrectly()
        {
            // Arrange
            var model = CreateModel();
            
            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(entity.meshRootGameObject.GetComponent<NFTShapeLoaderController>());
        }

        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBNFTShape model = new PBNFTShape();
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
            PBNFTShape model = new PBNFTShape();
            model.Visible = visible;
            model.WithCollisions = withCollision;
            model.IsPointerBlocker = isPointerBlocker;
            model.Color = new Color();
            model.Color.Red = 1;
            model.Color.Blue = 0;
            model.Color.Green = 0.5f;
            model.Src = "ethereum://test";

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.Visible, newModel.Visible);
            Assert.AreEqual(model.WithCollisions, newModel.WithCollisions);
            Assert.AreEqual(model.IsPointerBlocker, newModel.IsPointerBlocker);
            Assert.AreEqual(model.Src, newModel.Src);
            Assert.AreEqual(model.Color.Red, newModel.Color.Red);
            Assert.AreEqual(model.Color.Blue, newModel.Color.Blue);
            Assert.AreEqual(model.Color.Green, newModel.Color.Green);
        }

        private PBNFTShape CreateModel()
        {
            var model = new PBNFTShape();
            model.Src = "ethereum://test";
            return model;
        }
        
        private PBNFTShape SerializaAndDeserialize(PBNFTShape pb)
        {
            var result = NFTShapeSerializer.Serialize(pb);

            return NFTShapeSerializer.Deserialize(pb);
        }
    }
}
