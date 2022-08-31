using System.Collections.Generic;
using DCL;
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
        private PointerInputRepresentantion pointerInputRepresentantion;
        private IDCLEntity entity;
        private Queue<PointerEvent> pendingsPointerEvents;
        
        [SetUp]
        public void SetUp()
        {
            entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(512);
            pendingsPointerEvents = new Queue<PointerEvent>();
            pointerInputRepresentantion = new PointerInputRepresentantion(entity, new DataStore_ECS7(), PointerEventType.Up,Substitute.For<IECSComponentWriter>(),new Queue<PointerEvent>());
        }

        [TearDown]
        public void TearDown()
        {
            PointerInputRepresentantion.lamportTimestamp = 0;
        }

        [Test]
        public void AssignCorrectEnums()
        {
            // Assert
            Assert.AreEqual(PointerEventType.Up, pointerInputRepresentantion.pointerEventType);
            Assert.AreEqual(PointerInputEventType.UP, pointerInputRepresentantion.GetEventType());
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
            pointerInputRepresentantion.SetData(parcelScene, entity, showFeedback, button, distance, hoverText);
            
            // Arrange
            Assert.AreEqual(entity, pointerInputRepresentantion.eventEntity);
            Assert.AreEqual(button, pointerInputRepresentantion.button);
            Assert.AreEqual(hoverText, pointerInputRepresentantion.hoverText);
            Assert.AreEqual(showFeedback, pointerInputRepresentantion.showFeedback);
            Assert.IsTrue(Mathf.Approximately(distance, pointerInputRepresentantion.distance));
        }

        [Test]
        public void ReportEventCorrectly()
        {
            // Arrange
            string sceneId = "TestIdScene"; 
            var parcelScene = Substitute.For<IParcelScene>();
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = sceneId;
            parcelScene.Configure().sceneData.Returns(sceneData);
            
            bool showFeedback = true;
            string hoverText = "This test is a hover test one";
            ActionButton button = ActionButton.Backward;
            float distance = 16;
            pointerInputRepresentantion.SetData(parcelScene, entity, showFeedback, button, distance, hoverText);

            var ray = new Ray(UnityEngine.Vector3.zero, UnityEngine.Vector3.back);
            var hitInfo = new HitInfo();

            // Act
            pointerInputRepresentantion.ReportEvent(WebInterface.ACTION_BUTTON.ACTION_3, ray, hitInfo);
            
            // 
            Assert.AreEqual(1, pendingsPointerEvents.Count);

            var pointerEvent = pendingsPointerEvents.Dequeue();

            Assert.AreEqual(pointerEvent.button, ActionButton.Action3);
            Assert.AreEqual(pointerEvent.sceneId, sceneId);
            Assert.AreEqual(pointerEvent.type, PointerEventType.Up);
            Assert.AreEqual(pointerEvent.timestamp, PointerInputRepresentantion.lamportTimestamp-1);
            
            var hit = pointerEvent.hit;
            var result = ProtoConvertUtils.ToPBRaycasHit(512, null, ray, hitInfo);
            
            Assert.AreEqual(hit,result);
        }
    }
}