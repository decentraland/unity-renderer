using System;
using System.Collections.Generic;
using DCL;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class ECSPointerInputSystemShould
    {
        private const int MAX_TOOLTIPS = 2;

        private ECS7TestUtilsScenesAndEntities testUtils;
        private Action systemUpdate;

        private DataStore_ECS7 dataStoreEcs7;
        private IWorldState worldState;
        private IInternalECSComponent<InternalInputEventResults> inputEventResultsComponent;
        private IECSInteractionHoverCanvas interactionHoverCanvas;
        private ECSComponentsManager componentsManager;

        private Collider colliderEntity1;
        private Collider colliderEntity2;

        private ECS7TestScene scene;
        private ECS7TestEntity entity1;
        private ECS7TestEntity entity2;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory);

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);

            worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<string>()).Returns(true);

            dataStoreEcs7 = new DataStore_ECS7();
            interactionHoverCanvas = Substitute.For<IECSInteractionHoverCanvas>();
            interactionHoverCanvas.tooltipsCount.Returns(MAX_TOOLTIPS);

            systemUpdate = ECSPointerInputSystem.CreateSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                (ECSComponent<PBPointerEvents>)componentsManager.GetOrCreateComponent(ComponentID.POINTER_EVENTS),
                interactionHoverCanvas,
                worldState,
                dataStoreEcs7);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager);
            inputEventResultsComponent = internalComponents.inputEventResultsComponent;

            scene = testUtils.CreateScene("temptation");
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            var colliderGO1 = new GameObject("collider1");
            var colliderGO2 = new GameObject("collider2");

            colliderEntity1 = colliderGO1.AddComponent<BoxCollider>();
            colliderEntity2 = colliderGO2.AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(scene, entity1,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity1 } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new List<Collider>() { colliderEntity2 } });
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            Object.DestroyImmediate(colliderEntity1.gameObject);
            Object.DestroyImmediate(colliderEntity2.gameObject);
        }

        [Test]
        public void DetectPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.Down);
        }

        [Test]
        public void DetectPointerUpOnSameEntityAsPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.Up);
        }

        [Test]
        public void DetectPointerUpOnOtherEntityAsPointerDown()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.IsFalse(enqueuedEvent.hit.HasEntityId);
        }

        [Test]
        public void DetectPointerUpWhenInputReleaseWithoutHit()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // first event would be pointerDown
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.IsFalse(enqueuedEvent.hit.HasEntityId);
        }

        [Test]
        public void DetectHoverEnter()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.HoverEnter);
        }

        [Test]
        public void DetectHoverExitWhenNewHover()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExit = result.model.events.Dequeue();
            var enqueuedEventNewHoverEnter = result.model.events.Dequeue();

            Assert.AreEqual(entity2.entityId, enqueuedEventNewHoverEnter.hit.EntityId);
            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExit.hit.EntityId);
            Assert.IsTrue(enqueuedEventNewHoverEnter.type == PointerEventType.HoverEnter);
            Assert.IsTrue(enqueuedEventHoverExit.type == PointerEventType.HoverLeave);
        }

        [Test]
        public void DetectHoverExit()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = false;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = null;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExit = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExit.hit.EntityId);
            Assert.IsTrue(enqueuedEventHoverExit.type == PointerEventType.HoverLeave);
        }

        [Test]
        public void ShowPointerDownHoverTooltip()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Down,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Any,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(MAX_TOOLTIPS - 1).SetTooltipActive(Arg.Is<int>(i => i != 0), false);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, ActionButton.Any);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        public void NotShowPointerDownHoverTooltipWhenMoreThanMaxDistance()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = 1.1f;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Down,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Any,
                                HoverText = "Temptation",
                                MaxDistance = 1
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<ActionButton>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void ShowPointerUpHoverTooltip()
        {
            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Up,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Pointer,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.lastPointerInputEvent.buttonId = (int)ActionButton.Pointer;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(MAX_TOOLTIPS - 1).SetTooltipActive(Arg.Is<int>(i => i != 0), false);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, ActionButton.Pointer);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        public void NotShowPointerUpHoverTooltipWhenMoreThanMaxDistance()
        {
            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Up,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Pointer,
                                HoverText = "Temptation",
                                MaxDistance = 1
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.lastPointerInputEvent.buttonId = (int)ActionButton.Pointer;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = 1.1f;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<ActionButton>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void NotShowPointerUpHoverTooltipWhenButtonMismatch()
        {
            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Up,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Secondary,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.lastPointerInputEvent.buttonId = (int)ActionButton.Pointer;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<ActionButton>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void HandleSeveralHoverTooltip()
        {
            dataStoreEcs7.lastPointerInputEvent.buttonId = 0;
            dataStoreEcs7.lastPointerInputEvent.isButtonDown = true;
            dataStoreEcs7.lastPointerInputEvent.hasValue = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            PBPointerEvents pointerEvents = new PBPointerEvents()
            {
                PointerEvents =
                {
                    new List<PBPointerEvents.Types.Entry>()
                    {
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Down,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Any,
                                HoverText = "Temptation"
                            }
                        },
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Down,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Pointer,
                                HoverText = "Temptation2"
                            }
                        },
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.Down,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = ActionButton.Primary,
                                HoverText = "Temptation3"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(1).SetTooltipActive(1, true);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipText(1, "Temptation2");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, ActionButton.Any);
            interactionHoverCanvas.Received(1).SetTooltipInput(1, ActionButton.Pointer);
            interactionHoverCanvas.Received(1).Show();
        }
    }
}