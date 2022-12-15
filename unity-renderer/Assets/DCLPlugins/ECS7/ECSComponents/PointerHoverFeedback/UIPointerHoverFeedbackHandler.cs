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
    public class UIPointerHoverFeedbackHandler : UIElementHandlerBase, IECSComponentHandler<PBPointerHoverFeedback>
    {
        private UIEventsSubscriptions eventsSubscriptions;

        private readonly IInternalECSComponent<InternalInputEventResults> internalInputResults;

        public UIPointerHoverFeedbackHandler(
            IInternalECSComponent<InternalInputEventResults> internalInputResults,
            IInternalECSComponent<InternalUiContainer> internalUiContainer,
            int componentId) : base(internalUiContainer, componentId)
        {
            this.internalInputResults = internalInputResults;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            RemoveComponentFromRoot(scene, entity);
            eventsSubscriptions?.Dispose();
            eventsSubscriptions = null;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBPointerHoverFeedback model)
        {
            // PBPointerHoverFeedback is used for both 3D objects and UI
            // but for the former the handler is not needed.
            // TODO: how to avoid allocation of this class?

            var containerModel = internalUiContainer.GetFor(scene, entity)?.model;
            // if container model is null it's not a UI entity or it does not contain a root element
            if (containerModel == null)
                return;

            containerModel.components.Add(componentId);
            eventsSubscriptions = UIPointerEventsUtils.AddCommonInteractivity(containerModel.rootElement, scene, entity, model.PointerEvents, internalInputResults);
            internalUiContainer.PutFor(scene, entity, containerModel);
        }
    }
}
