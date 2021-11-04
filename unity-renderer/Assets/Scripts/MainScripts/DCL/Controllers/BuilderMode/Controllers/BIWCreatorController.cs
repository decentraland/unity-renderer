using DCL.Components;
using DCL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Builder;
using DCL.Controllers;
using UnityEngine;

namespace DCL
{
    public interface IBIWCreatorController : IBIWController
    {
        event Action OnCatalogItemPlaced;
        event Action OnInputDone;
        void CreateCatalogItem(CatalogItem catalogItem, bool autoSelect = true, bool isFloor = false);
        BIWEntity CreateCatalogItem(CatalogItem catalogItem, Vector3 startPosition, bool autoSelect = true, bool isFloor = false, Action<IDCLEntity> onFloorLoadedAction = null);
        void CreateErrorOnEntity(BIWEntity entity);
        void RemoveLoadingObjectInmediate(string entityId);
        bool IsAnyErrorOnEntities();
        void CreateLoadingObject(BIWEntity entity);
        void CleanUp();
    }

    public class BIWCreatorController : BIWController, IBIWCreatorController
    {
        private const float SECONDS_TO_SEND_ANALYTICS = 5f;
        public event Action OnInputDone;
        public event Action OnCatalogItemPlaced;

        private IBIWModeController modeController;

        private IBIWFloorHandler floorHandler;
        private IBIWEntityHandler entityHandler;

        private GameObject loadingObjectPrefab;
        private GameObject errorPrefab;

        private CatalogItem lastCatalogItemCreated;

        private readonly Dictionary<string, BIWLoadingPlaceHolder> loadingGameObjects = new Dictionary<string, BIWLoadingPlaceHolder>();
        private readonly Dictionary<BIWEntity, GameObject> errorGameObjects = new Dictionary<BIWEntity, GameObject>();

        private readonly List<KeyValuePair<CatalogItem, string>> itemsToSendAnalytics = new List<KeyValuePair<CatalogItem, string>>();

        private float lastAnalyticsSentTimestamp = 0;

