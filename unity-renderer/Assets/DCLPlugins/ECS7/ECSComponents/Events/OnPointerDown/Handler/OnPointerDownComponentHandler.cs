using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown.Handler
{
    public class OnPointerDownComponentHandler : IECSComponentHandler<PBOnPointerDown>
    {
        private PointerInputRepresentantion representantion;
        private IECSComponentWriter componentWriter;

        private bool isAdded = false;

        public OnPointerDownComponentHandler(IECSComponentWriter componentWriter)
        {
            this.componentWriter = componentWriter;
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            representantion = new PointerInputRepresentantion(PointerInputEventType.DOWN, componentWriter);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            representantion?.Dispose();
            DataStore.i.ecs7.RemovePointerEvent(entity.entityId,representantion);
            isAdded = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerDown model)
        {
            representantion.SetData(scene, entity, model);
            if(!isAdded)
                DataStore.i.ecs7.AddPointerEvent(entity.entityId,representantion);
        }
    }
}