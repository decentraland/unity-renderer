using DCL.CRDT;
using DCL.ECS7;
using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.InputSenderSystem;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestUtils;

namespace Tests.Systems.InputSender
{
    public class ECSInputSenderSystemShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private DualKeyValueSet<long, int, WriteData> outgoingMessages;
        private WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> componentPool;
        private InternalECSComponents internalComponents;

        private Action updateSystems;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            inputResultComponent = internalComponents.inputEventResultsComponent;
            outgoingMessages = new DualKeyValueSet<long, int, WriteData>();

            var componentsWriter = new Dictionary<int, ComponentWriter>()
            {
                { 666, new ComponentWriter(outgoingMessages) }
            };

            componentPool = new WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>>(0, () => new ProtobufWrappedComponent<PBPointerEventsResult>(new PBPointerEventsResult()));

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);

            var inputSenderSystem = new ECSInputSenderSystem(
                inputResultComponent,
                internalComponents.EngineInfo,
                componentsWriter,
                componentPool,
                () => scene.sceneData.sceneNumber);

            updateSystems = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                inputSenderSystem.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };

            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo());
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [Test]
        public void SendResultsWhenDirty()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = InputAction.IaPrimary, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaSecondary, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction4, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction3, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction5, hit = new RaycastHit() { } },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            updateSystems();

            outgoingMessages.Append_Called<PBPointerEventsResult>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.POINTER_EVENTS_RESULT,
                null
            );
        }

        [Test]
        public void NotSendResultsWhenIsNotDirty()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = InputAction.IaPrimary, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaSecondary, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction4, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction3, hit = new RaycastHit() { } },
                new InternalInputEventResults.EventData() { button = InputAction.IaAction5, hit = new RaycastHit() { } },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            var compData = inputResultComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY);
            var model = compData.Value.model;

            inputResultComponent.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);

            //clean dirty
            internalComponents.MarkDirtyComponentsUpdate();
            internalComponents.ResetDirtyComponentsUpdate();

            updateSystems();

            outgoingMessages.Append_NotCalled(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.POINTER_EVENTS_RESULT
            );
        }

        [Test]
        public void StoreSceneTickInResult()
        {
            IList<InternalInputEventResults.EventData> events = new List<InternalInputEventResults.EventData>()
            {
                new InternalInputEventResults.EventData() { button = InputAction.IaPrimary, hit = new RaycastHit() { } },
            };

            foreach (var eventData in events)
            {
                inputResultComponent.AddEvent(scene, eventData);
            }

            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = 3,
            });

            updateSystems();

            outgoingMessages.Append_Called<PBPointerEventsResult>(
                SpecialEntityId.SCENE_ROOT_ENTITY,
                ComponentID.POINTER_EVENTS_RESULT,
                (result) => result.TickNumber == 3
            );
        }

        [Test]
        public void EnableRestrictedActionsOnValidInput()
        {
            const uint CURRENT_TICK = 823;

            inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = InputAction.IaPrimary,
                type = PointerEventType.PetDown,
                hit = new RaycastHit()
                {
                    EntityId = 66
                }
            });

            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = CURRENT_TICK,
                EnableRestrictedActionTick = 0
            });

            updateSystems();

            var engineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
            Assert.AreEqual(CURRENT_TICK, engineInfo.EnableRestrictedActionTick);
        }

        [Test]
        public void NotEnableRestrictedActionsOnInvalidInputType()
        {
            inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = InputAction.IaPrimary,
                type = PointerEventType.PetHoverEnter,
                hit = new RaycastHit()
                {
                    EntityId = 66
                }
            });

            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = 823,
                EnableRestrictedActionTick = 0
            });

            updateSystems();

            var engineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
            Assert.AreEqual(0, engineInfo.EnableRestrictedActionTick);
        }

        [Test]
        public void NotEnableRestrictedActionsOnInvalidInputHit()
        {
            inputResultComponent.AddEvent(scene, new InternalInputEventResults.EventData()
            {
                button = InputAction.IaPrimary,
                type = PointerEventType.PetDown,
                hit = null
            });

            internalComponents.EngineInfo.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = 823,
                EnableRestrictedActionTick = 0
            });

            updateSystems();

            var engineInfo = internalComponents.EngineInfo.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
            Assert.AreEqual(0, engineInfo.EnableRestrictedActionTick);
        }

        [Test]
        public void NotEnableRestrictedActionsOnValidInputFromOtherScene()
        {
            const uint CURRENT_TICK = 823;
            var sceneWherePlayerIsNotCurrentlyOn = testUtils.CreateScene(667);

            inputResultComponent.AddEvent(sceneWherePlayerIsNotCurrentlyOn, new InternalInputEventResults.EventData()
            {
                button = InputAction.IaPrimary,
                type = PointerEventType.PetDown,
                hit = new RaycastHit()
                {
                    EntityId = 66
                }
            });

            internalComponents.EngineInfo.PutFor(sceneWherePlayerIsNotCurrentlyOn, SpecialEntityId.SCENE_ROOT_ENTITY, new InternalEngineInfo()
            {
                SceneTick = CURRENT_TICK,
                EnableRestrictedActionTick = 0
            });

            updateSystems();

            var engineInfo = internalComponents.EngineInfo.GetFor(sceneWherePlayerIsNotCurrentlyOn, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
            Assert.AreEqual(0, engineInfo.EnableRestrictedActionTick);
        }
    }
}