        public override void Initialize(Context context)
        {
            base.Initialize(context);

            modeController = context.editorContext.modeController;
            floorHandler = context.editorContext.floorHandler;
            entityHandler = context.editorContext.entityHandler;
            loadingObjectPrefab = context.projectReferencesAsset.loadingPrefab;
            errorPrefab = context.projectReferencesAsset.errorPrefab;

            if ( context.editorContext.editorHUD != null)
            {
                context.editorContext.editorHUD.OnCatalogItemSelected += OnCatalogItemSelected;
                context.editorContext.editorHUD.OnCatalogItemDropped += OnCatalogItemDropped;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if ( context.editorContext.editorHUD != null)
            {
                context.editorContext.editorHUD.OnCatalogItemSelected -= OnCatalogItemSelected;
                context.editorContext.editorHUD.OnCatalogItemDropped -= OnCatalogItemDropped;
            }

            CleanUp();
        }

        public override void Update()
        {
            base.Update();
            if (Time.realtimeSinceStartup >= lastAnalyticsSentTimestamp)
            {
                SendAnalytics();
                lastAnalyticsSentTimestamp = Time.realtimeSinceStartup + SECONDS_TO_SEND_ANALYTICS;
            }
        }

        private void SendAnalytics()
        {
            if (itemsToSendAnalytics.Count == 0)
                return;

            BIWAnalytics.NewObjectPlacedChunk(itemsToSendAnalytics);
            itemsToSendAnalytics.Clear();
        }

        public void CleanUp()
        {
            foreach (BIWLoadingPlaceHolder placeHolder in loadingGameObjects.Values)
            {
                placeHolder.Dispose();
            }

            foreach (BIWEntity entity in errorGameObjects.Keys.ToArray())
            {
                DeleteErrorOnEntity(entity);
            }

            loadingGameObjects.Clear();
            errorGameObjects.Clear();
        }

        public bool IsAnyErrorOnEntities() { return errorGameObjects.Count > 0; }

        private bool IsInsideTheLimits(CatalogItem sceneObject)
        {
            if ( context.editorContext.editorHUD == null)
                return false;

            SceneMetricsModel limits = sceneToEdit.metricsCounter.GetLimits();
            SceneMetricsModel usage = sceneToEdit.metricsCounter.GetModel();

            if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            if (limits.entities < usage.entities + sceneObject.metrics.entities)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            if (limits.materials < usage.materials + sceneObject.metrics.materials)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            if (limits.textures < usage.textures + sceneObject.metrics.textures)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
            {
                context.editorContext.editorHUD.ShowSceneLimitsPassed();
                return false;
            }

            return true;
        }

        public void CreateErrorOnEntity(BIWEntity entity)
        {
            if (errorGameObjects.ContainsKey(entity))
                return;

            GameObject instantiatedError = UnityEngine.Object.Instantiate(errorPrefab, Vector3.zero, errorPrefab.transform.rotation);
            instantiatedError.transform.SetParent(entity.rootEntity.gameObject.transform, true);
            instantiatedError.transform.localPosition = Vector3.zero;

            var missingAsset = instantiatedError.GetComponent<MissingAsset>();
            missingAsset.Configure(entity);

            errorGameObjects.Add(entity, instantiatedError);
            entity.OnDelete += DeleteErrorOnEntity;
        }

        public void DeleteErrorOnEntity(BIWEntity entity)
        {
            if (!errorGameObjects.ContainsKey(entity))
                return;

            entity.OnDelete -= DeleteErrorOnEntity;
            GameObject.Destroy(errorGameObjects[entity]);
            errorGameObjects.Remove(entity);
        }

        public void CreateCatalogItem(CatalogItem catalogItem, bool autoSelect = true, bool isFloor = false) { CreateCatalogItem(catalogItem, modeController.GetModeCreationEntryPoint(), autoSelect, isFloor); }

        public BIWEntity CreateCatalogItem(CatalogItem catalogItem, Vector3 startPosition, bool autoSelect = true, bool isFloor = false, Action<IDCLEntity> onFloorLoadedAction = null)
        {
            if (catalogItem.IsNFT() && BIWNFTController.i.IsNFTInUse(catalogItem.id))
                return null;

            IsInsideTheLimits(catalogItem);

            //Note (Adrian): This is a workaround until the mapping is handle by kernel
            AddSceneMappings(catalogItem);

            Vector3 editionPosition = modeController.GetCurrentEditionPosition();

            BIWEntity entity = entityHandler.CreateEmptyEntity(sceneToEdit, startPosition, editionPosition, false);
            entity.isFloor = isFloor;
            entity.SetRotation(Vector3.zero);

            if (!isFloor)
                CreateLoadingObject(entity);

            entity.rootEntity.OnShapeUpdated += (entity) => onFloorLoadedAction?.Invoke(entity);

            AddShape(catalogItem, entity);
            AddEntityNameComponent(catalogItem, entity);
            AddLockedComponent(entity);

            if (catalogItem.IsSmartItem())
            {
                AddSmartItemComponent(entity);
            }

            if (catalogItem.IsVoxel())
                entity.isVoxel = true;

            if (autoSelect)
            {
                entityHandler.DeselectEntities();
                entityHandler.Select(entity.rootEntity);
            }

            entity.rootEntity.gameObject.transform.eulerAngles = Vector3.zero;

            modeController.CreatedEntity(entity);

            lastCatalogItemCreated = catalogItem;

            entityHandler.EntityListChanged();
            entityHandler.NotifyEntityIsCreated(entity.rootEntity);
            OnInputDone?.Invoke();
            OnCatalogItemPlaced?.Invoke();
            return entity;
        }

        #region LoadingObjects

        public bool ExistsLoadingGameObjectForEntity(string entityId) { return loadingGameObjects.ContainsKey(entityId); }

        public void CreateLoadingObject(BIWEntity entity)
        {
            if (loadingGameObjects.ContainsKey(entity.rootEntity.entityId))
                return;

            BIWLoadingPlaceHolder loadingPlaceHolder = UnityEngine.Object.Instantiate(loadingObjectPrefab, entity.rootEntity.gameObject.transform).GetComponent<BIWLoadingPlaceHolder>();
            loadingGameObjects.Add(entity.rootEntity.entityId, loadingPlaceHolder);
            entity.OnShapeFinishLoading += OnShapeLoadFinish;
        }

        private void OnShapeLoadFinish(BIWEntity entity)
        {
            entity.OnShapeFinishLoading -= OnShapeLoadFinish;
            RemoveLoadingObject(entity.rootEntity.entityId);
        }

        public void RemoveLoadingObject(string entityId)
        {
            if (!loadingGameObjects.ContainsKey(entityId))
                return;
            BIWLoadingPlaceHolder loadingPlaceHolder = loadingGameObjects[entityId];
            loadingGameObjects.Remove(entityId);
            loadingPlaceHolder.DestroyAfterAnimation();
        }

        public void RemoveLoadingObjectInmediate(string entityId)
        {
            if (!loadingGameObjects.ContainsKey(entityId))
                return;
            BIWLoadingPlaceHolder loadingPlaceHolder = loadingGameObjects[entityId];
            loadingGameObjects.Remove(entityId);
            loadingPlaceHolder.Dispose();
        }

        #endregion

        #region Add Components

        private void AddSmartItemComponent(BIWEntity entity)
        {
            //Note (Adrian): This will disable the smart item component until it is implemented in kernel
            //TODO: After the implementation in kernel of smart items, we should eliminate this return
            return;
            SmartItemComponent.Model model = new SmartItemComponent.Model();
            model.values = new Dictionary<object, object>();

            sceneToEdit.EntityComponentCreateOrUpdateWithModel(entity.rootEntity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, model);

            //Note (Adrian): We can't wait to set the component 1 frame, so we set it
            if (entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out IEntityComponent component))
                ((SmartItemComponent) component).UpdateFromModel(model);
        }

