using Builder;
using Builder.Gizmos;
using Builder.MeshLoadIndicator;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Helpers.NFT;
using DCL.Interface;
using DCL.Models;
using DCL.Tutorial;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;
using Environment = DCL.Environment;

public class BuilderInWorldController : MonoBehaviour
{
    public enum EditModeState
    {
        Inactive = 0,
        FirstPerson = 1,
        Editor = 2
    }

    [Header("Activation of Feature")]
    public bool activeFeature = false;
    public bool bypassLandOwnershipCheck = false;

    [Header("Design variables")]
    public float scaleSpeed = 0.25f;

    public float rotationSpeed = 0.5f;
    public float msBetweenInputInteraction = 200;

    public float distanceLimitToSelectObjects = 50;

    [Header("Snap variables")]
    public float snapFactor = 1f;

    public float snapRotationDegresFactor = 15f;
    public float snapScaleFactor = 0.5f;

    public float snapDistanceToActivateMovement = 10f;

    [Header("Scene References")]
    public GameObject cameraParentGO;

    public GameObject cursorGO;
    public InputController inputController;
    public PlayerAvatarController avatarRenderer;

    [Header("Prefab References")]
    public OutlinerController outlinerController;

    public BuilderInWorldInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldBridge builderInWorldBridge;
    public Material outlinerMaterial;
    public DCLBuilderMeshLoadIndicatorController dclBuilderMeshLoadIndicatorController;
    public DCLBuilderMeshLoadIndicator meshLoadIndicator;
    public GameObject floorPrefab;

    [Header("Build Modes")]
    public BuilderInWorldFirstPersonMode firstPersonMode;

    public BuilderInWorldGodMode editorMode;

    [Header("Build References")]
    public int builderRendererIndex = 1;

    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger editModeChangeInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleCreateLastSceneObjectInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleRedoActionInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleUndoActionInputAction;

    [SerializeField]
    internal InputAction_Trigger toggleSnapModeInputAction;

    [SerializeField]
    internal InputAction_Hold multiSelectionInputAction;

    //Note(Adrian): This is for tutorial purposes
    public Action OnSceneObjectPlaced;

    BuilderInWorldMode currentActiveMode;

    [HideInInspector]
    public ParcelScene sceneToEdit;

    [HideInInspector]
    public bool isEditModeActivated = false,
        isSnapActive = true,
        isMultiSelectionActive = false,
        isAdvancedModeActive = true,
        isOutlineCheckActive = true;


    GameObject editionGO;
    GameObject undoGO, snapGO, freeMovementGO;

    float nexTimeToReceiveInput;

    int outlinerOptimizationCounter = 0, checkerInsideSceneOptimizationCounter = 0;
    int checkerSceneLimitsOptimizationCounter = 0;

    string sceneToEditId;

    CatalogItem lastCatalogItemCreated;
    CatalogItem lastFloorCalalogItemUsed;

    const float RAYCAST_MAX_DISTANCE = 10000f;

    InputAction_Hold.Started multiSelectionStartDelegate;
    InputAction_Hold.Finished multiSelectionFinishedDelegate;

    InputAction_Trigger.Triggered createLastSceneObjectDelegate;
    InputAction_Trigger.Triggered redoDelegate;
    InputAction_Trigger.Triggered undoDelegate;
    InputAction_Trigger.Triggered snapModeDelegate;

    EditModeState currentEditModeState = EditModeState.Inactive;

    bool catalogAdded = false;
    bool sceneReady = false;
    bool isTestMode = false;

    Dictionary<string, GameObject> floorPlaceHolderDict = new Dictionary<string, GameObject>();

    private void Awake()
    {
        BIWCatalogManager.Init();
    }

    void Start()
    {
        KernelConfig.i.EnsureConfigInitialized().Then(config => activeFeature = config.features.enableBuilderInWorld);
        KernelConfig.i.OnChange += OnKernelConfigChanged;

        if (snapGO == null)
            snapGO = new GameObject("SnapGameObject");

        snapGO.transform.SetParent(transform);

        if (freeMovementGO == null)
            freeMovementGO = new GameObject("FreeMovementGO");

        freeMovementGO.transform.SetParent(cameraParentGO.transform);

        if (editionGO == null)
            editionGO = new GameObject("EditionGO");

        editionGO.transform.SetParent(cameraParentGO.transform);

        if (undoGO == null)
        {
            undoGO = new GameObject("UndoGameObject");
            undoGO.transform.SetParent(transform);
        }

        HUDConfiguration hudConfig = new HUDConfiguration();
        hudConfig.active = true;
        hudConfig.visible = false;
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_MAIN);
        HUDController.i.CreateHudElement<BuilderInWorldInititalHUDController>(hudConfig, HUDController.HUDElementID.BUILDER_IN_WORLD_INITIAL);

