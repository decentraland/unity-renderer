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
        private PointerEventsComponentHandler componentHandler;
        private DataStore_ECS7 dataStoreEcs7;
        
        [SetUp]
        public void SetUp()
        {
            dataStoreEcs7 = new DataStore_ECS7();
            componentHandler = new PointerEventsComponentHandler(Substitute.For<IECSComponentWriter>(), dataStoreEcs7, Substitute.For<IECSContext>());
        }

        [Test]
        public void CreatePointerInputRepresentantionCorrectly()
        {
            // Arrange
            int entityId = 513;
            
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(entityId);
            
            var pointerEvent = new PBPointerEvent();
            pointerEvent.Button = ActionButton.Action3;
            pointerEvent.HoverText = "Test text";
            pointerEvent.MaxDistance = 15;
            pointerEvent.ShowFeedback = true;
            
            PBPointerEventEntry pointerEventEntry = new PBPointerEventEntry();
            pointerEventEntry.EventType = PointerEventType.Down;
            pointerEventEntry.EventInfo = pointerEvent;

            componentHandler.OnComponentCreated(parcelScene, entity);
            
            // Act
            componentHandler.OnComponentModelUpdated(parcelScene, entity, CreateModel(pointerEventEntry));
            
            // Assert
            var representantion = componentHandler.pointerEventsDictionary[pointerEventEntry];
            
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
            componentHandler.OnComponentModelUpdated(parcelScene, entity, CreateModel());
            
            // Act
            componentHandler.OnComponentModelUpdated(parcelScene, entity, CreateModel());
            
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
            componentHandler.OnComponentModelUpdated(parcelScene, entity, CreateModel());
            
            // Act
            componentHandler.OnComponentRemoved(parcelScene, entity);
            
            // Assert
            Assert.IsFalse(dataStoreEcs7.entityEvents.ContainsKey(entityId));
            Assert.AreEqual(0,dataStoreEcs7.entityEvents.Count());
        }
        
        [Test]
        public void RemovePointerInputRepresentantionIfNoLongerExists()
        {
            // Integration test
            // Arrange
            int entityId = 513;
            
            var parcelScene = Substitute.For<IParcelScene>();
            var entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(entityId);
            
            var pointerEvent = new PBPointerEvent();
            pointerEvent.Button = ActionButton.Action3;
            pointerEvent.HoverText = "Test asd";
            pointerEvent.MaxDistance = 15;
            pointerEvent.ShowFeedback = true;          
            
            var pointerEvent2 = new PBPointerEvent();
            pointerEvent2.Button = ActionButton.Action3;
            pointerEvent2.HoverText = "Test text";
            pointerEvent2.MaxDistance = 12;
            pointerEvent2.ShowFeedback = true;
            
            PBPointerEventEntry pointerEventEntry = new PBPointerEventEntry();
            pointerEventEntry.EventType = PointerEventType.Up;
            pointerEventEntry.EventInfo = pointerEvent;
            
            PBPointerEventEntry pointerEventEntry2 = new PBPointerEventEntry();
            pointerEventEntry2.EventType = PointerEventType.Down;
            pointerEventEntry2.EventInfo = pointerEvent2;
            
            PBPointerEvents pointerEvents = new PBPointerEvents();
            pointerEvents.PointerEvents.Add(pointerEventEntry);
            pointerEvents.PointerEvents.Add(pointerEventEntry2);

            componentHandler.OnComponentCreated(parcelScene, entity);
            
            componentHandler.OnComponentModelUpdated(parcelScene, entity, pointerEvents);
            
            Assert.AreEqual(2,componentHandler.pointerEventsDictionary.Count );
            Assert.IsTrue(dataStoreEcs7.entityEvents.ContainsKey(entityId));
            Assert.AreEqual(2,dataStoreEcs7.entityEvents[entityId].Count);
            Assert.AreEqual(1,dataStoreEcs7.entityEvents.Count());
            
            // Act
            pointerEvents.PointerEvents.Remove(pointerEventEntry2);
            componentHandler.OnComponentModelUpdated(parcelScene, entity, pointerEvents);
            
            // Assert
            Assert.AreEqual(1,componentHandler.pointerEventsDictionary.Count );
            Assert.IsTrue(dataStoreEcs7.entityEvents.ContainsKey(entityId));
            Assert.AreEqual(1,dataStoreEcs7.entityEvents[entityId].Count);
            Assert.AreEqual(1,dataStoreEcs7.entityEvents.Count());
            Assert.AreEqual(PointerInputEventType.UP, dataStoreEcs7.entityEvents[entityId][0].GetEventType());
        }
        
        private PBPointerEvents CreateModel()
        {
            var pointerEvent = new PBPointerEvent();
            pointerEvent.Button = ActionButton.Action3;
            pointerEvent.HoverText = "Test text";
            pointerEvent.MaxDistance = 15;
            pointerEvent.ShowFeedback = true;
            
            PBPointerEventEntry pointerEventEntry = new PBPointerEventEntry();
            pointerEventEntry.EventType = PointerEventType.Up;
            pointerEventEntry.EventInfo = pointerEvent;
            
            return CreateModel(pointerEventEntry);
        }

        private PBPointerEvents CreateModel(PBPointerEventEntry pointerEventEntry)
        {
            PBPointerEvents pointerEvents = new PBPointerEvents();
            pointerEvents.PointerEvents.Add(pointerEventEntry);
            return pointerEvents;
        }
    }
}