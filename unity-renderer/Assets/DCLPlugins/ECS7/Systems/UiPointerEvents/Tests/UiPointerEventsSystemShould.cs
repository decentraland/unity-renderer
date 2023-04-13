using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using ECSSystems.ECSUiPointerEventsSystem;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tests
{
    public class UiPointerEventsSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private InternalECSComponents internalComponents;
        private ComponentGroups componentGroups;

        private UIDocument uiDocumentResource;
        private UIDocument uiDocumentInstance;

        [SetUp]
        public void SetUp()
        {
            var factory = new ECSComponentsFactory();
            var manager = new ECSComponentsManager(factory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(manager, factory, executors);
            componentGroups = new ComponentGroups(manager);

            testUtils = new ECS7TestUtilsScenesAndEntities(manager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1);

            uiDocumentResource = Resources.Load<UIDocument>("ScenesUI");
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();

            if (uiDocumentInstance)
                Object.Destroy(uiDocumentInstance);
        }

        [Test]
        public void HandleUiWithoutRegisteredPointerEvents()
        {
            // add ui and pointer event to entity
            internalComponents.uiContainerComponent.PutFor(scene, entity, new InternalUiContainer(1)
            {
                components = { 1 }
            });

            internalComponents.PointerEventsComponent.PutFor(scene, entity, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false))
                }
            });

            // should register ui callbacks
            ECSUiPointerEventsSystem.HandleUiWithoutRegisteredPointerEvents(
                componentGroups.UnregisteredUiPointerEvents,
                internalComponents.RegisteredUiPointerEventsComponent,
                internalComponents.inputEventResultsComponent);

            var callbackModel = internalComponents.RegisteredUiPointerEventsComponent.GetFor(scene, entity).model;
            Assert.NotNull(callbackModel.OnPointerDownCallback);
            Assert.NotNull(callbackModel.OnPointerLeaveCallback);
            Assert.IsNull(callbackModel.OnPointerEnterCallback);
            Assert.IsNull(callbackModel.OnPointerUpCallback);
        }

        [Test]
        public void RegisterCallbackCorrectly()
        {
            uiDocumentInstance = Object.Instantiate(uiDocumentResource);

            VisualElement visualElement = new VisualElement();
            uiDocumentInstance.rootVisualElement.Add(visualElement);

            bool pointerDown = false;

            InternalRegisteredUiPointerEvents callbacks = new InternalRegisteredUiPointerEvents()
            {
                OnPointerDownCallback = evt => pointerDown = true
            };

            ECSUiPointerEventsSystem.RegisterUiPointerEvents(visualElement, callbacks);
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerDownEvent() { target = visualElement });

            Assert.IsTrue(pointerDown);
        }

        [Test]
        public void UnregisterCallbackCorrectly()
        {
            uiDocumentInstance = Object.Instantiate(uiDocumentResource);

            VisualElement visualElement = new VisualElement();
            uiDocumentInstance.rootVisualElement.Add(visualElement);

            bool pointerDown = false;

            InternalRegisteredUiPointerEvents callbacks = new InternalRegisteredUiPointerEvents()
            {
                OnPointerDownCallback = evt => pointerDown = true
            };

            ECSUiPointerEventsSystem.RegisterUiPointerEvents(visualElement, callbacks);
            ECSUiPointerEventsSystem.UnregisterUiPointerEvents(visualElement, callbacks);
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerDownEvent() { target = visualElement });

            Assert.IsFalse(pointerDown);
        }

        [Test]
        public void HandlePointerEventComponentUpdate()
        {
            // add required components entity
            internalComponents.uiContainerComponent.PutFor(scene, entity, new InternalUiContainer(1)
            {
                components = { 1 }
            });

            internalComponents.PointerEventsComponent.PutFor(scene, entity, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false))
                }
            });

            internalComponents.RegisteredUiPointerEventsComponent.PutFor(scene, entity, new InternalRegisteredUiPointerEvents());

            // mark PointerEventsComponent as dirty
            MarkDirtyComponents();

            // should update callbacks
            ECSUiPointerEventsSystem.HandlePointerEventComponentUpdate(
                componentGroups.RegisteredUiPointerEvents,
                internalComponents.RegisteredUiPointerEventsComponent,
                internalComponents.inputEventResultsComponent);

            var callbackModel = internalComponents.RegisteredUiPointerEventsComponent.GetFor(scene, entity).model;
            Assert.NotNull(callbackModel.OnPointerDownCallback);
            Assert.NotNull(callbackModel.OnPointerLeaveCallback);
            Assert.IsNull(callbackModel.OnPointerEnterCallback);
            Assert.IsNull(callbackModel.OnPointerUpCallback);
        }

        [Test]
        public void HandleUiContainerRemoval()
        {
            // add required components entity
            internalComponents.PointerEventsComponent.PutFor(scene, entity, new InternalPointerEvents());

            internalComponents.RegisteredUiPointerEventsComponent.PutFor(scene, entity, new InternalRegisteredUiPointerEvents());

            ECSUiPointerEventsSystem.HandleUiContainerRemoval(
                componentGroups.RegisteredUiPointerEventsWithUiRemoved,
                internalComponents.RegisteredUiPointerEventsComponent);

            // InternalRegisteredUiPointerEvents should be removed
            Assert.IsNull(internalComponents.RegisteredUiPointerEventsComponent.GetFor(scene, entity));
        }

        [Test]
        public void HandlePointerEventsRemoval()
        {
            // add required components entity
            internalComponents.uiContainerComponent.PutFor(scene, entity, new InternalUiContainer(1)
            {
                components = { 1 }
            });

            internalComponents.RegisteredUiPointerEventsComponent.PutFor(scene, entity, new InternalRegisteredUiPointerEvents());

            ECSUiPointerEventsSystem.HandlePointerEventsRemoval(
                componentGroups.RegisteredUiPointerEventsWithPointerEventsRemoved,
                internalComponents.RegisteredUiPointerEventsComponent);

            // InternalRegisteredUiPointerEvents should be removed
            Assert.IsNull(internalComponents.RegisteredUiPointerEventsComponent.GetFor(scene, entity));
        }

        [Test]
        public void CreateUiPointerEventsCorrectly()
        {
            var events = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false))
                }
            };

            var result = ECSUiPointerEventsSystem.CreateUiPointerEvents(
                scene,
                entity,
                internalComponents.inputEventResultsComponent,
                events);

            Assert.NotNull(result.OnPointerDownCallback);
            Assert.NotNull(result.OnPointerLeaveCallback);
            Assert.IsNull(result.OnPointerEnterCallback);
            Assert.IsNull(result.OnPointerUpCallback);

            // unsupported input action
            events = new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaAction5, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaJump, "temptation", 0, false))
                }
            };

            result = ECSUiPointerEventsSystem.CreateUiPointerEvents(
                scene,
                entity,
                internalComponents.inputEventResultsComponent,
                events);

            Assert.IsNull(result.OnPointerDownCallback);
            Assert.IsNull(result.OnPointerLeaveCallback);
            Assert.IsNull(result.OnPointerEnterCallback);
            Assert.IsNull(result.OnPointerUpCallback);
        }

        [Test]
        public void SetupCorrectly()
        {
            var system = new ECSUiPointerEventsSystem(
                internalComponents.RegisteredUiPointerEventsComponent,
                internalComponents.inputEventResultsComponent,
                componentGroups.UnregisteredUiPointerEvents,
                componentGroups.RegisteredUiPointerEvents,
                componentGroups.RegisteredUiPointerEventsWithUiRemoved,
                componentGroups.RegisteredUiPointerEventsWithPointerEventsRemoved);

            void UpdateSystem()
            {
                MarkDirtyComponents();
                system.Update();
                ResetDirtyComponents();
            }

            uiDocumentInstance = Object.Instantiate(uiDocumentResource);

            var internalUiComponent = new InternalUiContainer(1)
            {
                components = { 1 }
            };

            internalComponents.uiContainerComponent.PutFor(scene, entity, internalUiComponent);

            internalComponents.PointerEventsComponent.PutFor(scene, entity, new InternalPointerEvents()
            {
                PointerEvents =
                {
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetDown,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetUp,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverEnter,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false)),
                    new InternalPointerEvents.Entry(
                        PointerEventType.PetHoverLeave,
                        new InternalPointerEvents.Info(InputAction.IaPointer, "temptation", 0, false))
                }
            });

            UpdateSystem();

            // send events
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerDownEvent() { target = internalUiComponent.rootElement });
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerUpEvent() { target = internalUiComponent.rootElement });
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerEnterEvent() { target = internalUiComponent.rootElement });
            uiDocumentInstance.rootVisualElement.SendEvent(new PointerLeaveEvent() { target = internalUiComponent.rootElement });

            Queue<PointerEventType> expectedEventTypes = new Queue<PointerEventType>();
            expectedEventTypes.Enqueue(PointerEventType.PetDown);
            expectedEventTypes.Enqueue(PointerEventType.PetUp);
            expectedEventTypes.Enqueue(PointerEventType.PetHoverEnter);
            expectedEventTypes.Enqueue(PointerEventType.PetHoverLeave);

            var inputResult = internalComponents.inputEventResultsComponent.GetFor(scene, entity).model;

            Assert.AreEqual(4, inputResult.events.Count);

            for (int i=0; i< inputResult.events.Count; i++)
            {
                InternalInputEventResults.EventData result = inputResult.events[i];
                Assert.AreEqual(entity.entityId, result.hit.EntityId);
                Assert.AreEqual(InputAction.IaPointer, result.button);
                Assert.AreEqual(expectedEventTypes.Dequeue(), result.type);
            }
        }

        private void MarkDirtyComponents()
        {
            internalComponents.MarkDirtyComponentsUpdate();
        }

        private void ResetDirtyComponents()
        {
            internalComponents.ResetDirtyComponentsUpdate();
        }
    }
}
