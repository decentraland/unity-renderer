using System.Collections.Generic;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECS7.Systems.PointerEventResolver;
using DCLPlugins.ECSComponents.Events;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class ECSPointerEventResolverSystemShould
    {
        private const string SCENE_ID = "TestId";
        private IECSComponentWriter componentsWriter;
        private Queue<PointerEvent> pointerEventsQueue;

        [SetUp]
        public void SetUp()
        {
            componentsWriter = Substitute.For<IECSComponentWriter>();
            pointerEventsQueue = new Queue<PointerEvent>();
        }

        [Test]
        public void SendPointerEventResult()
        {
            // Arrange
            var update = ECSPointerEventResolverSystem.CreateSystem(componentsWriter);

            pointerEventsQueue.Enqueue(CreatePointerEvent());

            // Act
            update.Invoke();

            // Assert
            componentsWriter.Received(1)
                            .PutComponent(
                                SCENE_ID,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());
        }

        [Test]
        public void ClearPointerEventResult()
        {
            // Arrange
            var update = ECSPointerEventResolverSystem.CreateSystem(componentsWriter);

            pointerEventsQueue.Enqueue(CreatePointerEvent());

            // Act
            update.Invoke();

            // Assert
            Assert.AreEqual(0, pointerEventsQueue.Count);
        }

        [Test]
        public void GroupPointerEventResultOfTheSameScene()
        {
            // Arrange
            var update = ECSPointerEventResolverSystem.CreateSystem(componentsWriter);

            pointerEventsQueue.Enqueue(CreatePointerEvent());
            pointerEventsQueue.Enqueue(CreatePointerEvent());

            // Act
            update.Invoke();

            // Assert
            componentsWriter.Received(1)
                            .PutComponent(
                                SCENE_ID,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());
        }

        [Test]
        public void DontSendIfThereIsNoPendingPointerEvents()
        {
            // Arrange
            var state = new ECSPointerEventResolverSystem.State()
            {
                pendingPointerEventsQueue = pointerEventsQueue,
                currentPointerEventsQueue = new Queue<PointerEvent>(),
                componentsWriter = componentsWriter
            };

            state.currentPointerEventsQueue.Enqueue(CreatePointerEvent());
            state.currentPointerEventsQueue.Enqueue(CreatePointerEvent());
            state.currentPointerEventsQueue.Enqueue(CreatePointerEvent());

            // Act
            ECSPointerEventResolverSystem.LateUpdate(state);

            // Assert
            componentsWriter.Received(0)
                            .PutComponent(
                                SCENE_ID,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());
        }

        [Test]
        public void RemoveOlderPointerEventsWhen()
        {
            // Arrange
            var state = new ECSPointerEventResolverSystem.State()
            {
                pendingPointerEventsQueue = pointerEventsQueue,
                currentPointerEventsQueue = new Queue<PointerEvent>(),
                componentsWriter = componentsWriter
            };

            var firstPointerEvent = CreatePointerEvent(SCENE_ID, ActionButton.Backward);
            pointerEventsQueue.Enqueue(firstPointerEvent);

            for (int i = 0; i < ECSPointerEventResolverSystem.MAX_AMOUNT_OF_POINTER_EVENTS_SENT; i++)
            {
                pointerEventsQueue.Enqueue(CreatePointerEvent());
            }

            // Act
            ECSPointerEventResolverSystem.LateUpdate(state);

            // Assert
            componentsWriter.Received(1)
                            .PutComponent(
                                SCENE_ID,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());

            Assert.IsFalse(state.currentPointerEventsQueue.Contains(firstPointerEvent));
            Assert.AreEqual(state.currentPointerEventsQueue.Count, ECSPointerEventResolverSystem.MAX_AMOUNT_OF_POINTER_EVENTS_SENT);
        }

        [Test]
        public void SeparateInTwoDifferentsComponentsIfDifferentScenes()
        {
            // Arrange
            string newSceneId = "NewScene";
            var update = ECSPointerEventResolverSystem.CreateSystem(componentsWriter);

            pointerEventsQueue.Enqueue(CreatePointerEvent());

            var pointerEventScene2 = CreatePointerEvent(newSceneId);
            pointerEventsQueue.Enqueue(pointerEventScene2);

            // Act
            update.Invoke();

            // Assert
            componentsWriter.Received(1)
                            .PutComponent(
                                SCENE_ID,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());

            componentsWriter.Received(1)
                            .PutComponent(
                                newSceneId,
                                SpecialEntityId.SCENE_ROOT_ENTITY,
                                ComponentID.POINTER_EVENTS_RESULT,
                                Arg.Any<PBPointerEventsResult>());

        }

        private PointerEvent CreatePointerEvent(string sceneId = SCENE_ID, ActionButton actionButton = ActionButton.Action3)
        {
            return new PointerEvent(
                sceneId: sceneId,
                button: actionButton,
                hit: new RaycastHit(),
                type: PointerEventType.Down,
                timestamp: 5,
                analog: 1
            );
        }
    }
}