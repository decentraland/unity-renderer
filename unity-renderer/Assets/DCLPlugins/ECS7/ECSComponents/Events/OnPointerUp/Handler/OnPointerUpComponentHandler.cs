using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;

namespace DCL.ECSComponents.OnPointerUp
{
    public class OnPointerUpComponentHandler : IECSComponentHandler<PBOnPointerUp>
    {
        private PointerInputRepresentantion representantion;
        private IECSComponentWriter componentWriter;
        private DataStore_ECS7 dataStore;
        
        private bool isAdded = false;

        public OnPointerUpComponentHandler(IECSComponentWriter componentWriter, DataStore_ECS7 dataStore)
        {
            this.dataStore = dataStore;
            this.componentWriter = componentWriter;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            if(representantion != null)
                representantion.Dispose();
            
            representantion = new PointerInputRepresentantion(entity, dataStore, PointerInputEventType.UP, componentWriter);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            representantion?.Dispose();
            dataStore.RemovePointerEvent(entity.entityId,representantion);
            isAdded = false;
        }
        
        
        private PBOnPointerUp NormalizeAndClone(PBOnPointerUp model)
        {
            PBOnPointerUp normalizedModel = model.Clone();
            
            normalizedModel.ShowFeedback = !model.HasShowFeedback || model.ShowFeedback;
            normalizedModel.Distance = model.HasDistance ? model.Distance : 10.0f;
            normalizedModel.HoverText = model.HasHoverText ? model.HoverText : "Interact";
            normalizedModel.Button = model.HasButton ? model.Button : ActionButton.Any;
            
            return normalizedModel;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerUp model)
        {
            PBOnPointerUp normalizedModel = NormalizeAndClone(model);
            representantion.SetData(scene, entity, normalizedModel.ShowFeedback, normalizedModel.Button, normalizedModel.Distance, normalizedModel.HoverText);
            if (!isAdded)
            {
                isAdded = true;
                dataStore.AddPointerEvent(entity.entityId, representantion);
            }
        }
    }
}