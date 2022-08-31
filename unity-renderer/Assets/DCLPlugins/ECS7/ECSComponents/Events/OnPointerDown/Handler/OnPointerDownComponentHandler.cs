using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents.Events;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;

namespace DCLPlugins.ECSComponents.OnPointerDown
{
    public class OnPointerDownComponentHandler : IECSComponentHandler<PBOnPointerDown>
    {
        internal PointerInputRepresentantion representantion;
        private IECSComponentWriter componentWriter;
        private DataStore_ECS7 dataStore;
        private IECSContext context;

        private bool isAdded = false;

        public OnPointerDownComponentHandler(IECSComponentWriter componentWriter, DataStore_ECS7 dataStore, IECSContext context)
        {
            this.dataStore = dataStore;
            this.componentWriter = componentWriter;

            this.context = context;
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            if(representantion != null)
                representantion.Dispose();
            
            representantion = new PointerInputRepresentantion(entity, dataStore, PointerEventType.Down, componentWriter, context.systemsContext.pendingResolvePointerEvents);
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