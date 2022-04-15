using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;
using static ProtocolV2;

/// <summary>
/// This class will handle all the messages that will be sent to kernel.
/// </summary>
public class BuilderInWorldBridge : MonoBehaviour
{

    //Note Adrian: OnKernelUpdated in not called in the update of the transform, since it will give a lot of 
    //events and probably dont need to get called with that frecuency
    public event Action OnKernelUpdated;
    public event Action<bool, string> OnPublishEnd;
    public event Action<RequestHeader> OnHeadersReceived;

    //This is done for optimization purposes, recreating new objects can increase garbage collection
    private TransformComponent entityTransformComponentModel = new TransformComponent();

    private PublishPayload payload = new PublishPayload();
    private ModifyEntityComponentEvent modifyEntityComponentEvent = new ModifyEntityComponentEvent();
    private EntityPayload entityPayload = new EntityPayload();
    private EntitySingleComponentPayload entitySingleComponentPayload = new EntitySingleComponentPayload();

    #region MessagesFromKernel

    public void PublishSceneResult(string payload)
    {
        PublishSceneResultPayload publishSceneResultPayload = JsonUtility.FromJson<PublishSceneResultPayload>(payload);

        if (publishSceneResultPayload.ok)
        {
            OnPublishEnd?.Invoke(true, "");

            AudioScriptableObjects.confirm.Play();
        }
        else
        {
            OnPublishEnd?.Invoke(false, publishSceneResultPayload.error);

            AudioScriptableObjects.error.Play();
        }
    }

    public void AddAssets(string payload)
    {
        //We remove the old assets to they don't collide with the new ones
        BIWUtils.RemoveAssetsFromCurrentScene();

        AssetCatalogBridge.i.AddScenesObjectToSceneCatalog(payload);
    }

    public void RequestedHeaders(string payload) { OnHeadersReceived?.Invoke(JsonConvert.DeserializeObject<RequestHeader>(payload)); }

    #endregion

    #region MessagesToKernel

    public void AskKernelForCatalogHeadersWithParams(string method, string url) { WebInterface.SendRequestHeadersForUrl(BIWSettings.BIW_HEADER_REQUEST_WITH_PARAM_EVENT_NAME, method, url); }

    public void UpdateSmartItemComponent(BIWEntity entity, IParcelScene scene)
    {
        SmartItemComponent smartItemComponent = entity.rootEntity.TryGetComponent<SmartItemComponent>();
        if (smartItemComponent == null)
            return;

        entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID_COMPONENT.SMART_ITEM;

        entitySingleComponentPayload.data = smartItemComponent.GetValues();

        ChangeEntityComponent(entitySingleComponentPayload, scene);
    }

