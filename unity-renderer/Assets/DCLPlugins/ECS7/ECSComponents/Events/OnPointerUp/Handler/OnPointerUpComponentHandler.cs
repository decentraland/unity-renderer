using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCLPlugins.ECSComponents;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

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
            dataStore.entitiesOnPointerEvent.AddRefCount(entity.entityId);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            dataStore.entitiesOnPointerEvent.RemoveRefCount(entity.entityId);
            if (!dataStore.entitiesOnPointerEvent.ContainsKey(entity.entityId))
            {
                List<GameObject> collidersToDestroy = dataStore.entityOnPointerEventColliderGameObject[entity.entityId];
                for (int x = 0; x < collidersToDestroy.Count; x++)
                {
                    GameObject.Destroy(collidersToDestroy[x]);
                }
                dataStore.entityOnPointerEventColliderGameObject.Remove(entity.entityId);
            }

            representantion?.Dispose();
            dataStore.RemovePointerEvent(entity.entityId,representantion);
            isAdded = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerUp model)
        {
            representantion.SetData(scene, model.ShowFeedback, model.Button, model.Distance, model.HoverText);
            if (!isAdded)
            {
                isAdded = true;
                dataStore.AddPointerEvent(entity.entityId, representantion);
            }
        }
    }
}