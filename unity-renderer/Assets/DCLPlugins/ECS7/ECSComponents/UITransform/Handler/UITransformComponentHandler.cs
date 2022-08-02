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
        private readonly UIDataContainer uiDataContainer;
        
        public UITransformComponentHandler(UIDataContainer dataContainer)
        {
            this.uiDataContainer = dataContainer;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }
        
        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            if (entity.parentId != (long)SpecialEntityId.SCENE_ROOT_ENTITY)
            {
                entity.parentId = (long)SpecialEntityId.SCENE_ROOT_ENTITY;
                ECSTransformUtils.SetParent(scene, entity, (long)SpecialEntityId.SCENE_ROOT_ENTITY);
            }
            uiDataContainer.RemoveUIComponent(scene,entity);
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            if (model.ParentEntity != SpecialEntityId.SCENE_ROOT_ENTITY || entity.parentId !=  model.ParentEntity)
            {
                entity.parentId = model.ParentEntity;
                ECSTransformUtils.SetParent(scene, entity, model.ParentEntity);
            }

            uiDataContainer.AddUIComponent(scene,entity, model);
        }
    }
}