using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class PointerEventsHandler : IECSComponentHandler<PBPointerEvents>
    {
        private readonly IInternalECSComponent<InternalPointerEvents> internalPointerEvents;

        public PointerEventsHandler(IInternalECSComponent<InternalPointerEvents> internalPointerEvents)
        {
            this.internalPointerEvents = internalPointerEvents;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            internalPointerEvents.RemoveFor(scene, entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBPointerEvents model)
        {
            var internalPointerEventsModel = new InternalPointerEvents(new List<InternalPointerEvents.Entry>());

            for (int i = 0; i < model.PointerEvents.Count; i++)
            {
                var pointerEvent = model.PointerEvents[i];

                InternalPointerEvents.Info info = new InternalPointerEvents.Info(
                    pointerEvent.EventInfo.GetButton(),
                    pointerEvent.EventInfo.GetHoverText(),
                    pointerEvent.EventInfo.GetMaxDistance(),
                    pointerEvent.EventInfo.GetShowFeedback());

                InternalPointerEvents.Entry entry = new InternalPointerEvents.Entry(pointerEvent.EventType, info);
                internalPointerEventsModel.PointerEvents.Add(entry);
            }

            internalPointerEvents.PutFor(scene, entity, internalPointerEventsModel);
        }
    }
}
