using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ProtocolV2;

/// <summary>
/// This class will handle all the messages that will be sent to kernel. 
/// </summary>
public class BuilderInWorldBridge : MonoBehaviour
{

    //This is done for optimization purposes, recreating new objects can increase garbage collection
    TransformComponent entityTransformComponentModel = new TransformComponent();

    StoreSceneStateEvent storeSceneState = new StoreSceneStateEvent();
    ModifyEntityComponentEvent modifyEntityComponentEvent = new ModifyEntityComponentEvent();
    EntityPayload entityPayload = new EntityPayload();
    EntitySingleComponentPayload entitySingleComponentPayload = new EntitySingleComponentPayload();

    public void ChangedEntityName(DCLBuilderInWorldEntity entity, ParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID.NAME;


        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in entity.rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.NAME)
            {       
                entitySingleComponentPayload.data = ((DCLName)keyValuePairBaseDisposable.Value).GetModel();
            }
        }
       

        modifyEntityComponentEvent.payload = entitySingleComponentPayload;

        WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.eventType = BuilderInWorldSettings.STATE_EVENT_NAME;
        sceneEvent.payload = modifyEntityComponentEvent;


        //Note (Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
        string message = JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        WebInterface.BuilderInWorldMessage(BuilderInWorldSettings.SCENE_EVENT_NAME, message);
    }

    public void AddEntityOnKernel(DecentralandEntity entity, ParcelScene scene)
    {
        List<ComponentPayload> list = new List<ComponentPayload>();
        foreach (KeyValuePair<CLASS_ID_COMPONENT, BaseComponent> keyValuePair in entity.components)
        {
            ComponentPayload componentPayLoad = new ComponentPayload();
            componentPayLoad.componentId = Convert.ToInt32(keyValuePair.Key);

            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {             
                TransformComponent entityComponentModel = new TransformComponent();

                entityComponentModel.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                entityComponentModel.rotation = new QuaternionRepresentation(entity.gameObject.transform.rotation);
                entityComponentModel.scale = entity.gameObject.transform.localScale;

                componentPayLoad.data = entityComponentModel;
            }
            else
            {
                componentPayLoad.data = keyValuePair.Value.GetModel();
            }

            list.Add(componentPayLoad);


        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in entity.GetSharedComponents())
        {
            ComponentPayload componentPayLoad = new ComponentPayload();

            componentPayLoad.componentId = keyValuePairBaseDisposable.Value.GetClassId();
            componentPayLoad.data = keyValuePairBaseDisposable.Value.GetModel();

            list.Add(componentPayLoad);
        }

        SendNewEntityToKernel(scene.sceneData.id, entity.entityId, list.ToArray());
    }

    public void EntityTransformReport(DecentralandEntity entity, ParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID_COMPONENT.TRANSFORM;

        entityTransformComponentModel.position = SceneController.i.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
        entityTransformComponentModel.rotation = new QuaternionRepresentation(entity.gameObject.transform.rotation);
        entityTransformComponentModel.scale = entity.gameObject.transform.localScale;

        entitySingleComponentPayload.data = entityTransformComponentModel;

        modifyEntityComponentEvent.payload = entitySingleComponentPayload;

        WebInterface.SceneEvent<ModifyEntityComponentEvent> sceneEvent = new WebInterface.SceneEvent<ModifyEntityComponentEvent>();
        sceneEvent.sceneId = scene.sceneData.id;
        sceneEvent.eventType = BuilderInWorldSettings.STATE_EVENT_NAME;
        sceneEvent.payload = modifyEntityComponentEvent;


        //Note (Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
    
        string message = JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });


        WebInterface.BuilderInWorldMessage(BuilderInWorldSettings.SCENE_EVENT_NAME, message);
    }

    public void RemoveEntityOnKernel(string entityId, ParcelScene scene)
    {
        RemoveEntityEvent removeEntityEvent = new RemoveEntityEvent();
        RemoveEntityPayload removeEntityPayLoad = new RemoveEntityPayload();
        removeEntityPayLoad.entityId = entityId;
        removeEntityEvent.payload = removeEntityPayLoad;

        WebInterface.SendSceneEvent(scene.sceneData.id, BuilderInWorldSettings.STATE_EVENT_NAME, removeEntityEvent);
    }

    public void StartKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StartStatefulMode(scene.sceneData.id));
    }

    public void ExitKernelEditMode(ParcelScene scene)
    {
        WebInterface.ReportControlEvent(new WebInterface.StopStatefulMode(scene.sceneData.id));
    }

    public void PublishScene(ParcelScene scene)
    {
        WebInterface.SendSceneEvent(scene.sceneData.id, BuilderInWorldSettings.STATE_EVENT_NAME, storeSceneState);
    }

    void SendNewEntityToKernel(string sceneId, string entityId, ComponentPayload[] componentsPayload)
    {
        AddEntityEvent addEntityEvent = new AddEntityEvent();
        entityPayload.entityId = entityId;
        entityPayload.components = componentsPayload;

        addEntityEvent.payload = entityPayload;

        WebInterface.SceneEvent<AddEntityEvent> sceneEvent = new WebInterface.SceneEvent<AddEntityEvent>();
        sceneEvent.sceneId = sceneId;
        sceneEvent.eventType = BuilderInWorldSettings.STATE_EVENT_NAME;
        sceneEvent.payload = addEntityEvent;


        //Note (Adrian): We use Newtonsoft instead of JsonUtility because we need to deal with super classes, JsonUtility doesn't encode them
        string message = Newtonsoft.Json.JsonConvert.SerializeObject(sceneEvent, Formatting.None, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });

        WebInterface.BuilderInWorldMessage(BuilderInWorldSettings.SCENE_EVENT_NAME, message);
    }
}
