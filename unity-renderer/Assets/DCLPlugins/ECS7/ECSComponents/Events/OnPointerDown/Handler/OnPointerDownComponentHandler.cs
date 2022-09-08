using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;

namespace DCLPlugins.ECSComponents.OnPointerDown
{
    public class OnPointerDownComponentHandler : IECSComponentHandler<PBOnPointerDown>
    {
        private PointerInputRepresentation representantion;
        private IECSComponentWriter componentWriter;
        private DataStore_ECS7 dataStore;

        private bool isAdded = false;

        public OnPointerDownComponentHandler(IECSComponentWriter componentWriter, DataStore_ECS7 dataStore)
        {
            this.dataStore = dataStore;
            this.componentWriter = componentWriter;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            if (representantion != null)
                representantion.Dispose();

            representantion = new PointerInputRepresentation(entity, dataStore, PointerEventType.Down, componentWriter, new OnPointerEventHandler(), null);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            dataStore.RemovePointerEvent(entity.entityId, representantion);
            representantion?.Dispose();
            isAdded = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerDown model)
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