        editModeChangeInputAction.OnTriggered += OnEditModeChangeAction;

        createLastSceneObjectDelegate = (action) => CreateLastSceneObject();
        redoDelegate = (action) => RedoAction();
        undoDelegate = (action) => UndoAction();
        snapModeDelegate = (action) => ChangeSnapMode();

        toggleCreateLastSceneObjectInputAction.OnTriggered += createLastSceneObjectDelegate;
        toggleRedoActionInputAction.OnTriggered += redoDelegate;
        toggleUndoActionInputAction.OnTriggered += undoDelegate;
        toggleSnapModeInputAction.OnTriggered += snapModeDelegate;

        multiSelectionStartDelegate = (action) => StartMultiSelection();
        multiSelectionFinishedDelegate = (action) => EndMultiSelection();

        multiSelectionInputAction.OnStarted += multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished += multiSelectionFinishedDelegate;

        HUDController.i.builderInWorldInititalHud.OnEnterEditMode += TryStartEnterEditMode;
        HUDController.i.builderInWorldMainHud.OnStopInput += StopInput;
        HUDController.i.builderInWorldMainHud.OnResumeInput += ResumeInput;


        HUDController.i.builderInWorldMainHud.OnChangeModeAction += ChangeAdvanceMode;
        HUDController.i.builderInWorldMainHud.OnResetAction += ResetScaleAndRotation;

        HUDController.i.builderInWorldMainHud.OnCatalogItemSelected += OnCatalogItemSelected;
        HUDController.i.builderInWorldMainHud.OnTutorialAction += StartTutorial;
        HUDController.i.builderInWorldMainHud.OnPublishAction += PublishScene;
        HUDController.i.builderInWorldMainHud.OnLogoutAction += ExitEditMode;

        BuilderInWorldNFTController.i.OnNFTUsageChange += OnNFTUsageChange;

        builderInputWrapper.OnMouseClick += MouseClick;

