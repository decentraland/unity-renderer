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
    public class UITextComponentHandler : IECSComponentHandler<PBUiTextShape>
    {
        private readonly UIDataContainer uiDataContainer;
        
        public UITextComponentHandler(UIDataContainer dataContainer)
        {
            this.uiDataContainer = dataContainer;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
        }
        
        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            uiDataContainer.RemoveUIText(scene,entity);
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiTextShape model)
        {
            uiDataContainer.AddUIComponent(scene,entity, model);
        }
    }
}