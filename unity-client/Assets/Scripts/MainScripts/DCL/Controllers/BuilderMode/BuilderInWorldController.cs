using Builder;
using Builder.Gizmos;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
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

    [Header("Prefab References")]

    public OutlinerController outlinerController;
    public BuilderInWorldInputWrapper builderInputWrapper;
    public DCLBuilderGizmoManager gizmoManager;
    public ActionController actionController;
    public BuilderInWorldEntityHandler builderInWorldEntityHandler;
    public BuilderInWorldBridge builderInWorldBridge;
    public Material outlinerMaterial;

    [Header("Build Modes")]

    public BuilderInWorldFirstPersonMode firstPersonMode;
    public BuilderInWorldGodMode editorMode;

    [Header("Build References")]

    public int builderRendererIndex = 1;
    public LayerMask layerToRaycast;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger editModeChangeInputAction;
    [SerializeField] internal InputAction_Trigger toggleCreateLastSceneObjectInputAction;
    [SerializeField] internal InputAction_Trigger toggleRedoActionInputAction;
    [SerializeField] internal InputAction_Trigger toggleUndoActionInputAction;
    [SerializeField] internal InputAction_Trigger toggleSnapModeInputAction;

    [SerializeField] internal InputAction_Hold multiSelectionInputAction;

    //Note(Adrian): This is for tutorial purposes
    public Action OnSceneObjectPlaced;

    BuilderInWorldMode currentActiveMode;

    ParcelScene sceneToEdit;

    bool isEditModeActivated = false,
         isSnapActive = true,
         isMultiSelectionActive = false,
         isAdvancedModeActive = true,
         isOutlineCheckActive = true;


    GameObject editionGO;
    GameObject undoGO, snapGO, freeMovementGO;

    float nexTimeToReceiveInput;

    int outlinerOptimizationCounter = 0, checkerInsideSceneOptimizationCounter = 0;

    string sceneToEditId;

    SceneObject lastSceneObjectCreated;

    const float RAYCAST_MAX_DISTANCE = 10000f;

    InputAction_Hold.Started multiSelectionStartDelegate;
    InputAction_Hold.Finished multiSelectionFinishedDelegate;

    InputAction_Trigger.Triggered createLastSceneObjectDelegate;
    InputAction_Trigger.Triggered redoDelegate;
    InputAction_Trigger.Triggered undoDelegate;
    InputAction_Trigger.Triggered snapModeDelegate;

    EditModeState currentEditModeState = EditModeState.Inactive;

    bool catalogAdded = false;

    void Start()
    {
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
        HUDController.i.CreateHudElement<BuildModeHUDController>(hudConfig, HUDController.HUDElementID.BUILD_MODE);

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

        HUDController.i.buildModeHud.OnStopInput += StopInput;
        HUDController.i.buildModeHud.OnResumeInput += ResumeInput;


        HUDController.i.buildModeHud.OnChangeModeAction += ChangeAdvanceMode;
        HUDController.i.buildModeHud.OnResetAction += ResetScaleAndRotation;

        HUDController.i.buildModeHud.OnSceneObjectSelected += CreateSceneObjectSelected;
        HUDController.i.buildModeHud.OnTutorialAction += StartTutorial;
        HUDController.i.buildModeHud.OnPublishAction += PublishScene;

        builderInputWrapper.OnMouseClick += MouseClick;

        builderInWorldEntityHandler.Init();
        InitEditModes();


        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);


        AssetCatalogBridge.sceneAssetPackCatalog.GetValues();
        ExternalCallsController.i.GetContentAsString(BuilderInWorldSettings.BASE_URL_ASSETS_PACK, CatalogReceived);

    }

    private void OnDestroy()
    {
        editModeChangeInputAction.OnTriggered -= OnEditModeChangeAction;

        toggleCreateLastSceneObjectInputAction.OnTriggered -= createLastSceneObjectDelegate;
        toggleRedoActionInputAction.OnTriggered -= redoDelegate;
        toggleUndoActionInputAction.OnTriggered -= undoDelegate;
        toggleSnapModeInputAction.OnTriggered -= snapModeDelegate;

        multiSelectionInputAction.OnStarted -= multiSelectionStartDelegate;
        multiSelectionInputAction.OnFinished -= multiSelectionFinishedDelegate;

        if (HUDController.i.buildModeHud != null)
        {
            HUDController.i.buildModeHud.OnStopInput -= StopInput;
            HUDController.i.buildModeHud.OnResumeInput -= ResumeInput;

            HUDController.i.buildModeHud.OnChangeModeAction -= ChangeAdvanceMode;
            HUDController.i.buildModeHud.OnResetAction -= ResetScaleAndRotation;

            HUDController.i.buildModeHud.OnSceneObjectSelected -= CreateSceneObjectSelected;
            HUDController.i.buildModeHud.OnTutorialAction -= StartTutorial;
            HUDController.i.buildModeHud.OnPublishAction -= PublishScene;
        }


        builderInputWrapper.OnMouseClick -= MouseClick;

        firstPersonMode.OnInputDone -= InputDone;
        editorMode.OnInputDone -= InputDone;

        firstPersonMode.OnActionGenerated -= actionController.AddAction;
        editorMode.OnActionGenerated -= actionController.AddAction;

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

    }

    void CatalogReceived(string catalogJson)
    {
        AssetCatalogBridge.i.AddFullSceneObjectCatalog(catalogJson);
        catalogAdded = true;
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
        TutorialController.i.SetTutorialEnabled(false.ToString(), TutorialController.TutorialType.BuilderInWorld);
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

    bool IsInsideTheLimits(SceneObject sceneObject)
    {
        SceneMetricsController.Model limits = sceneToEdit.metricsController.GetLimits();
        SceneMetricsController.Model usage = sceneToEdit.metricsController.GetModel();

        if (limits.bodies < usage.bodies + sceneObject.metrics.bodies)
        {

            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.entities < usage.entities + sceneObject.metrics.entities)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.materials < usage.materials + sceneObject.metrics.materials)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.meshes < usage.meshes + sceneObject.metrics.meshes)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.textures < usage.textures + sceneObject.metrics.textures)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        if (limits.triangles < usage.triangles + sceneObject.metrics.triangles)
        {
            HUDController.i.buildModeHud.ShowSceneLimitsPassed();
            return false;
        }
        return true;
    }

    void CreateSceneObjectSelected(SceneObject sceneObject)
    {
        if (!IsInsideTheLimits(sceneObject)) return;

        //Note (Adrian): This is a workaround until the mapping is handle by kernel

        LoadParcelScenesMessage.UnityParcelScene data = sceneToEdit.sceneData;
        data.baseUrl = BuilderInWorldSettings.BASE_URL_CATALOG;

        foreach (KeyValuePair<string, string> content in sceneObject.contents)
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
        SceneController.i.UpdateParcelScenesExecute(data);

        GLTFShape mesh = (GLTFShape)sceneToEdit.SharedComponentCreate(sceneObject.id, Convert.ToInt32(CLASS_ID.GLTF_SHAPE));
        mesh.model = new LoadableShape.Model();
        mesh.model.src = sceneObject.model;

        DCLName name = (DCLName)sceneToEdit.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));



        DCLBuilderInWorldEntity entity = builderInWorldEntityHandler.CreateEmptyEntity(sceneToEdit, currentActiveMode.GetCreatedEntityPoint(), editionGO.transform.position);

        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, mesh.id);
        sceneToEdit.SharedComponentAttach(entity.rootEntity.entityId, name.id);
        name.SetNewName(sceneObject.name);


        if (sceneObject.asset_pack_id == BuilderInWorldSettings.VOXEL_ASSETS_PACK_ID)
            entity.isVoxel = true;
        builderInWorldEntityHandler.DeselectEntities();
        builderInWorldEntityHandler.Select(entity.rootEntity);


        entity.gameObject.transform.eulerAngles = Vector3.zero;

        currentActiveMode.CreatedEntity(entity);
        if (!isAdvancedModeActive)
            Utils.LockCursor();
        lastSceneObjectCreated = sceneObject;

        builderInWorldEntityHandler.NotifyEntityIsCreated(entity.rootEntity);
        InputDone();
        OnSceneObjectPlaced?.Invoke();
    }

    void CreateLastSceneObject()
    {
        if (lastSceneObjectCreated != null)
        {
            if (builderInWorldEntityHandler.IsAnyEntitySelected())
                builderInWorldEntityHandler.DeselectEntities();
            CreateSceneObjectSelected(lastSceneObjectCreated);
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
                HUDController.i.buildModeHud.ActivateFirstPersonModeUI();
                HUDController.i.buildModeHud.SetVisibilityOfCatalog(false);
                cursorGO.SetActive(true);
                break;
            case EditModeState.Editor:
                cursorGO.SetActive(false);
                currentActiveMode = editorMode;
                isAdvancedModeActive = true;
                HUDController.i.buildModeHud.ActivateGodModeUI();
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

    void InputDone()
    {
        nexTimeToReceiveInput = Time.timeSinceLevelLoad + msBetweenInputInteraction / 1000;
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
                StartEnterEditMode();
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
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); ;

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

                if (!entityToCheck.IsSelected && entityToCheck.tag == BuilderInWorldSettings.VOXEL_TAG)
                {
                    if (Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position) < currentDistance)
                    {
                        voxelEntityHit = new VoxelEntityHit(entityToCheck, hit);
                        currentDistance = Vector3.Distance(Camera.main.transform.position, entityToCheck.rootEntity.gameObject.transform.position);
                    }
                }
            }
        }
        return voxelEntityHit;
    }

    void NewSceneReady(string id)
    {
        if (sceneToEditId != id) return;
        SceneController.i.OnReadyScene -= NewSceneReady;
        sceneToEditId = null;
        EnterEditMode();
    }

    public void StartEnterEditMode()
    {
        if (sceneToEditId != null) return;

        FindSceneToEdit();
        sceneToEditId = sceneToEdit.sceneData.id;
        SceneController.i.OnReadyScene += NewSceneReady;

        builderInWorldBridge.StartKernelEditMode(sceneToEdit);
    
    }

    public void EnterEditMode()
    {

        HUDController.i.buildModeHud.SetVisibility(true);

        isEditModeActivated = true;
        ParcelSettings.VISUAL_LOADING_ENABLED = false;

        inputController.isBuildModeActivate = true;

        FindSceneToEdit();


        sceneToEdit.SetEditMode(true);
        cursorGO.SetActive(false);
        HUDController.i.buildModeHud.SetParcelScene(sceneToEdit);

        if (currentActiveMode == null)
            SetBuildMode(EditModeState.Editor);

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(false);

        DCLCharacterController.OnPositionSet += ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(true);
        builderInWorldEntityHandler.EnterEditMode(sceneToEdit);

        SceneController.i.ActivateBuilderInWorldEditScene();

        ActivateBuilderInWorldCamera();
    }


    public void ExitEditMode()
    {

        CommonScriptableObjects.builderInWorldNotNecessaryUIVisibilityStatus.Set(true);

        HUDController.i.buildModeHud.SetVisibility(false);

        inputController.isBuildModeActivate = false;
        snapGO.transform.SetParent(transform);

        ParcelSettings.VISUAL_LOADING_ENABLED = true;

        outlinerController.CancelAllOutlines();

        cursorGO.SetActive(true);
        builderInWorldEntityHandler.DeselectEntities();
        isEditModeActivated = false;
        sceneToEdit.SetEditMode(false);
        SetBuildMode(EditModeState.Inactive);


        DCLCharacterController.OnPositionSet -= ExitAfterCharacterTeleport;
        builderInputWrapper.gameObject.SetActive(false);
        builderInWorldBridge.ExitKernelEditMode(sceneToEdit);

        HUDController.i.buildModeHud.ClearEntityList();

        SceneController.i.DeactivateBuilderInWorldEditScene();

        DeactivateBuilderInWorldCamera();
    }

    public void ActivateBuilderInWorldCamera()
    {
        DCLBuilderOutline outliner = Camera.main.GetComponent<DCLBuilderOutline>();

        if (outliner == null)
        {
            outliner = Camera.main.gameObject.AddComponent(typeof(DCLBuilderOutline)) as DCLBuilderOutline;
            outliner.SetOutlineMaterial(outlinerMaterial);
        }
        else
        {
            outliner.enabled = true;
        }

        outliner.Activate();

        UniversalAdditionalCameraData additionalCameraData = Camera.main.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(builderRendererIndex);
    }

    public void DeactivateBuilderInWorldCamera()
    {
        DCLBuilderOutline outliner = Camera.main.GetComponent<DCLBuilderOutline>();
        outliner.enabled = false;
        outliner.Deactivate();

        UniversalAdditionalCameraData additionalCameraData = Camera.main.transform.GetComponent<UniversalAdditionalCameraData>();
        additionalCameraData.SetRenderer(0);
    }

    void ExitAfterCharacterTeleport(DCLCharacterPosition position)
    {
        ExitEditMode();
    }

    void FindSceneToEdit()
    {
        foreach (ParcelScene scene in SceneController.i.scenesSortedByDistance)
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
    }
}