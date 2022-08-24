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
    public class BillboardShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private BillboardComponentHandler componentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            componentHandler = new BillboardComponentHandler(DataStore.i.player,Substitute.For<IUpdateEventHandler>());

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
            componentHandler.OnComponentCreated(scene,entity);
            var currentRotation = gameObject.transform.rotation;
            CommonScriptableObjects.cameraPosition.Set(new UnityEngine.Vector3(10, 10, 10));
                
            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreNotEqual(currentRotation,  gameObject.transform.rotation);
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);

            // Assert
            Assert.AreEqual(model.X, newModel.X);
            Assert.AreEqual(model.Y, newModel.Y);
            Assert.AreEqual(model.Z, newModel.Z);
        }

        private PBBillboard SerializaAndDeserialize(PBBillboard pb)
        {
            var serialized = BillboardSerializer.Serialize(pb);
            return BillboardSerializer.Deserialize(serialized);
        }

        private PBBillboard CreateModel()
        {
            PBBillboard model = new PBBillboard();
            model.X = true;
            model.Y = false;
            model.Z = false;
            return model;
        }
    }
}
