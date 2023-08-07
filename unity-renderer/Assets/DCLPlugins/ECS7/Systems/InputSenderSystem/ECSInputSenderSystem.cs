using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.Models;
using System;
using System.Collections.Generic;

namespace ECSSystems.InputSenderSystem
{
    public class ECSInputSenderSystem
    {
        private readonly IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private readonly IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> componentPool;
        private readonly Func<int> getCurrentSceneNumber;
        private uint lastTimestamp = 0;

        public ECSInputSenderSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBPointerEventsResult>> componentPool,
            Func<int> getCurrentSceneNumber)
        {
            this.inputResultComponent = inputResultComponent;
            this.engineInfoComponent = engineInfoComponent;
            this.componentsWriter = componentsWriter;
            this.componentPool = componentPool;
            this.getCurrentSceneNumber = getCurrentSceneNumber;
        }

        public void Update()
        {
            var inputResults = inputResultComponent.GetForAll();
            int currentSceneNumber = getCurrentSceneNumber();
            bool restrictedActionEnabled = false;

            for (int i = 0; i < inputResults.Count; i++)
            {
                var model = inputResults[i].value.model;

                if (!model.dirty)
                    continue;

                var scene = inputResults[i].value.scene;
                var entity = inputResults[i].value.entity;
                long entityId = entity.entityId;
                InternalEngineInfo engineInfoModel = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)!.Value.model;
                bool checkRestrictedAction = scene.isPersistent || scene.sceneData.sceneNumber == currentSceneNumber;

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                int count = model.events.Count;

                for (int j = 0; j < count; j++)
                {
                    InternalInputEventResults.EventData inputEvent = model.events[j];

                    InputAction actionButton = inputEvent.button;
                    PointerEventType evtType = inputEvent.type;
                    RaycastHit hit = inputEvent.hit;

                    if (checkRestrictedAction && !restrictedActionEnabled)
                    {
                        if (IsValidInputForRestrictedActions(entityId, actionButton, evtType, hit))
                        {
                            restrictedActionEnabled = true;
                            engineInfoModel.EnableRestrictedActionTick = engineInfoModel.SceneTick;
                            engineInfoComponent.PutFor(currentSceneNumber, SpecialEntityId.SCENE_ROOT_ENTITY, engineInfoModel);
                        }
                    }

                    var componentPooled = componentPool.Get();
                    var componentModel = componentPooled.WrappedComponent.Model;
                    componentModel.Button = inputEvent.button;
                    componentModel.Hit = inputEvent.hit;
                    componentModel.State = inputEvent.type;
                    componentModel.Timestamp = lastTimestamp++;
                    componentModel.TickNumber = engineInfoModel.SceneTick;

                    writer.Append(entityId, ComponentID.POINTER_EVENTS_RESULT, componentPooled);
                }

                model.events.Clear();
            }
        }

        private static bool IsValidInputForRestrictedActions(long entityId, InputAction actionButton,
            PointerEventType evtType, RaycastHit hit)
        {
            if (entityId == SpecialEntityId.SCENE_ROOT_ENTITY)
                return false;

            return hit != null
                   && (evtType == PointerEventType.PetDown || evtType == PointerEventType.PetUp)
                   && actionButton != InputAction.IaAny;
        }
    }
}