    public void ChangeEntityLockStatus(BIWEntity entity, IParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID.LOCKED_ON_EDIT;

        foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in entity.rootEntity.sharedComponents)
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.LOCKED_ON_EDIT)
            {
                entitySingleComponentPayload.data = ((DCLLockedOnEdit) keyValuePairBaseDisposable.Value).GetModel();
            }
        }

        ChangeEntityComponent(entitySingleComponentPayload, scene);
    }

    public void ChangedEntityName(BIWEntity entity, IParcelScene scene)
    {
        entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
        entitySingleComponentPayload.componentId = (int) CLASS_ID.NAME;

        foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in entity.rootEntity.sharedComponents)
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NAME)
            {
                entitySingleComponentPayload.data = ((DCLName) keyValuePairBaseDisposable.Value).GetModel();
            }
        }

        ChangeEntityComponent(entitySingleComponentPayload, scene);
    }

    void ChangeEntityComponent(EntitySingleComponentPayload payload, IParcelScene scene)
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
        OnKernelUpdated?.Invoke();
    }

    public void AddEntityOnKernel(IDCLEntity entity, IParcelScene scene)
    {
        if (scene == null)
            return;

        List<ComponentPayload> list = new List<ComponentPayload>();

        foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> keyValuePair in entity.components)
        {
            ComponentPayload componentPayLoad = new ComponentPayload();
            componentPayLoad.componentId = Convert.ToInt32(keyValuePair.Key);

            if (keyValuePair.Key == CLASS_ID_COMPONENT.TRANSFORM)
            {
                TransformComponent entityComponentModel = new TransformComponent();

                entityComponentModel.position = WorldStateUtils.ConvertUnityToScenePosition(entity.gameObject.transform.position, scene);
                entityComponentModel.rotation = new QuaternionRepresentation(entity.gameObject.transform.rotation);
                entityComponentModel.scale = entity.gameObject.transform.lossyScale;

                componentPayLoad.data = entityComponentModel;
            }
            else
            {
                componentPayLoad.data = keyValuePair.Value.GetModel();
            }

            list.Add(componentPayLoad);
        }

        foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in entity.sharedComponents)
        {
            ComponentPayload componentPayLoad = new ComponentPayload();

            componentPayLoad.componentId = keyValuePairBaseDisposable.Value.GetClassId();

            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
            {
                NFTComponent nftComponent = new NFTComponent();
                NFTShape.Model model = (NFTShape.Model) keyValuePairBaseDisposable.Value.GetModel();

                nftComponent.color = new ColorRepresentation(model.color);
                nftComponent.assetId = model.assetId;
                nftComponent.src = model.src;
                nftComponent.style = model.style;

                componentPayLoad.data = nftComponent;
            }
            else
            {
                componentPayLoad.data = keyValuePairBaseDisposable.Value.GetModel();
            }

            list.Add(componentPayLoad);
        }

        SendNewEntityToKernel(scene.sceneData.id, entity.entityId, list.ToArray());
    }

    public void EntityTransformReport(IDCLEntity entity, IParcelScene scene)
    {
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

    public void RemoveEntityOnKernel(string entityId, IParcelScene scene)
    {
        RemoveEntityEvent removeEntityEvent = new RemoveEntityEvent();
        RemoveEntityPayload removeEntityPayLoad = new RemoveEntityPayload();
        removeEntityPayLoad.entityId = entityId;
        removeEntityEvent.payload = removeEntityPayLoad;

        WebInterface.SendSceneEvent(scene.sceneData.id, BIWSettings.STATE_EVENT_NAME, removeEntityEvent);
        OnKernelUpdated?.Invoke();
    }

    public void StartIsolatedMode(ILand land)
    {
        IsolatedConfig config = new IsolatedConfig();
        config.mode = IsolatedMode.BUILDER;

        IsolatedBuilderConfig builderConfig = new IsolatedBuilderConfig();

        builderConfig.sceneId = land.sceneId;
        builderConfig.recreateScene = true;
        builderConfig.killPortableExperiences = true;
        builderConfig.land = land;

        config.payload = builderConfig;
        WebInterface.StartIsolatedMode(config);
    }

    public void StopIsolatedMode()
    {
        IsolatedConfig config = new IsolatedConfig();
        config.mode = IsolatedMode.BUILDER;
        WebInterface.StopIsolatedMode(config);
    }

    public void PublishScene(Dictionary<string, object > filesToDecode, Dictionary<string, object > files, CatalystSceneEntityMetadata metadata, StatelessManifest statelessManifest, bool publishFromPanel )
    {
        payload.filesToDecode = filesToDecode;
        payload.files = files;
        payload.metadata = metadata;
        payload.pointers = metadata.scene.parcels;
        payload.statelessManifest = statelessManifest;
        payload.reloadSingleScene = publishFromPanel;
        
        WebInterface.PublishStatefulScene(payload);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    void SendNewEntityToKernel(string sceneId, string entityId, ComponentPayload[] componentsPayload)
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
        OnKernelUpdated?.Invoke();
    }

    #endregion
}