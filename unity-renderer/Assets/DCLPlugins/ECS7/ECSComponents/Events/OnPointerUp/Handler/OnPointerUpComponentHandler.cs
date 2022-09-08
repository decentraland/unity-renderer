using DCL.Components;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;

namespace DCL.ECSComponents.OnPointerUp
{
    public class OnPointerUpComponentHandler : IECSComponentHandler<PBOnPointerUp>
    {
        private PointerInputRepresentation representantion;
        private IECSComponentWriter componentWriter;
        private DataStore_ECS7 dataStore;
        private ECSContext context;

        private bool isAdded = false;

        public OnPointerUpComponentHandler(IECSComponentWriter componentWriter, DataStore_ECS7 dataStore, ECSContext context)
        {
            this.context = context;
            this.dataStore = dataStore;
            this.componentWriter = componentWriter;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            if (representantion != null)
                representantion.Dispose();

            representantion = new PointerInputRepresentation(entity, dataStore, PointerEventType.Up, componentWriter, new OnPointerEventHandler(), context.systemsContext.pendingResolvingPointerEvents);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            representantion?.Dispose();
            dataStore.RemovePointerEvent(entity.entityId, representantion);
            isAdded = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerUp model)
        {
            representantion.SetData(scene, entity, model.GetShowFeedback(), model.GetButton(), model.GetMaxDistance(), model.GetHoverText());
            if (!isAdded)
            {
                isAdded = true;
                dataStore.AddPointerEvent(entity.entityId, representantion);
            }
        }
    }
}