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
        public readonly BaseDictionary<int, UISceneDataContainer> sceneData = new BaseDictionary<int, UISceneDataContainer>();

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
            if(sceneData.TryGetValue(scene.sceneData.sceneNumber, out UISceneDataContainer sceneDataContainer))
            {
                return sceneDataContainer;
            }
            else
            {
                UISceneDataContainer newSceneDataContainer = new UISceneDataContainer();
                sceneData[scene.sceneData.sceneNumber] = newSceneDataContainer;
                return newSceneDataContainer;
            }
        }
    }
}