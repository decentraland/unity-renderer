using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System;

namespace ECSSystems.InputSenderSystem
{
    public static class ECSInputSenderSystem
    {
        private class State
        {
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
            public IECSComponentWriter componentWriter;
            public uint lastTimestamp = 0;
        }

        public static Action CreateSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IECSComponentWriter componentWriter)
        {
            var state = new State()
            {
                inputResultComponent = inputResultComponent,
                engineInfoComponent = engineInfoComponent,
                componentWriter = componentWriter
            };

            return () => Update(state);
        }

        private static void Update(State state)
        {
            var inputResults = state.inputResultComponent.GetForAll();
            var writer = state.componentWriter;

            for (int i = 0; i < inputResults.Count; i++)
            {
                var model = inputResults[i].value.model;

                if (!model.dirty)
                    continue;

                var scene = inputResults[i].value.scene;
                var entity = inputResults[i].value.entity;

                int count = model.events.Count;

                for (int j = 0; j < count; j++)
                {
                    InternalInputEventResults.EventData inputEvent = model.events[j];

                    writer.AppendComponent(scene.sceneData.sceneNumber,
                        entity.entityId,
                        ComponentID.POINTER_EVENTS_RESULT,
                        new PBPointerEventsResult()
                        {
                            Button = inputEvent.button,
                            Hit = inputEvent.hit,
                            State = inputEvent.type,
                            Timestamp = state.lastTimestamp++,
                            TickNumber = state.engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick
                        },
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
                }

                model.events.Clear();
            }
        }
    }
}
