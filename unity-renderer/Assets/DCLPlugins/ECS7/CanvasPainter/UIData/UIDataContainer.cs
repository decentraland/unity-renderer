using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;

namespace DCL.ECS7.UI
{
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