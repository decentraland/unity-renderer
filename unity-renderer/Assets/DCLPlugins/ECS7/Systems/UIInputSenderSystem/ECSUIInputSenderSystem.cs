using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;

namespace ECSSystems.UIInputSenderSystem
{
    /// <summary>
    /// Handles sending unique events from UI Elements to the scene
    /// </summary>
    public class ECSUIInputSenderSystem
    {
        internal IInternalECSComponent<InternalUIInputResults> inputResultComponent { get; }
        internal IECSComponentWriter componentWriter { get; }

        public ECSUIInputSenderSystem(IInternalECSComponent<InternalUIInputResults> inputResultComponent, IECSComponentWriter componentWriter)
        {
            this.inputResultComponent = inputResultComponent;
            this.componentWriter = componentWriter;
        }

        public void Update()
        {
            var inputResults = inputResultComponent.GetForAll();

            for (var i = 0; i < inputResults.Count; i++)
            {
                var model = inputResults[i].value.model;
                if (!model.dirty)
                    continue;

                var scene = inputResults[i].value.scene;
                var entity = inputResults[i].value.entity;

                // Results are already prepared in its final form by the UI Components themselves

                while (model.Results.TryDequeue(out var result))
                {
                    componentWriter.PutComponent(
                        result.Message.GetType(),
                        scene.sceneData.sceneNumber,
                        entity.entityId,
                        result.ComponentId,
                        result.Message,
                        ECSComponentWriteType.SEND_TO_SCENE | ECSComponentWriteType.WRITE_STATE_LOCALLY
                        );
                }
            }
        }
    }
}