        builderInWorldEntityHandler.Init();
        InitEditModes();


        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);


        if (!isTestMode)
        {
            ExternalCallsController.i.GetContentAsString(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived);
            BuilderInWorldNFTController.i.Initialize();
        }

        meshLoadIndicator.SetCamera(Camera.main);
    }

    private void OnDestroy()
    {
        KernelConfig.i.OnChange -= OnKernelConfigChanged;
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;

        toggleCreateLastSceneObjectInputAction.OnTriggered -= createLastSceneObjectDelegate;
        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;
        toggleSnapModeInputAction.OnTriggered -= snapModeDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        if(HUDController.i.builderInWorldInititalHud != null)
            HUDController.i.builderInWorldInititalHud.OnEnterEditMode -= TryStartEnterEditMode;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnStopInput -= StopInput;
            HUDController.i.builderInWorldMainHud.OnResumeInput -= ResumeInput;

            HUDController.i.builderInWorldMainHud.OnChangeModeAction -= ChangeAdvanceMode;
            HUDController.i.builderInWorldMainHud.OnResetAction -= ResetScaleAndRotation;

            HUDController.i.builderInWorldMainHud.OnCatalogItemSelected -= OnCatalogItemSelected;
            HUDController.i.builderInWorldMainHud.OnTutorialAction -= StartTutorial;
            HUDController.i.builderInWorldMainHud.OnPublishAction -= PublishScene;
            HUDController.i.builderInWorldMainHud.OnLogoutAction -= ExitEditMode;
        }


        builderInputWrapper.OnMouseClick -= MouseClick;

        firstPersonMode.OnInputDone -= InputDone;
        editorMode.OnInputDone -= InputDone;

        firstPersonMode.OnActionGenerated -= actionController.AddAction;
        editorMode.OnActionGenerated -= actionController.AddAction;
        BuilderInWorldNFTController.i.OnNFTUsageChange -= OnNFTUsageChange;

        CleanItems();

    }

    private void Update()
    {
        if (!isEditModeActivated) return;

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
                CheckEditModeInput();
            if (currentActiveMode != null)
                currentActiveMode.CheckInput();
        }

        if (checkerInsideSceneOptimizationCounter >= 60)
        {

            if (!sceneToEdit.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
                ExitEditMode();
            checkerInsideSceneOptimizationCounter = 0;
        }
        else
        {
            checkerInsideSceneOptimizationCounter++;
        }

        if(checkerSceneLimitsOptimizationCounter >= 10)
        {
            checkerSceneLimitsOptimizationCounter = 0;
            CheckPublishConditions();
        }
        else
        {
            checkerSceneLimitsOptimizationCounter++;
        }
    }

    public void SetTestMode()
    {
        isTestMode = true;
    }

    void CheckPublishConditions()
    {
        bool canPublishScene = true;

        if(!sceneToEdit.metricsController.IsInsideTheLimits())
        {
            HUDController.i.builderInWorldMainHud.SetPublishBtnAvailability(false);
            return;
        }

        if(!builderInWorldEntityHandler.AreAllEntitiesInsideBoundaries())
        {
            HUDController.i.builderInWorldMainHud.SetPublishBtnAvailability(false);
            return;
        }


        HUDController.i.builderInWorldMainHud.SetPublishBtnAvailability(canPublishScene);
    }

    void OnNFTUsageChange()
    {
        HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
    }

    void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        EnableFeature(current.features.enableBuilderInWorld);
    }

    void EnableFeature(bool enable)
    {
        activeFeature = enable;
    }

    void CatalogReceived(string catalogJson)
    {
        AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        CatalogLoaded();
    }

    public void CatalogLoaded()
    {
        catalogAdded = true;
        if(HUDController.i.builderInWorldMainHud != null)
           HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
        StartEnterEditMode();
    }

    void StopInput()
    {
        builderInputWrapper.StopInput();
    }

    void ResumeInput()
    {
        builderInputWrapper.ResumeInput();
    }

    void InitEditModes()
    {
        firstPersonMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());
        editorMode.Init(editionGO, undoGO, snapGO, freeMovementGO, builderInWorldEntityHandler.GetSelectedEntityList());

        firstPersonMode.OnInputDone += InputDone;
        editorMode.OnInputDone += InputDone;

        firstPersonMode.OnActionGenerated += actionController.AddAction;
        editorMode.OnActionGenerated += actionController.AddAction;
    }

    void StartTutorial()
    {
        TutorialController.i.SetBuilderInWorldTutorialEnabled();
    }

    void MouseClick(int buttonID, Vector3 position)
    {
        if (!isEditModeActivated) return;

        if (Time.timeSinceLevelLoad >= nexTimeToReceiveInput)
        {
            if (Utils.isCursorLocked || isAdvancedModeActive)
            {
                if (buttonID == 0)
                {
                    MouseClickDetected();
                    InputDone();
                    return;
                }

                CheckOutline();
            }
        }
    }

    bool IsInsideTheLimits(CatalogItem sceneObject)
    {
        if (HUDController.i.builderInWorldMainHud == null)
            return false;

        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            HUDController.i.builderInWorldMainHud.ShowSceneLimitsPassed();
            return false;
        }

        return true;
    }

    void OnCatalogItemSelected(CatalogItem sceneObject)
    {
        if (IsCatalogItemFloor(sceneObject))
        {
            builderInWorldEntityHandler.DeleteFloorEntities();
            CatalogItem lastFloor = lastFloorCalalogItemUsed;

            CreateFloor(sceneObject);

            BuildInWorldCompleteAction buildAction = new BuildInWorldCompleteAction();

            buildAction.CreateChangeFloorAction(lastFloor, sceneObject);
            actionController.AddAction(buildAction);
        }
        else
        {
            CreateSceneObject(sceneObject);
        }
    }

    DCLBuilderInWorldEntity CreateSceneObject(CatalogItem catalogItem, bool autoSelect = true, bool isFloor = false)
    {
        if (catalogItem.IsNFT() && BuilderInWorldNFTController.i.IsNFTInUse(catalogItem.id)) return null;

        IsInsideTheLimits(catalogItem);

        //Note (Adrian): This is a workaround until the mapping is handle by kernel

        LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
        data.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;

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

        Environment.i.world.sceneController.UpdateParcelScenesExecute(data);


        DCLName name = (DCLName)sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
        DCLLockedOnEdit entityLocked = (DCLLockedOnEdit)sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));

        DCLBuilderInWorldEntity entity = builderInWorldEntityHandler.CreateEmptyEntity(sceneToEdit, currentActiveMode.GetCreatedEntityPoint(), editionGO.transform.position);
        entity.isFloor = isFloor;

        if (entity.isFloor)
            entityLocked.SetIsLocked(true);
        else
            entityLocked.SetIsLocked(false);

        if (catalogItem.IsNFT())
        {
            NFTShape nftShape = (NFTShape)sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.NFT_SHAPE));
            nftShape.model = new NFTShape.Model();
            nftShape.model.color = new Color(0.6404918f, 0.611472f, 0.8584906f);
            nftShape.model.src = catalogItem.model;
            nftShape.model.assetId = catalogItem.id;

            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, nftShape.id);
        }
        else
        {
            GLTFShape mesh = (GLTFShape)sceneToEdit.SharedComponentCreate(catalogItem.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
            mesh.model = new LoadableShape.Model();
            mesh.model.src = catalogItem.model;
            mesh.model.assetId = catalogItem.id;
            sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, mesh.id);
        }

        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, name.id);
        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, entityLocked.id);

        builderInWorldEntityHandler.SetEntityName(entity, catalogItem.name);
  
        if(catalogItem.IsSmartItem())
        {
            SmartItemComponent.Model model = new SmartItemComponent.Model();
            model.values = new Dictionary<object, object>();

            string jsonModel = JsonUtility.ToJson(model);

            //Note (Adrian): This shouldn't work this way, we should have a function to create the component from Model directly
            sceneToEdit.EntityComponentCreateOrUpdateFromUnity(entity.rootEntity.entityId, CLASS_ID_COMPONENT.SMART_ITEM, jsonModel);

            //Note (Adrian): We can't wait to set the component 1 frame, so we set it
            if(entity.rootEntity.TryGetBaseComponent(CLASS_ID_COMPONENT.SMART_ITEM, out BaseComponent baseComponent))
                 ((SmartItemComponent)baseComponent).SetModel(model);
        }

        if (catalogItem.IsVoxel())
            entity.isVoxel = true;

        if (autoSelect)
        {
            builderInWorldEntityHandler.DeselectEntities();
            builderInWorldEntityHandler.Select(entity.rootEntity);
        }

        entity.gameObject.transform.eulerAngles = Vector3.zero;

        currentActiveMode.CreatedEntity(entity);
        if (!isAdvancedModeActive)
            Utils.LockCursor();
        lastCatalogItemCreated = catalogItem;

        builderInWorldEntityHandler.NotifyEntityIsCreated(entity.rootEntity);
        InputDone();
        OnSceneObjectPlaced?.Invoke();

        return entity;
    }

    void CreateLastSceneObject()
    {
        if (lastCatalogItemCreated != null)
        {
            if (builderInWorldEntityHandler.IsAnyEntitySelected())
                builderInWorldEntityHandler.DeselectEntities();
            OnCatalogItemSelected(lastCatalogItemCreated);
            InputDone();
        }
    }

    void ChangeSnapMode()
    {
        SetSnapActive(!isSnapActive);
        InputDone();
    }

    void RedoAction()
    {
        actionController.TryToRedoAction();
        InputDone();
    }

    void UndoAction()
    {
        InputDone();

        if (currentActiveMode.ShouldCancelUndoAction())
            return;

        actionController.TryToUndoAction();
    }

    void CheckEditModeInput()
    {
        if (!builderInWorldEntityHandler.IsAnyEntitySelected() || isMultiSelectionActive)
        {
            CheckOutline();
        }

        if (builderInWorldEntityHandler.IsAnyEntitySelected())
        {
            currentActiveMode.CheckInputSelectedEntities();
        }
    }

    public void ChangeAdvanceMode()
    {
        SetAdvanceMode(!isAdvancedModeActive);
        InputDone();
    }

    public void SetBuildMode(EditModeState state)
    {
        if (currentActiveMode != null)
            currentActiveMode.Deactivate();
        isAdvancedModeActive = false;

        currentActiveMode = null;
        switch (state)
        {
            case EditModeState.Inactive:
                break;
            case EditModeState.FirstPerson:
                currentActiveMode = firstPersonMode;
                if (HUDController.i.builderInWorldMainHud != null)
                {
                    HUDController.i.builderInWorldMainHud.ActivateFirstPersonModeUI();
                    HUDController.i.builderInWorldMainHud.SetVisibilityOfCatalog(false);
                }
                cursorGO.SetActive(true);
                break;
            case EditModeState.Editor:
                cursorGO.SetActive(false);
                currentActiveMode = editorMode;
                isAdvancedModeActive = true;
                if(HUDController.i.builderInWorldMainHud != null)
                   HUDController.i.builderInWorldMainHud.ActivateGodModeUI();

                avatarRenderer.SetAvatarVisibility(false);
                break;
        }

        currentEditModeState = state;

        if (currentActiveMode != null)
        {
            currentActiveMode.Activate(sceneToEdit);
            currentActiveMode.SetSnapActive(isSnapActive);
            builderInWorldEntityHandler.SetActiveMode(currentActiveMode);
        }
    }

    public void SetAdvanceMode(bool advanceModeActive)
    {
        if (!advanceModeActive)
        {
            SetBuildMode(EditModeState.FirstPerson);
        }
        else
        {
            SetBuildMode(EditModeState.Editor);
        }
    }

    void StartMultiSelection()
    {
        isMultiSelectionActive = true;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        currentActiveMode.StartMultiSelection();
    }

    void EndMultiSelection()
    {
        isMultiSelectionActive = false;
        builderInWorldEntityHandler.SetMultiSelectionActive(isMultiSelectionActive);
        currentActiveMode.EndMultiSelection();
        outlinerController.CancelUnselectedOutlines();
    }

    private void CheckOutline()
    {
        if (outlinerOptimizationCounter >= 10 && isOutlineCheckActive)
        {
            if (!BuilderInWorldUtils.IsPointerOverUIElement())
            {
                DCLBuilderInWorldEntity entity = GetEntityOnPointer();
                if (!isMultiSelectionActive)
                    outlinerController.CancelAllOutlines();
                else
                    outlinerController.CancelUnselectedOutlines();

                if (entity != null && !entity.IsSelected)
                    outlinerController.OutlineEntity(entity);
            }

            outlinerOptimizationCounter = 0;
        }
        else outlinerOptimizationCounter++;
    }

    public void UndoEditionGOLastStep()
    {
        BuilderInWorldUtils.CopyGameObjectStatus(undoGO, editionGO, false, false);
    }

    public void ResetScaleAndRotation()
    {
        currentActiveMode.ResetScaleAndRotation();
    }

    public void SetOutlineCheckActive(bool isActive)
    {
        isOutlineCheckActive = isActive;
    }

    public void SetSnapActive(bool isActive)
    {
        isSnapActive = isActive;
        currentActiveMode.SetSnapActive(isActive);
    }

    public void CleanItems()
    {
        Destroy(undoGO);
        Destroy(snapGO);
        Destroy(editionGO);
        Destroy(freeMovementGO);

        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.Dispose();

        if (HUDController.i.builderInWorldInititalHud != null)
            HUDController.i.builderInWorldInititalHud.Dispose();

        if (Camera.main != null)
        {
            DCLBuilderOutline outliner = Camera.main.GetComponent<DCLBuilderOutline>();
            Destroy(outliner);
        }
    }

    void InputDone()
    {
        nexTimeToReceiveInput = Time.timeSinceLevelLoad + msBetweenInputInteraction / 1000;
    }

    [ContextMenu("Activate feature")]
    public void ActivateFeature()
    {
        activeFeature = true;
        HUDController.i.taskbarHud.SetBuilderInWorldStatus(activeFeature);
    }

    private void OnEditModeChangeAction(DCLAction_Trigger action)
    {
        if (activeFeature)
        {
            if (isEditModeActivated)
            {
                ExitEditMode();
            }
            else
            {
                TryStartEnterEditMode();
            }
        }
    }

    void MouseClickDetected()
    {
        DCLBuilderInWorldEntity entityToSelect = GetEntityOnPointer();
        if (entityToSelect != null)
        {
            builderInWorldEntityHandler.EntityClicked(entityToSelect);
        }
        else if (!isMultiSelectionActive)
        {
            builderInWorldEntityHandler.DeselectEntities();
        }
    }

    public DCLBuilderInWorldEntity GetEntityOnPointer()
    {
        RaycastHit hit;
        UnityEngine.Ray ray;
        float distanceToSelect = distanceLimitToSelectObjects;
        if (!isAdvancedModeActive)
        {
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            distanceToSelect = 9999;
        }

        if (Physics.Raycast(ray, out hit, distanceToSelect, layerToRaycast))
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                return builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);
            }
        }

        return null;
    }

    public VoxelEntityHit GetCloserUnselectedVoxelEntityOnPointer()
    {
        RaycastHit[] hits;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ;

        float currentDistance = 9999;
        VoxelEntityHit voxelEntityHit = null;

        hits = Physics.RaycastAll(ray, RAYCAST_MAX_DISTANCE, layerToRaycast);

        foreach (RaycastHit hit in hits)
        {
            string entityID = hit.collider.gameObject.name;

            if (sceneToEdit.entities.ContainsKey(entityID))
            {
                DCLBuilderInWorldEntity entityToCheck = builderInWorldEntityHandler.GetConvertedEntity(sceneToEdit.entities[entityID]);

                if (entityToCheck == null) continue;

                Camera camera = Camera.main;

                if (!entityToCheck.IsSelected && entityToCheck.tag == BuilderInWorldSettings.VOXEL_TAG)
                {
                    if (Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                    {
                        voxelEntityHit = new VoxelEntityHit(entityToCheck, hit);
                        currentDistance = Vector3.Distance(camera.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                    }
                }
            }
        }

        return voxelEntityHit;
    }

    public void NewSceneReady(string id)
    {
        if (sceneToEditId != id)
            return;

        Environment.i.world.sceneController.OnReadyScene -= NewSceneReady;
        sceneToEditId = null;
        sceneReady = true;
        CheckEnterEditMode();
    }

    bool UserHasPermissionOnParcelScene(ParcelScene scene)
    {
        if (bypassLandOwnershipCheck)
            return true;

        UserProfile userProfile = UserProfile.GetOwnUserProfile();
        foreach (UserProfileModel.ParcelsWithAccess parcelWithAccess in userProfile.parcelsWithAccess)
        {
            foreach (Vector2Int parcel in scene.sceneData.parcels)
            {
                if (parcel.x == parcelWithAccess.x && parcel.y == parcelWithAccess.y)
                    return true;
            }
        }
        return false;
    }

    void CheckEnterEditMode()
    {
        if (catalogAdded && sceneReady) EnterEditMode();
    }

    public void TryStartEnterEditMode()
    {
        TryStartEnterEditMode(true);
    }

    public void TryStartEnterEditMode(bool activateCamera)
    {
        if (sceneToEditId != null)
            return;

        FindSceneToEdit();

        if(!UserHasPermissionOnParcelScene(sceneToEdit))
        {
            Notification.Model notificationModel = new Notification.Model();
            notificationModel.message = "You don't have permissions to operate this land";
            notificationModel.type = NotificationFactory.Type.GENERIC;
            HUDController.i.notificationHud.ShowNotification(notificationModel);
            return;
        }

        if (activateCamera)
            editorMode.ActivateCamera(sceneToEdit);

        if (catalogAdded)
            StartEnterEditMode();
    }

    void StartEnterEditMode()
    {
        if (sceneToEdit == null)
            return;

        sceneToEditId = sceneToEdit.sceneData.id;
        inputController.isInputActive = false;

        Environment.i.world.sceneController.OnReadyScene += NewSceneReady;

        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
    }

    public void EnterEditMode()
    {
        BuilderInWorldNFTController.i.ClearNFTs();

        ParcelSettings.VISUAL_LOADING_ENABLED = false;


        inputController.isInputActive = true;
        inputController.isBuildModeActivate = true;

        FindSceneToEdit();

        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.SetVisibility(true);
            HUDController.i.builderInWorldMainHud.SetParcelScene(sceneToEdit);
            HUDController.i.builderInWorldMainHud.RefreshCatalogContent();
            HUDController.i.builderInWorldMainHud.RefreshCatalogAssetPack();
        }

        if (currentActiveMode == null)
            SetBuildMode(EditModeState.Editor);

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);

        Environment.i.world.sceneController.ActivateBuilderInWorldEditScene();

 

        ActivateBuilderInWorldCamera();
        if (IsNewScene())
            SetupNewScene();

        isEditModeActivated = true;
    }

    public void ExitEditMode()
    {
        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        inputController.isBuildModeActivate = false;
        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);
        builderInWorldEntityHandler.ExitFromEditMode();

        sceneToEdit.SetEditMode(false);
        SetBuildMode(EditModeState.Inactive);


        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(false);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);

        avatarRenderer.SetAvatarVisibility(true);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.ClearEntityList();
            HUDController.i.builderInWorldMainHud.SetVisibility(false);
        }

        Environment.i.world.sceneController.DeactivateBuilderInWorldEditScene();

        DeactivateBuilderInWorldCamera();
        isEditModeActivated = false;
    }

    public bool IsCatalogItemFloor(CatalogItem floorSceneObject)
    {
        return string.Equals(floorSceneObject.category, BuilderInWorldSettings.FLOOR_CATEGORY);
    }

    public bool IsNewScene()
    {
        return sceneToEdit.entities.Count <= 0;
    }

    public void SetupNewScene()
    {
        CatalogItem floorSceneObject = BuilderInWorldUtils.CreateFloorSceneObject();
        CreateFloor(floorSceneObject);
    }

    public void CreateFloor(CatalogItem floorSceneObject)
    {
        Vector3 initialPosition = new Vector3(ParcelSettings.PARCEL_SIZE / 2, 0, ParcelSettings.PARCEL_SIZE / 2);
        Vector2Int[] parcelsPoints = sceneToEdit.sceneData.parcels;

        foreach (Vector2Int parcel in parcelsPoints)
        {
            DCLBuilderInWorldEntity decentralandEntity = CreateSceneObject(floorSceneObject,false,true);
            decentralandEntity.rootEntity.OnShapeUpdated += OnFloorLoaded;
            decentralandEntity.transform.position = Environment.i.world.state.ConvertPointInSceneToUnityPosition(initialPosition, parcel);
            dclBuilderMeshLoadIndicatorController.ShowIndicator(decentralandEntity.rootEntity.gameObject.transform.position, decentralandEntity.rootEntity.entityId);

            GameObject floorPlaceHolder =  Instantiate(floorPrefab, decentralandEntity.rootEntity.gameObject.transform.position, Quaternion.identity);
            floorPlaceHolderDict.Add(decentralandEntity.rootEntity.entityId, floorPlaceHolder);
            builderInWorldBridge.EntityTransformReport(decentralandEntity.rootEntity, sceneToEdit);
        }

        builderInWorldEntityHandler.DeselectEntities();

        lastFloorCalalogItemUsed = floorSceneObject;
    }

    void OnFloorLoaded(DecentralandEntity entity)
    {
        entity.OnShapeUpdated -= OnFloorLoaded;
        dclBuilderMeshLoadIndicatorController.HideIndicator(entity.entityId);

        GameObject floorPlaceHolder = floorPlaceHolderDict[entity.entityId];
        floorPlaceHolderDict.Remove(entity.entityId);
        Destroy(floorPlaceHolder);
    }


    public void ActivateBuilderInWorldCamera()
    {
        Camera camera = Camera.main;
        DCLBuilderOutline outliner = camera.GetComponent<DCLBuilderOutline>();

        if (outliner == null)
        {
            outliner = camera.gameObject.AddComponent(typeof(DCLBuilderOutline)) as DCLBuilderOutline;
            outliner.SetOutlineMaterial(outlinerMaterial);
        }
        else
        {
            outliner.enabled = true;
        }

        outliner.Activate();

        UniversalAdditionalCameraData additionalCameraData = camera.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(builderRendererIndex);
    }

    public void DeactivateBuilderInWorldCamera()
    {
        Camera camera = Camera.main;
        DCLBuilderOutline outliner = camera.GetComponent<DCLBuilderOutline>();
        if (outliner != null)
        {
            outliner.enabled = false;
            outliner.Deactivate();
        }

        outliner.enabled = false;
        outliner.Deactivate();

        UniversalAdditionalCameraData additionalCameraData = camera.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(0);
    }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position)
    {
        ExitEditMode();
    }

    void FindSceneToEdit()
    {
        foreach (ParcelScene scene in Environment.i.world.state.scenesSortedByDistance)
        {
            if (scene.IsInsideSceneBoundaries(DCLCharacterController.i.characterPosition))
            {
                if (sceneToEdit != null && sceneToEdit != scene)
                    actionController.ResetActionList();
                sceneToEdit = scene;
                break;
            }
        }
    }

    void PublishScene()
    {
        builderInWorldBridge.PublishScene(sceneToEdit);
        HUDController.i.builderInWorldMainHud.PublishStart();
    }
}