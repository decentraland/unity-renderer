using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents.UIAbstractElements;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCL.ECSComponents
{
    /// <summary>
    /// Adds interactivity to UI components
    /// </summary>
    public class UIPointerEventsHandler : UIElementHandlerBase, IECSComponentHandler<PBPointerEvents>
    {
        private UIEventsSubscriptions eventsSubscriptions;

        private readonly IInternalECSComponent<InternalInputEventResults> internalInputResults;
        private readonly IInternalECSComponent<InternalPointerEvents> internalPointerEvents;

        public UIPointerEventsHandler(
            IInternalECSComponent<InternalPointerEvents> internalPointerEvents,
            IInternalECSComponent<InternalInputEventResults> internalInputResults,
            IInternalECSComponent<InternalUiContainer> internalUiContainer,
            int componentId) : base(internalUiContainer, componentId)
        {
            this.internalInputResults = internalInputResults;
            this.internalPointerEvents = internalPointerEvents;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveComponentFromEntity(scene, entity);
            eventsSubscriptions?.Dispose();
            eventsSubscriptions = null;
            internalPointerEvents.RemoveFor(scene, entity);
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBPointerEvents model)
        {
            InternalPointerEvents internalPointerEventsModel = new InternalPointerEvents();

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


            // PBPointerEvents is used for both 3D objects and UI
            // but for the former the handler is not needed.
            // TODO: how to avoid allocation of this class?

            var containerModel = internalUiContainer.GetFor(scene, entity)?.model;
            // if container model is null it's not a UI entity or it does not contain a root element
            if (containerModel == null)
                return;

            if (containerModel.components.Add(componentId))
            {
                eventsSubscriptions = UIPointerEventsUtils.AddCommonInteractivity(containerModel.rootElement, scene, entity, model.PointerEvents, internalInputResults);
                internalUiContainer.PutFor(scene, entity, containerModel);
            }
        }
    }
}
