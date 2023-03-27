using DCL;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Interface;
using DCL.Models;
using ECSSystems.PointerInputSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;
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
        private InternalECSComponents internalComponents;

        private Collider colliderEntity1;
        private Collider colliderEntity2;

        private ECS7TestScene scene;
        private ECS7TestEntity entity1;
        private ECS7TestEntity entity2;

        [SetUp]
        public void SetUp()
        {
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            var componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);

            worldState = Substitute.For<IWorldState>();
            worldState.ContainsScene(Arg.Any<int>()).Returns(true);

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

            // systemUpdate = () =>
            // {
            //     internalComponents.MarkDirtyComponentsUpdate();
            //     inputSystemUpdate();
            //     internalComponents.ResetDirtyComponentsUpdate();
            // };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            inputEventResultsComponent = internalComponents.inputEventResultsComponent;

            scene = testUtils.CreateScene(666);
            entity1 = scene.CreateEntity(10111);
            entity2 = scene.CreateEntity(10112);

            dataStoreEcs7.scenes.Add(scene);

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
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.PetDown);
        }

        [Test]
        public void DetectPointerUpOnSameEntityAsPointerDown()
        {
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            result.model.events.Dequeue(); // first event would be pointerDown;
            result.model.events.Dequeue(); // second event would be pointerHoverEnter
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.PetUp);

            // no remaining events because we don't leave the raycast on entity1
            Assert.AreEqual(result.model.events.Count, 0);
        }

        [Test]
        public void DetectPointerUpOnOtherEntityAsPointerDown()
        {
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            result.model.events.Dequeue(); // first event would be pointerDown

            var result2 = inputEventResultsComponent.GetFor(scene, entity2.entityId);
            var enqueuedEvent = result2.model.events.Dequeue();

            Assert.AreEqual(enqueuedEvent.type, PointerEventType.PetUp);
            Assert.AreEqual(enqueuedEvent.hit.EntityId, entity2.entityId);

            // remaining the pointerHoverLeave of entity1 and pointerHoveEnter of entity2
            Assert.AreEqual(result.model.events.Count, 2);
        }

        [Test]
        public void DetectPointerUpWhenInputReleaseWithoutHit()
        {
            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            Assert.AreEqual(result.model.events.Count, 3);

            // up, hover and leave

            var result2 = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var enqueuedEvent = result2.model.events.Dequeue();

            Assert.AreEqual(enqueuedEvent.type, PointerEventType.PetUp);
            Assert.IsFalse(enqueuedEvent.hit.HasEntityId);
        }

        [Test]
        public void DetectHoverEnter()
        {
            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.PetHoverEnter);
        }

        [Test]
        public void DetectHoverExitWhenNewHover()
        {
            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity2;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExitEntity1 = result.model.events.Dequeue();

            var result2 = inputEventResultsComponent.GetFor(scene, entity2.entityId);
            var enqueuedEventNewHoverEnter = result2.model.events.Dequeue();

            Assert.AreEqual(entity2.entityId, enqueuedEventNewHoverEnter.hit.EntityId);
            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExitEntity1.hit.EntityId);
            Assert.IsTrue(enqueuedEventNewHoverEnter.type == PointerEventType.PetHoverEnter);
            Assert.IsTrue(enqueuedEventHoverExitEntity1.type == PointerEventType.PetHoverLeave);
        }

        [Test]
        public void DetectHoverExit()
        {
            dataStoreEcs7.inputActionState[0] = false;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hit.collider = null;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);
            result.model.events.Dequeue(); // hoverEnter - entity1
            var enqueuedEventHoverExit = result.model.events.Dequeue();

            Assert.AreEqual(entity1.entityId, enqueuedEventHoverExit.hit.EntityId);
            Assert.IsTrue(enqueuedEventHoverExit.type == PointerEventType.PetHoverLeave);
        }

        [Test]
        public void ShowPointerDownHoverTooltip()
        {
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
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaAny,
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
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaAny);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        public void NotShowPointerDownHoverTooltipWhenMoreThanMaxDistance()
        {
            dataStoreEcs7.inputActionState[0] = true;

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
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaAny,
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
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
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
                            EventType = PointerEventType.PetUp,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaPointer,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();
            interactionHoverCanvas.ClearReceivedCalls();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(MAX_TOOLTIPS - 1).SetTooltipActive(Arg.Is<int>(i => i != 0), false);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaPointer);
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
                            EventType = PointerEventType.PetUp,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaPointer,
                                HoverText = "Temptation",
                                MaxDistance = 1
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();
            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.ClearReceivedCalls();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hit.distance = 1.1f;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            interactionHoverCanvas.Received(MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
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
                            EventType = PointerEventType.PetUp,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaSecondary,
                                HoverText = "Temptation"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            dataStoreEcs7.inputActionState[(int)InputAction.IaPointer] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            // systemUpdate is called twice
            interactionHoverCanvas.Received(2 * MAX_TOOLTIPS).SetTooltipActive(Arg.Any<int>(), false);
            interactionHoverCanvas.DidNotReceive().SetTooltipText(Arg.Any<int>(), Arg.Any<string>());
            interactionHoverCanvas.DidNotReceive().SetTooltipInput(Arg.Any<int>(), Arg.Any<InputAction>());
            interactionHoverCanvas.DidNotReceive().Show();
        }

        [Test]
        public void HandleSeveralHoverTooltip()
        {
            dataStoreEcs7.inputActionState[0] = true;

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
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaAny,
                                HoverText = "Temptation"
                            }
                        },
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaPointer,
                                HoverText = "Temptation2"
                            }
                        },
                        new PBPointerEvents.Types.Entry()
                        {
                            EventType = PointerEventType.PetDown,
                            EventInfo = new PBPointerEvents.Types.Info()
                            {
                                Button = InputAction.IaPrimary,
                                HoverText = "Temptation3"
                            }
                        }
                    }
                }
            };

            componentsManager.DeserializeComponent(ComponentID.POINTER_EVENTS, scene, entity1, ProtoSerialization.Serialize(pointerEvents));

            systemUpdate();

            interactionHoverCanvas.Received(1).SetTooltipActive(0, true);
            interactionHoverCanvas.Received(1).SetTooltipText(0, "Temptation3");
            interactionHoverCanvas.Received(1).SetTooltipInput(0, InputAction.IaPrimary);
            interactionHoverCanvas.Received(1).Show();
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(66, 66)]
        public void ReturnResultHitPositionInSceneSpace(int sceneBaseCoordX, int sceneBaseCoordY)
        {
            // 1. Set scene, entity and its collider
            Vector2Int sceneBaseCoords = new Vector2Int(sceneBaseCoordX, sceneBaseCoordY);
            var newTestScene = testUtils.CreateScene(2222, sceneBaseCoords, new List<Vector2Int>() { sceneBaseCoords });
            var testEntity = newTestScene.CreateEntity(12345);
            Collider testEntityCollider = new GameObject("testEntityCollider").AddComponent<BoxCollider>();

            internalComponents.onPointerColliderComponent.PutFor(newTestScene, testEntity,
                new InternalColliders() { colliders = new List<Collider>() { testEntityCollider } });

            // 2. position collider entity inside scene space
            ECSTransformHandler transformHandler = new ECSTransformHandler(worldState,
                Substitute.For<BaseVariable<Vector3>>(),
                Substitute.For<IInternalECSComponent<InternalSceneBoundsCheck>>());

            var entityLocalPosition = new Vector3(8, 1, 8);
            var transformModel = new ECSTransform() { position = entityLocalPosition };
            transformHandler.OnComponentModelUpdated(newTestScene, testEntity, transformModel);

            dataStoreEcs7.inputActionState[0] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = testEntityCollider;

            // 3. update pointer ray hit values with object unity position
            var entityGlobalPosition = WorldStateUtils.ConvertSceneToUnityPosition(entityLocalPosition, newTestScene);
            dataStoreEcs7.lastPointerRayHit.hit.point = entityGlobalPosition;
            dataStoreEcs7.lastPointerRayHit.ray.origin = entityGlobalPosition + Vector3.back * 3;

            // 4. Update to enqueue new events
            systemUpdate();

            var result = inputEventResultsComponent.GetFor(newTestScene, testEntity.entityId);
            var enqueuedEvent = result.model.events.Dequeue();

            Assert.AreEqual(testEntity.entityId, enqueuedEvent.hit.EntityId);
            Assert.IsTrue(enqueuedEvent.type == PointerEventType.PetDown);

            // 5. Check enqueued event has correct position and origin
            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z
            }, enqueuedEvent.hit.Position);

            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z - 3
            }, enqueuedEvent.hit.Origin);

            // 6. Clean up
            Object.DestroyImmediate(testEntityCollider.gameObject);
        }

        [Test]
        public void DetectTwoInputDown()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;
            dataStoreEcs7.inputActionState[(int)InputAction.IaAction3] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = true;
            dataStoreEcs7.lastPointerRayHit.hit.collider = colliderEntity1;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, entity1.entityId);

            var enqueuedEvent1 = result.model.events.Dequeue();
            Assert.AreEqual(InputAction.IaPrimary, enqueuedEvent1.button);
            Assert.AreEqual(entity1.entityId, enqueuedEvent1.hit.EntityId);
            Assert.IsTrue(enqueuedEvent1.type == PointerEventType.PetDown);

            var enqueuedEvent2 = result.model.events.Dequeue();
            Assert.AreEqual(InputAction.IaAction3, enqueuedEvent2.button);
            Assert.AreEqual(entity1.entityId, enqueuedEvent2.hit.EntityId);
            Assert.IsTrue(enqueuedEvent2.type == PointerEventType.PetDown);
        }

        [Test]
        public void DetectGlobalInputDown()
        {
            dataStoreEcs7.inputActionState[(int)InputAction.IaPrimary] = true;
            dataStoreEcs7.inputActionState[(int)InputAction.IaAction3] = true;

            dataStoreEcs7.lastPointerRayHit.didHit = false;
            dataStoreEcs7.lastPointerRayHit.hasValue = true;

            systemUpdate();

            var result = inputEventResultsComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);

            var enqueuedEvent1 = result.model.events.Dequeue();
            Assert.AreEqual(InputAction.IaPrimary, enqueuedEvent1.button);
            Assert.IsTrue(enqueuedEvent1.type == PointerEventType.PetDown);
        }


        [Test]
        public void EnsureWebInterfaceAndProtobufInputEnumsMatch()
        {
            var inputActionsWebInterface = Enum.GetValues(typeof(WebInterface.ACTION_BUTTON)) as int[];
            var inputActionsProto = Enum.GetValues(typeof(InputAction)) as int[];
            Assert.AreEqual(inputActionsProto!.Length, inputActionsWebInterface!.Length);
            for (var i = 0; i < inputActionsWebInterface.Length; i++)
            {
                Assert.AreEqual(inputActionsWebInterface[i], inputActionsProto[i]);
            }
        }
    }
}
