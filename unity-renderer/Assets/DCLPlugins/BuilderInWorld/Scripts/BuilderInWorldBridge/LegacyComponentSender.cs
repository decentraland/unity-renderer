using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using UnityEngine;
using static ProtocolV2;

namespace DCL.Builder
{
    public class LegacyComponentSender : IComponentSendHandler
    {
        //This is done for optimization purposes, recreating new objects can increase garbage collection
        private TransformComponent entityTransformComponentModel = new TransformComponent();
        private EntityPayload entityPayload = new EntityPayload();
        private ModifyEntityComponentEvent modifyEntityComponentEvent = new ModifyEntityComponentEvent();
        private EntitySingleComponentPayload entitySingleComponentPayload = new EntitySingleComponentPayload();
        
        public void SendNewEntityToKernel(string sceneId, long entityId, ComponentPayload[] componentsPayload)
        {
            AddEntityEvent addEntityEvent = new AddEntityEvent();
            entityPayload.entityId = entityId;
            entityPayload.components = componentsPayload;

            addEntityEvent.payload = entityPayload;

            WebInterface.SceneEvent<AddEntityEvent> sceneEvent = new WebInterface.SceneEvent<AddEntityEvent>();
            sceneEvent.sceneId = sceneId;
            sceneEvent.eventType = BIWSettings.STATE_EVENT_NAME;
            sceneEvent.payload = addEntityEvent;

            //Note(Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
            string message = JsonConvert.SerializeObject(sceneEvent);
            WebInterface.BuilderInWorldMessage(BIWSettings.SCENE_EVENT_NAME, message);
        }

        public void ChangeEntityComponent(EntitySingleComponentPayload payload, IParcelScene scene)
        {
            modifyEntityComponentEvent.payload = payload;

            WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.eventType = BIWSettings.STATE_EVENT_NAME;
            sceneEvent.payload = modifyEntityComponentEvent;

            //Note (Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
            string message = JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            WebInterface.BuilderInWorldMessage(BIWSettings.SCENE_EVENT_NAME, message);
        }
        
        public void EntityTransformReport(IDCLEntity entity, IParcelScene scene) {  
            
            entitySingleComponentPayload.entityId = entity.entityId;
            entitySingleComponentPayload.componentId = (int) CLASS_ID_COMPONENT.TRANSFORM;

            entityTransformComponentModel.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
            entityTransformComponentModel.rotation = new QuaternionRepresentation(entity.gameObject.transform.rotation);
            entityTransformComponentModel.scale = entity.gameObject.transform.lossyScale;

            entitySingleComponentPayload.data = entityTransformComponentModel;

            modifyEntityComponentEvent.payload = entitySingleComponentPayload;

            WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.eventType = BIWSettings.STATE_EVENT_NAME;
            sceneEvent.payload = modifyEntityComponentEvent;

            //Note (Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
            string message = JsonConvert.SerializeObject(sceneEvent);
            WebInterface.BuilderInWorldMessage(BIWSettings.SCENE_EVENT_NAME, message);
        }
        
        public void RemoveEntityOnKernel(long entityId, IParcelScene scene)
        {  
            RemoveEntityEvent removeEntityEvent = new RemoveEntityEvent();
            RemoveEntityPayload removeEntityPayLoad = new RemoveEntityPayload();
            removeEntityPayLoad.entityId = entityId;
            removeEntityEvent.payload = removeEntityPayLoad;

            WebInterface.SendSceneEvent(scene.sceneData.id, BIWSettings.STATE_EVENT_NAME, removeEntityEvent);
        }
    }
}

