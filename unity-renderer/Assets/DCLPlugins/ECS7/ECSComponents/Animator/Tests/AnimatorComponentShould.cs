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
    public class AnimatorComponentShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private AnimatorComponentHandler componentHandler;
        private GameObject gameObject;
        private GameObject parentGameObject;
        private DataStore_ECS7 dataStoreEcs7;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            parentGameObject = new GameObject();
            gameObject.transform.SetParent(parentGameObject.transform);
            
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            dataStoreEcs7 = new DataStore_ECS7();
            componentHandler = new AnimatorComponentHandler(dataStoreEcs7);

            
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
            GameObject.Destroy(parentGameObject);
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Arrange
            entity.gameObject.AddComponent<Animation>();

            // Act
            componentHandler.Initialize(entity);

            // Assert
            Assert.IsNotNull(componentHandler.animComponent);
        }
        
        [Test]
        public void ReactToShapeReadyCorrectly()
        {
            // Arrange
            PBAnimator model = CreateModel();
            entity.gameObject.AddComponent<Animation>();
            componentHandler.animComponent = null;
            componentHandler.isShapeLoaded = false;
            componentHandler.OnComponentModelUpdated(scene,entity,model);
            
            // Act
            dataStoreEcs7.AddShapeReady(entity.entityId,entity.gameObject);

            // Assert
            Assert.IsNotNull(componentHandler.animComponent);
        }
        
        [Test]
        public void InitializateCorrectly()
        {
            // Arrange
            entity.gameObject.AddComponent<Animation>();
            
            // Act
            componentHandler.Initialize(entity);

            // Assert
            Assert.IsNotNull(componentHandler.animComponent);
        }

        [Test]
        public void SerializeCorrectly()
        {
            // Arrange
            PBAnimator model = CreateModel();
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
        
        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            PBAnimator model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);
            
            // Assert
            Assert.AreEqual(model.States, newModel.States);
        }

        private PBAnimator SerializaAndDeserialize(PBAnimator pbBox)
        {
            byte[] serialized;
            using(var memoryStream = new MemoryStream())
            {
                pbBox.WriteTo(memoryStream);
                serialized = memoryStream.ToArray();
            }

            return PBAnimator.Parser.ParseFrom((byte[])serialized);
        }

        private PBAnimator CreateModel()
        {
            PBAnimator model = new PBAnimator();
            PBAnimationState animationState = new PBAnimationState();
            animationState.Clip = "animation:0";
            animationState.Name = "Test";
            animationState.Loop = true;
            animationState.Playing = true;
            animationState.Speed = 1f;
            animationState.Weight = 1f;
            
            model.States.Add(animationState);
            
            return model;
        }
    }
}
