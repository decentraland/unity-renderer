using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.OnPointerDown;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace DCLPlugins.ECSComponents.Test
{
    public class OnPointerDownComponentHandlerShould
    {
        private OnPointerDownComponentHandler componentHandler;
        private DataStore_ECS7 dataStoreEcs7;
        
        [SetUp]
        public void SetUp()
        {
            dataStoreEcs7 = new DataStore_ECS7();
            componentHandler = new OnPointerDownComponentHandler(Substitute.For<IECSComponentWriter>(), dataStoreEcs7, Substitute.For<IECSContext>());
        }

        [Test]
        public void CreatePointerInputRepresentantionCorrectly()
        {
            // Arrange
            int entityId = 513;
            
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(entityId);
            
            var pointerEvent = new PBOnPointerDown();
            pointerEvent.Button = ActionButton.Action3;
            pointerEvent.HoverText = "Test text";
            pointerEvent.MaxDistance = 15;
            pointerEvent.ShowFeedback = true;

            componentHandler.OnComponentCreated(parcelScene, entity);
            
            // Act
            componentHandler.OnComponentModelUpdated(parcelScene, entity, pointerEvent);
            
            // Assert
            var representantion = componentHandler.representantion;
            
            Assert.AreEqual(PointerInputEventType.DOWN ,representantion.GetEventType());
            Assert.AreEqual(pointerEvent.Button, representantion.button);
            Assert.AreEqual(pointerEvent.HoverText, representantion.hoverText);
            Assert.AreEqual(pointerEvent.MaxDistance, representantion.distance);
            Assert.AreEqual(pointerEvent.ShowFeedback, representantion.showFeedback);
        }
        
        [Test]
        public void AddToDataStoreCorrectly()
        {
            // Arrange
            int entityId = 513;
            
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(entityId);
            
            componentHandler.OnComponentCreated(parcelScene, entity);
            componentHandler.OnComponentModelUpdated(parcelScene, entity, new PBOnPointerDown());
            
            // Act
            componentHandler.OnComponentModelUpdated(parcelScene, entity, new PBOnPointerDown());
            
            // Assert
            Assert.IsTrue(dataStoreEcs7.entityEvents.ContainsKey(entityId));
            Assert.AreEqual(1,dataStoreEcs7.entityEvents[entityId].Count);
            Assert.AreEqual(1,dataStoreEcs7.entityEvents.Count());
        }

        [Test]
        public void RemoveFromDataStoreCorrectly()
        {
            // Arrange
            int entityId = 513;
            
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(entityId);
            
            componentHandler.OnComponentCreated(parcelScene, entity);
            componentHandler.OnComponentModelUpdated(parcelScene, entity, new PBOnPointerDown());
            
            // Act
            componentHandler.OnComponentRemoved(parcelScene, entity);
            
            // Assert
            Assert.IsFalse(dataStoreEcs7.entityEvents.ContainsKey(entityId));
            Assert.AreEqual(0,dataStoreEcs7.entityEvents.Count());
        }
    }
}