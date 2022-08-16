using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECS7.UI;
using DCL.ECSRuntime;
using DCL.Models;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace DCL.ECSComponents
{
    public class UITransformComponentHandler : IECSComponentHandler<PBUiTransform>
    {
        private readonly IUIDataContainer uiDataContainer;
        
        public UITransformComponentHandler(IUIDataContainer dataContainer)
        {
            this.uiDataContainer = dataContainer;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }
        
        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            // If we remove an UITransform from a entity, it should have the root entity as parent
            entity.parentId = SpecialEntityId.SCENE_ROOT_ENTITY;
            uiDataContainer.RemoveUITransform(scene,entity);
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            entity.parentId = model.Parent;

            uiDataContainer.AddUIComponent(scene,entity, model);
        }
    }
}