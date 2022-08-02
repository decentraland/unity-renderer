using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;

namespace DCL.ECS7.UI
{
    public class UISceneDataContainer
    {
        public readonly BaseDictionary<long,PBUiTransform> sceneCanvasTransform = new BaseDictionary<long,PBUiTransform>();
        public readonly BaseDictionary<long,PBUiTextShape> sceneCanvasText = new BaseDictionary<long,PBUiTextShape>();
        
        private bool isDirty = false ;

        public bool IsDirty() => isDirty;
        
        public void AddUIComponent(IDCLEntity entity, PBUiTransform model)
        {
            isDirty = true;
            sceneCanvasTransform[entity.entityId] = model;
        }
                
        public void AddUIComponent(IDCLEntity entity, PBUiTextShape model)
        {
            isDirty = true;
            sceneCanvasText[entity.entityId] = model;
        }

        public void RemoveUIComponent(IDCLEntity entity)
        {
            isDirty = true;
            sceneCanvasTransform.Remove(entity.entityId);
            sceneCanvasText.Remove(entity.entityId);
        }
    }
    
    public class UIDataContainer
    {
        public readonly BaseDictionary<string, UISceneDataContainer> sceneData = new BaseDictionary<string, UISceneDataContainer>();

        public void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            GetDataContainer(scene).AddUIComponent(entity,model);
        }
                
        public void AddUIComponent(IParcelScene scene, IDCLEntity entity, PBUiTextShape model)
        {
            GetDataContainer(scene).AddUIComponent(entity,model);
        }
        
        public void RemoveUIComponent(IParcelScene scene, IDCLEntity entity)
        {
            GetDataContainer(scene).RemoveUIComponent(entity);
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