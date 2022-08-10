using System;
using DCL.Controllers;
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

    public interface IUIDataContainer
    {
        void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiTransform model);
        void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiText model);
        void RemoveUITransform(IParcelScene scene, IDCLEntity entity);
        void RemoveUIText(IParcelScene scene, IDCLEntity entity);
        UISceneDataContainer GetDataContainer(IParcelScene scene);
    }
    
    public class UIDataContainer : IUIDataContainer
    {
        public readonly BaseDictionary<string, UISceneDataContainer> sceneData = new BaseDictionary<string, UISceneDataContainer>();

        public void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            GetDataContainer(scene).AddUIComponent(entity,model);
        }
                
        public void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiText model)
        {
            GetDataContainer(scene).AddUIComponent(entity,model);
        }

        public void RemoveUITransform(IParcelScene scene, IDCLEntity entity)
        {
            GetDataContainer(scene).RemoveUITransform(entity);    
        }
        
        public void RemoveUIText(IParcelScene scene, IDCLEntity entity)
        {
            GetDataContainer(scene).RemoveUIText(entity);
        }

        public UISceneDataContainer GetDataContainer(IParcelScene scene)
        {
            var sceneId = scene.sceneData.id;
            
            if(sceneData.TryGetValue(sceneId, out UISceneDataContainer sceneDataContainer))
            {
                return sceneDataContainer;
            }
            else
            {
                UISceneDataContainer newSceneDataContainer = new UISceneDataContainer();
                sceneData[sceneId] = newSceneDataContainer;
                return newSceneDataContainer;
            }
        }
    }
}