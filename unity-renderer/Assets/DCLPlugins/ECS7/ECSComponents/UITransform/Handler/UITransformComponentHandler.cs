using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;

namespace DCL.ECSComponents
{
    public class UITransformComponentHandler : IECSComponentHandler<PBUiTransform>
    {
        private readonly DataStore_ECS7 dataStore;
        private readonly IUpdateEventHandler updateEventHandler;
        
        private List<PBUiTransform> models = new List<PBUiTransform>();
        private Dictionary<long, PBUiTransform> transforms = new Dictionary<long, PBUiTransform>();
        
        public UITransformComponentHandler(DataStore_ECS7 dataStore, IUpdateEventHandler updateEventHandler)
        {
            this.dataStore = dataStore;
            this.updateEventHandler = updateEventHandler;
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.LateUpdate, DrawUI);
        }
        
        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            
        }
        
        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.LateUpdate, DrawUI);
        }
        
        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBUiTransform model)
        {
            dataStore.sceneCanvas[scene.sceneData.id][entity.entityId] = model;
        }


        private void DrawUI()
        {
            
        }
    }
}