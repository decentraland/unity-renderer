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
            var entityId = data.hit.EntityId == 0 ? SpecialEntityId.SCENE_ROOT_ENTITY : data.hit.EntityId;
            var model = component.GetFor(scene, entityId)?.model ??
                        new InternalInputEventResults()
                        {
                            lastTimestamp = 0,
                            events = new Queue<InternalInputEventResults.EventData>()
                        };

            model.events.Enqueue(data);

            component.PutFor(scene, entityId, model);
        }
    }
}
