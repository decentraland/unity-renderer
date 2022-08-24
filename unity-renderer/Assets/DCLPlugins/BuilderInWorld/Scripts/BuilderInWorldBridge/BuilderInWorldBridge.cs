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

namespace DCL.Builder
{
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

        private PublishPayload payload = new PublishPayload();
        private EntitySingleComponentPayload entitySingleComponentPayload = new EntitySingleComponentPayload();

        private IComponentSendHandler componentSendHandler;

        private void Awake()
        {
            componentSendHandler = new LegacyComponentSender();
        }

        public void SetScene(IBuilderScene builderScene)
        {
            switch (builderScene.sceneVersion)
            {
                case IBuilderScene.SceneVersion.LEGACY:
                    componentSendHandler = new LegacyComponentSender();
                    break;
                case IBuilderScene.SceneVersion.ECS:
                    componentSendHandler = new ECSComponentSenderHandler();
                    break;
            }
        }

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
            if (scene.componentsManagerLegacy.TryGetBaseComponent(entity.rootEntity, CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
                return;

            entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
            entitySingleComponentPayload.componentId = (int) CLASS_ID_COMPONENT.SMART_ITEM;

            entitySingleComponentPayload.data = ((SmartItemComponent)component).GetValues();

            ChangeEntityComponent(entitySingleComponentPayload, scene);
        }

        public void ChangeEntityLockStatus(BIWEntity entity, IParcelScene scene)
        {
            entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
            entitySingleComponentPayload.componentId = (int) CLASS_ID.LOCKED_ON_EDIT;

            if (scene.componentsManagerLegacy.TryGetSharedComponent(entity.rootEntity, CLASS_ID.LOCKED_ON_EDIT, out ISharedComponent component))
            {
                entitySingleComponentPayload.data = ((DCLLockedOnEdit) component).GetModel();
            }

            ChangeEntityComponent(entitySingleComponentPayload, scene);
        }

        public void ChangedEntityName(BIWEntity entity, IParcelScene scene)
        {
            entitySingleComponentPayload.entityId = entity.rootEntity.entityId;
            entitySingleComponentPayload.componentId = (int) CLASS_ID.NAME;

            if (scene.componentsManagerLegacy.TryGetSharedComponent(entity.rootEntity, CLASS_ID.NAME, out ISharedComponent component))
            {
                entitySingleComponentPayload.data = ((DCLName) component).GetModel();
            }

            ChangeEntityComponent(entitySingleComponentPayload, scene);
        }

        void ChangeEntityComponent(EntitySingleComponentPayload payload, IParcelScene scene)
        {
            componentSendHandler.ChangeEntityComponent(payload,scene);
            OnKernelUpdated?.Invoke();
        }

        public void AddEntityOnKernel(IDCLEntity entity, IParcelScene scene)
        {
            if (scene == null)
                return;

            List<ComponentPayload> list = new List<ComponentPayload>();

            foreach (KeyValuePair<CLASS_ID_COMPONENT, IEntityComponent> keyValuePair in scene.componentsManagerLegacy.GetComponentsDictionary(entity))
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

            using (var iterator = scene.componentsManagerLegacy.GetSharedComponents(entity))
            {
                while (iterator.MoveNext())
                {
                    ComponentPayload componentPayLoad = new ComponentPayload();

                    componentPayLoad.componentId = iterator.Current.GetClassId();

                    if (iterator.Current.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
                    {
                        NFTComponent nftComponent = new NFTComponent();
                        NFTShape.Model model = (NFTShape.Model) iterator.Current.GetModel();

                        nftComponent.color = new ColorRepresentation(model.color);
                        nftComponent.assetId = model.assetId;
                        nftComponent.src = model.src;
                        nftComponent.style = model.style;

                        componentPayLoad.data = nftComponent;
                    }
                    else
                    {
                        componentPayLoad.data = iterator.Current.GetModel();
                    }

                    list.Add(componentPayLoad);
                }
            }

            SendNewEntityToKernel(scene.sceneData.id, entity.entityId, list.ToArray());
        }

        public void EntityTransformReport(IDCLEntity entity, IParcelScene scene)
        {
           componentSendHandler.EntityTransformReport(entity,scene);
        }

        public void RemoveEntityOnKernel(long entityId, IParcelScene scene)
        {
            componentSendHandler.RemoveEntityOnKernel(entityId,scene);
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
        
        private void SendNewEntityToKernel(string sceneId, long entityId, ComponentPayload[] componentsPayload)
        {
            componentSendHandler.SendNewEntityToKernel(sceneId,entityId,componentsPayload);
            OnKernelUpdated?.Invoke();
        }

        #endregion

    }
}