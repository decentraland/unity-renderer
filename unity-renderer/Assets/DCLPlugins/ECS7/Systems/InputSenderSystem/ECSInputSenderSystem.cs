using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;
using UnityEngine;
using RaycastHit = DCL.ECSComponents.RaycastHit;

namespace ECSSystems.InputSenderSystem
{
    public class ECSInputSenderSystem
    {
        private readonly IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private readonly IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private readonly IECSComponentWriter componentWriter;
        private readonly Func<int> getCurrentSceneNumber;

        private uint lastTimestamp = 0;

        public ECSInputSenderSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IECSComponentWriter componentWriter,
            Func<int> getCurrentSceneNumber)
        {
            this.inputResultComponent = inputResultComponent;
            this.engineInfoComponent = engineInfoComponent;
            this.componentWriter = componentWriter;
            this.getCurrentSceneNumber = getCurrentSceneNumber;
        }

        public void Update()
        {
            var inputResults = inputResultComponent.GetForAll();
            var writer = componentWriter;
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
                InternalEngineInfo engineInfoModel = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model;
                bool checkRestrictedAction = scene.sceneData.sceneNumber == currentSceneNumber;

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

                    writer.AppendComponent(scene.sceneData.sceneNumber,
                        entityId,
                        ComponentID.POINTER_EVENTS_RESULT,
                        new PBPointerEventsResult()
                        {
                            Button = actionButton,
                            Hit = hit,
                            State = evtType,
                            Timestamp = lastTimestamp++,
                            TickNumber = engineInfoModel.SceneTick
                        },
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
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
