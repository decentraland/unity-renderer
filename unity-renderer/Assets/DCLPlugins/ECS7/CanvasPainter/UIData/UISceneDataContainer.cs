using System;
using DCL.ECSComponents;
using DCL.Models;

namespace DCL.ECS7.UI
{
    public class UISceneDataContainer
    {
        public event Action<IDCLEntity> OnUITransformRemoved;
        
        public readonly BaseDictionary<long,PBUiTransform> sceneCanvasTransform = new BaseDictionary<long,PBUiTransform>();
        public readonly BaseDictionary<long,PBUiText> sceneCanvasText = new BaseDictionary<long,PBUiText>();
        
        private bool isDirty = false;

        public bool IsDirty() => isDirty;

        public void UIRendered() => isDirty = false;
        
        public void AddUIComponent(IDCLEntity entity, PBUiTransform model)
        {
            isDirty = true;
            sceneCanvasTransform[entity.entityId] = model;
        }
                
        public void AddUIComponent(IDCLEntity entity, PBUiText model)
        {
            isDirty = true;
            sceneCanvasText[entity.entityId] = model;
        }

        public void RemoveUITransform(IDCLEntity entity)
        {
            isDirty = true;
            sceneCanvasTransform.Remove(entity.entityId);
            OnUITransformRemoved?.Invoke(entity);
        }

        public void RemoveUIText(IDCLEntity entity)
        {
            isDirty = true;
            sceneCanvasText.Remove(entity.entityId);
        }
    }

}