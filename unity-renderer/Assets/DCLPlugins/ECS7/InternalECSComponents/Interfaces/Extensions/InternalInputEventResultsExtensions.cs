using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;

namespace DCL.ECS7.InternalComponents
{
    public static class InternalInputEventResultsExtensions
    {
        internal const int MAX_AMOUNT_OF_POINTER_EVENTS_SENT = 30;

        public static void AddEvent(this IInternalECSComponent<InternalInputEventResults> component, IParcelScene scene,
            InternalInputEventResults.EventData data)
        {
            var model = component.GetFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY)?.model ??
                        new InternalInputEventResults()
                        {
                            lastTimestamp = 0,
                            events = new Queue<InternalInputEventResults.EventData>(MAX_AMOUNT_OF_POINTER_EVENTS_SENT)
                        };

            if (model.events.Count >= MAX_AMOUNT_OF_POINTER_EVENTS_SENT)
            {
                //drop oldest event
                model.events.Dequeue();
            }

            data.timestamp = model.lastTimestamp++;
            model.events.Enqueue(data);

            component.PutFor(scene, SpecialEntityId.SCENE_ROOT_ENTITY, model);
        }
    }
}