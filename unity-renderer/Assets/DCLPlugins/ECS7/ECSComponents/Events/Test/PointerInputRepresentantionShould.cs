using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using Ray = UnityEngine.Ray;

namespace DCLPlugins.ECSComponents.Events.Test
{
    public class PointerInputRepresentantionShould
    {
        private PointerInputRepresentation pointerInputRepresentation;
        private IDCLEntity entity;
        private Queue<PointerEvent> pendingsPointerEvents;
        private IOnPointerEventHandler onPointerEventHandler;
        
        [SetUp]
        public void SetUp()
        {
            onPointerEventHandler = Substitute.For<IOnPointerEventHandler>();
            
            entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(512);
            pendingsPointerEvents = new Queue<PointerEvent>();
            pointerInputRepresentation = new PointerInputRepresentation(entity, new DataStore_ECS7(), PointerEventType.Up,Substitute.For<IECSComponentWriter>(), onPointerEventHandler, pendingsPointerEvents);
        }

        [TearDown]
        public void TearDown()
        {
            PointerInputRepresentation.lamportTimestamp = 0;
        }

        [Test]
        public void AssignCorrectEnums()
        {
            // Assert
            Assert.AreEqual(PointerEventType.Up, pointerInputRepresentation.pointerEventType);
            Assert.AreEqual(PointerInputEventType.UP, pointerInputRepresentation.GetEventType());
        }

        [Test]
        public void SetDataCorrectly()
        {
            // Arrange
            var parcelScene = Substitute.For<IParcelScene>();
            
            bool showFeedback = true;
            string hoverText = "This test is a hover test one";
            ActionButton button = ActionButton.Backward;
            float distance = 16;
            
            // Act
            pointerInputRepresentation.SetData(parcelScene, entity, showFeedback, button, distance, hoverText);
            
            // Arrange
            Assert.AreEqual(entity, pointerInputRepresentation.eventEntity);
            Assert.AreEqual(button, pointerInputRepresentation.button);
            Assert.AreEqual(hoverText, pointerInputRepresentation.hoverText);
            Assert.AreEqual(showFeedback, pointerInputRepresentation.showFeedback);
            Assert.IsTrue(Mathf.Approximately(distance, pointerInputRepresentation.distance));
        }

        [Test]
        public void ReportEventCorrectly()
        {
            // Arrange
            onPointerEventHandler.Configure().GetMeshName(Arg.Any<Collider>()).Returns("TestMesh");
            string sceneId = "TestIdScene"; 
            var parcelScene = Substitute.For<IParcelScene>();
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = sceneId;
            parcelScene.Configure().sceneData.Returns(sceneData);
            
            bool showFeedback = true;
            string hoverText = "This test is a hover test one";
            ActionButton button = ActionButton.Backward;
            float distance = 16;
            pointerInputRepresentation.SetData(parcelScene, entity, showFeedback, button, distance, hoverText);

            var ray = new Ray(UnityEngine.Vector3.zero, UnityEngine.Vector3.back);
            var hitInfo = new HitInfo();

            // Act
            pointerInputRepresentation.ReportEvent(WebInterface.ACTION_BUTTON.ACTION_3, ray, hitInfo);
            
            // Assert
            Assert.AreEqual(1, pendingsPointerEvents.Count);

            var pointerEvent = pendingsPointerEvents.Dequeue();

            Assert.AreEqual(pointerEvent.button, ActionButton.Action3);
            Assert.AreEqual(pointerEvent.sceneId, sceneId);
            Assert.AreEqual(pointerEvent.type, PointerEventType.Up);
            Assert.AreEqual(pointerEvent.timestamp, PointerInputRepresentation.lamportTimestamp-1);
            
            var hit = pointerEvent.hit;
            var result = ProtoConvertUtils.ToPBRaycasHit(512, "TestMesh", ray, hitInfo);
            
            Assert.AreEqual(hit,result);
        }
    }
}