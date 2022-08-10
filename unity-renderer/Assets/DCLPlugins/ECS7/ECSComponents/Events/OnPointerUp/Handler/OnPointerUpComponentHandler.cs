﻿using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSComponents.Utils;
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
            dataStore.entitiesOnPointerEventCounter.AddRefCount(entity.entityId);
            isAdded = false;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            OnPointerEventUtils.DisposeEvent(entity, dataStore, representantion);
            isAdded = false;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBOnPointerUp model)
        {
            representantion.SetData(scene, model.GetShowFeedback(), model.GetButton(), model.GetMaxDistance(), model.GetHoverText());
            if (!isAdded)
            {
                isAdded = true;
                dataStore.AddPointerEvent(entity.entityId, representantion);
            }
        }
    }
}