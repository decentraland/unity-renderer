using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;

namespace DCLPlugins.ECSComponents.OnPointerDown
{
    public class OnPointerDownComponentHandler : IECSComponentHandler<PBOnPointerDown>
    {
        private PointerInputRepresentantion representantion;
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
            if(representantion != null)
                representantion.Dispose();
            
            representantion = new PointerInputRepresentantion(entity, dataStore, PointerInputEventType.DOWN, componentWriter);
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
            representantion.SetData(scene, entity, model.ShowFeedback, model.Button, model.Distance, model.HoverText);
            if (!isAdded)
            {
                isAdded = true;
                dataStore.AddPointerEvent(entity.entityId, representantion);
            }
        }
    }
}