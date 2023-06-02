using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;

namespace ECSSystems.InputSenderSystem
{
    public class ECSInputSenderSystem
    {
        private IInternalECSComponent<InternalInputEventResults> inputResultComponent;
        private IInternalECSComponent<InternalEngineInfo> engineInfoComponent;
        private IECSComponentWriter componentWriter;
        private uint lastTimestamp = 0;

        public ECSInputSenderSystem(
            IInternalECSComponent<InternalInputEventResults> inputResultComponent,
            IInternalECSComponent<InternalEngineInfo> engineInfoComponent,
            IECSComponentWriter componentWriter)
        {
            this.inputResultComponent = inputResultComponent;
            this.engineInfoComponent = engineInfoComponent;
            this.componentWriter = componentWriter;
        }

        public void Update()
        {
            var inputResults = inputResultComponent.GetForAll();
            var writer = componentWriter;

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
                            Timestamp = lastTimestamp++,
                            TickNumber = engineInfoComponent.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY).model.SceneTick
                        },
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY);
                }

                model.events.Clear();
            }
        }
    }
}