        private void AddEntityNameComponent(CatalogItem catalogItem, BIWEntity entity)
        {
            DCLName name = (DCLName) sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, name.id);
            entityHandler.SetEntityName(entity, catalogItem.name, false);
        }

        private void AddLockedComponent(BIWEntity entity)
        {
            DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
            if (entity.isFloor)
                entityLocked.SetIsLocked(true);
            else
                entityLocked.SetIsLocked(false);

            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, entityLocked.id);
        }

        private void AddShape(CatalogItem catalogItem, BIWEntity entity)
        {
            if (catalogItem.IsNFT())
            {
                NFTShape nftShape = (NFTShape) sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
                nftShape.model = new NFTShape.Model();
                nftShape.model.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
                nftShape.model.src = catalogItem.model;
                nftShape.model.assetId = catalogItem.id;
                sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, nftShape.id);

                nftShape.CallWhenReady(entity.ShapeLoadFinish);
            }
            else
            {
                GLTFShape gltfComponent = (GLTFShape) sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
                gltfComponent.model = new LoadableShape.Model();
                gltfComponent.model.src = catalogItem.model;
                gltfComponent.model.assetId = catalogItem.id;
                sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, gltfComponent.id);

                gltfComponent.CallWhenReady(entity.ShapeLoadFinish);
            }
        }

        #endregion

        private void AddSceneMappings(CatalogItem catalogItem)
        {
            LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
            data.baseUrl = BIWUrlUtils.GetUrlSceneObjectContent();
            if (data.contents == null)
                data.contents = new List<ContentServerUtils.MappingPair>();
            foreach (KeyValuePair<string, string> content in catalogItem.contents)
            {
                ContentServerUtils.MappingPair mappingPair = new ContentServerUtils.MappingPair();
                mappingPair.file = content.Key;
                mappingPair.hash = content.Value;
                bool found = false;
                foreach (ContentServerUtils.MappingPair mappingPairToCheck in data.contents)
                {
                    if (mappingPairToCheck.file == mappingPair.file)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    data.contents.Add(mappingPair);
            }

            DCL.Environment.i.world.sceneController.UpdateParcelScenesExecute(data);
        }

        public void CreateLastCatalogItem()
        {
            if (lastCatalogItemCreated != null)
            {
                if (entityHandler.IsAnyEntitySelected())
                    entityHandler.DeselectEntities();
                OnCatalogItemSelected(lastCatalogItemCreated);
                OnInputDone?.Invoke();
            }
        }

        private void OnCatalogItemSelected(CatalogItem catalogItem)
        {
            if (floorHandler.IsCatalogItemFloor(catalogItem))
            {
                floorHandler.ChangeFloor(catalogItem);
            }
            else
            {
                CreateCatalogItem(catalogItem);
            }

            string catalogSection = "";
            if ( context.editorContext.editorHUD != null)
                catalogSection =    context.editorContext.editorHUD.GetCatalogSectionSelected().ToString();

            itemsToSendAnalytics.Add(new KeyValuePair<CatalogItem, string>(catalogItem, catalogSection));
        }

        private void OnCatalogItemDropped(CatalogItem catalogItem)
        {
            OnCatalogItemSelected(catalogItem);
            entityHandler.DeselectEntities();
        }
    }
}