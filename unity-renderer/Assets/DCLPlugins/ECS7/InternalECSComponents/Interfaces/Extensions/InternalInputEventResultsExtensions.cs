using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalInputEventResultsExtensions
    {
        public static void AddEvent(this IInternalECSComponent<InternalInputEventResults> component, IParcelScene scene,
            InternalInputEventResults.EventData data)
        {
            var entityId = data.hit.HasEntityId ? data.hit.EntityId : SpecialEntityId.SCENE_ROOT_ENTITY;
            var model = component.GetFor(scene, entityId)?.model ??
                        new InternalInputEventResults()
                        {
                            lastTimestamp = 0,
                            events = new Queue<InternalInputEventResults.EventData>()
                        };

            data.timestamp = model.lastTimestamp++;
            model.events.Enqueue(data);

            component.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
        }
    }
}
