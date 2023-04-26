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
using RPC.Context;
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
        private RestrictedActionsContext restrictedActionsRpcContext;

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

            restrictedActionsRpcContext = new RestrictedActionsContext();
            restrictedActionsRpcContext.LastFrameWithInput = -1;

            systemUpdate = ECSPointerInputSystem.CreateSystem(
                internalComponents.onPointerColliderComponent,
                internalComponents.inputEventResultsComponent,
                internalComponents.PointerEventsComponent,
                interactionHoverCanvas,
                worldState,
                dataStoreEcs7,
                restrictedActionsRpcContext);

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
                new InternalColliders() { colliders = new KeyValueSet<Collider, int>() { { colliderEntity1, 0 } } });

            internalComponents.onPointerColliderComponent.PutFor(scene, entity2,
                new InternalColliders() { colliders = new KeyValueSet<Collider, int>() { { colliderEntity2, 0 } } });
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
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetDown);
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

            // first event would be pointerDown;
            // second event would be pointerHoverEnter
            // third event would be pointerUp
            var evt = result.model.events[2];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetUp);
            Assert.AreEqual(3, result.model.events.Count);
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
            result.model.events.RemoveAt(0); // first event would be pointerDown

            var result2 = inputEventResultsComponent.GetFor(scene, entity2.entityId);
            var evt = result2.model.events[0];

            Assert.AreEqual(evt.type, PointerEventType.PetUp);
            Assert.AreEqual(evt.hit.EntityId, entity2.entityId);

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
            var evt = result2.model.events[0];

            Assert.AreEqual(evt.type, PointerEventType.PetUp);
            Assert.IsFalse(evt.hit.HasEntityId);
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
            var evt = result.model.events[0];

            Assert.AreEqual(entity1.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetHoverEnter);
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

            // result.model.events[0] == hoverEnter - entity1
            var evtEventHoverExitEntity1 = result.model.events[1];

            var result2 = inputEventResultsComponent.GetFor(scene, entity2.entityId);
            var evtEventNewHoverEnter = result2.model.events[0];

            Assert.AreEqual(entity2.entityId, evtEventNewHoverEnter.hit.EntityId);
            Assert.AreEqual(entity1.entityId, evtEventHoverExitEntity1.hit.EntityId);
            Assert.IsTrue(evtEventNewHoverEnter.type == PointerEventType.PetHoverEnter);
            Assert.IsTrue(evtEventHoverExitEntity1.type == PointerEventType.PetHoverLeave);
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

            //result.model.events[0] == hoverEnter - entity1
            var evtEventHoverExit = result.model.events[1];

            Assert.AreEqual(entity1.entityId, evtEventHoverExit.hit.EntityId);
            Assert.IsTrue(evtEventHoverExit.type == PointerEventType.PetHoverLeave);
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
                new InternalColliders() { colliders = new KeyValueSet<Collider, int>() { { testEntityCollider, 0 } } });

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
            var evt = result.model.events[0];

            Assert.AreEqual(testEntity.entityId, evt.hit.EntityId);
            Assert.IsTrue(evt.type == PointerEventType.PetDown);

            // 5. Check enqueued event has correct position and origin
            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z
            }, evt.hit.Position);

            Assert.AreEqual(new Decentraland.Common.Vector3()
            {
                X = entityLocalPosition.x,
                Y = entityLocalPosition.y,
                Z = entityLocalPosition.z - 3
            }, evt.hit.GlobalOrigin);

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

            var evt1 = result.model.events[0];
            Assert.AreEqual(InputAction.IaPrimary, evt1.button);
            Assert.AreEqual(entity1.entityId, evt1.hit.EntityId);
            Assert.IsTrue(evt1.type == PointerEventType.PetDown);

            var evt2 = result.model.events[1];
            Assert.AreEqual(InputAction.IaAction3, evt2.button);
            Assert.AreEqual(entity1.entityId, evt2.hit.EntityId);
            Assert.IsTrue(evt2.type == PointerEventType.PetDown);
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

            var evt = result.model.events[0];
            Assert.AreEqual(InputAction.IaPrimary, evt.button);
            Assert.IsTrue(evt.type == PointerEventType.PetDown);
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

        [Test]
        [TestCase(InputAction.IaAction3, ExpectedResult = true)]
        [TestCase(InputAction.IaAction4, ExpectedResult = true)]
        [TestCase(InputAction.IaAction5, ExpectedResult = true)]
        [TestCase(InputAction.IaAction6, ExpectedResult = true)]
        [TestCase(InputAction.IaPrimary, ExpectedResult = true)]
        [TestCase(InputAction.IaSecondary, ExpectedResult = true)]
        [TestCase(InputAction.IaPointer, ExpectedResult = true)]
        [TestCase(InputAction.IaAny, ExpectedResult = false)]
        [TestCase(InputAction.IaForward, ExpectedResult = false)]
        [TestCase(InputAction.IaBackward, ExpectedResult = false)]
        [TestCase(InputAction.IaRight, ExpectedResult = false)]
        [TestCase(InputAction.IaLeft, ExpectedResult = false)]
        [TestCase(InputAction.IaJump, ExpectedResult = false)]
        [TestCase(InputAction.IaWalk, ExpectedResult = false)]
        public bool SetCurrentFrameWhenInputActionIsValid(InputAction inputAction)
        {
            int currentFrame = Time.frameCount;
            dataStoreEcs7.inputActionState[(int)inputAction] = true;
            systemUpdate();
            return currentFrame == restrictedActionsRpcContext.LastFrameWithInput;
        }
    }
}
