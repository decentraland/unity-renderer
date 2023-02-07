using System;
using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace ECSSystems.InputSenderSystem
{
    public static class ECSInputSenderSystem
    {
        // Entities' number as buffer for PointerEventResult, the range is [MIN_ENTITY_TARGET, MAX_ENTITY_TARGET]
        private const long MIN_ENTITY_TARGET = 32;
        private const long MAX_ENTITY_TARGET = 64;

        private class State
        {
            public IInternalECSComponent<InternalInputEventResults> inputResultComponent;
            public IECSComponentWriter componentWriter;
        }

        public static Action CreateSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IECSComponentWriter componentWriter)
        {
            var state = new State()
            {
                inputResultComponent = inputResultComponent,
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

                inputResults[i].value.model.lastEntity++;
                if (inputResults[i].value.model.lastEntity < MIN_ENTITY_TARGET || inputResults[i].value.model.lastEntity >= MAX_ENTITY_TARGET)
                {
                    inputResults[i].value.model.lastEntity = MIN_ENTITY_TARGET;
                }

                var scene = inputResults[i].value.scene;

                PBPointerEventsResult result = new PBPointerEventsResult();
                result.Commands.Capacity = model.events.Count;

                // using foreach to iterate through queue without removing it elements
                // if it proves too slow we should switch the queue for a list
                foreach (InternalInputEventResults.EventData inputEvent in model.events)
                {
                    result.Commands.Add(new PBPointerEventsResult.Types.PointerCommand()
                    {
                        Button = inputEvent.button,
                        Hit = inputEvent.hit,
                        State = inputEvent.type,
                        Timestamp = inputEvent.timestamp
                    });
                }
                model.events.Clear();

                writer.PutComponent(scene.sceneData.sceneNumber,
                    inputResults[i].value.model.lastEntity,
                    ComponentID.POINTER_EVENTS_RESULT,
                    result,
                    ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
            }
        }
    }
}
