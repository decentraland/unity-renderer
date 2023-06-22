using DCL.ECS7;
using DCL.ECS7.InternalComponents;
using System.Collections.Generic;

namespace ECSSystems.UIInputSenderSystem
{
    /// <summary>
    /// Handles sending unique events from UI Elements to the scene
    /// </summary>
    public class ECSUIInputSenderSystem
    {
        internal IInternalECSComponent<InternalUIInputResults> inputResultComponent { get; }
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;

        public ECSUIInputSenderSystem(IInternalECSComponent<InternalUIInputResults> inputResultComponent, IReadOnlyDictionary<int, ComponentWriter> componentsWriter)
        {
            this.inputResultComponent = inputResultComponent;
            this.componentsWriter = componentsWriter;
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

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer))
                    continue;

                // Results are already prepared in its final form by the UI Components themselves

                while (model.Results.TryDequeue(out var result))
                {
                    writer.Put(entity.entityId, result.ComponentId, result.Message);
                }
            }
        }
    }
}
