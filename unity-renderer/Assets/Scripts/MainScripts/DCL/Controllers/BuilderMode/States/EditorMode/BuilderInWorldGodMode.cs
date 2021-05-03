using Builder.Gizmos;
using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Environment = DCL.Environment;

public class BuilderInWorldGodMode : BuilderInWorldMode
{
    [Header("Editor Design")]
    public float distanceEagleCamera = 20f;
    public LayerMask layerToStopClick;
    public float snapDragFactor = 5f;

    [Header("Scenes References")]
    public FreeCameraMovement freeCameraController;

    public CameraController cameraController;
    public Transform lookAtT;
    public MouseCatcher mouseCatcher;
    public PlayerAvatarController avatarRenderer;

    [Header("Prefab References")]
    public DCLBuilderGizmoManager gizmoManager;

    public VoxelController voxelController;
    public BIWOutlinerController outlinerController;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger focusOnSelectedEntitiesInputAction;

    [SerializeField]
    internal InputAction_Hold multiSelectionInputAction;

    ParcelScene sceneToEdit;

    public LayerMask groundLayer;

    private bool isPlacingNewObject = false;
    private bool mouseMainBtnPressed = false;
    private bool mouseSecondaryBtnPressed = false;
    private bool isSquareMultiSelectionInputActive = false;
    private bool isMouseDragging = false;
    private bool isTypeOfBoundSelectionSelected = false;
    private bool isVoxelBoundMultiSelection = false;
    private bool multiSelectionButtonPressed = false;
    private bool changeSnapTemporaryButtonPressed = false;

    private bool wasGizmosActive = false;
    private bool isDraggingStarted = false;
    private bool canDragSelectedEntities = false;

    private bool activateCamera = true;

    private Vector3 lastMousePosition;
    private Vector3 dragStartedPoint;

    public const float RAYCAST_MAX_DISTANCE = 10000f;

    private void Start()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmosTransformStart;

        BuilderInWorldInputWrapper.OnMouseDown += OnInputMouseDown;
        BuilderInWorldInputWrapper.OnMouseUp += OnInputMouseUp;
        BuilderInWorldInputWrapper.OnMouseDrag += OnInputMouseDrag;

        focusOnSelectedEntitiesInputAction.OnTriggered += (o) => FocusOnSelectedEntitiesInput();

        multiSelectionInputAction.OnStarted += (o) => multiSelectionButtonPressed = true;
        multiSelectionInputAction.OnFinished += (o) => multiSelectionButtonPressed = false;

        multiSelectionInputAction.OnStarted += (o) => ChangeSnapTemporaryActivated();
        multiSelectionInputAction.OnFinished += (o) => ChangeSnapTemporaryDeactivated();

        gizmoManager.OnChangeTransformValue += EntitiesTransfromByGizmos;
    }

    private void OnDestroy()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart -= OnGizmosTransformStart;

        BuilderInWorldInputWrapper.OnMouseDown -= OnInputMouseDown;
        BuilderInWorldInputWrapper.OnMouseUp -= OnInputMouseUp;
        BuilderInWorldInputWrapper.OnMouseDrag -= OnInputMouseDrag;

        gizmoManager.OnChangeTransformValue -= EntitiesTransfromByGizmos;

        if (HUDController.i.builderInWorldMainHud == null)
            return;

        HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange -= UpdateSelectionPosition;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange -= UpdateSelectionRotation;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange -= UpdateSelectionScale;

        HUDController.i.builderInWorldMainHud.OnTranslateSelectedAction -= TranslateMode;
        HUDController.i.builderInWorldMainHud.OnRotateSelectedAction -= RotateMode;
        HUDController.i.builderInWorldMainHud.OnScaleSelectedAction -= ScaleMode;
    }

    private void Update()
    {
        if (isPlacingNewObject)
        {
            if (!voxelController.IsActive())
                SetEditObjectAtMouse();
            else
                voxelController.SetEditObjectLikeVoxel();
        }
        else if (isSquareMultiSelectionInputActive && isMouseDragging)
        {
            List<DCLBuilderInWorldEntity> allEntities = null;
            if (!isTypeOfBoundSelectionSelected || !isVoxelBoundMultiSelection)
                allEntities = builderInWorldEntityHandler.GetAllEntitiesFromCurrentScene();
            else if (isVoxelBoundMultiSelection)
                allEntities = builderInWorldEntityHandler.GetAllVoxelsEntities();

            foreach (DCLBuilderInWorldEntity entity in allEntities)
            {
                if (entity.isVoxel && !isVoxelBoundMultiSelection && isTypeOfBoundSelectionSelected)
                    continue;
                if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
                {
                    if (BuilderInWorldUtils.IsWithInSelectionBounds(entity.rootEntity.meshesInfo.mergedBounds.center, lastMousePosition, Input.mousePosition))
                    {
                        if (!isTypeOfBoundSelectionSelected && !entity.IsLocked)
                        {
                            if (entity.isVoxel)
                                isVoxelBoundMultiSelection = true;
                            else
                                isVoxelBoundMultiSelection = false;
                            isTypeOfBoundSelectionSelected = true;
                        }

                        outlinerController.OutlineEntity(entity);
                    }
                    else
                    {
                        outlinerController.CancelEntityOutline(entity);
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        if (mouseMainBtnPressed && isSquareMultiSelectionInputActive)
        {
            var rect = BuilderInWorldUtils.GetScreenRect(lastMousePosition, Input.mousePosition);
            BuilderInWorldUtils.DrawScreenRect(rect, new Color(1f, 1f, 1f, 0.5f));
            BuilderInWorldUtils.DrawScreenRectBorder(rect, 1, Color.white);
        }
    }

    private void ChangeSnapTemporaryDeactivated()
    {
        if (changeSnapTemporaryButtonPressed)
            SetSnapActive(!isSnapActive);

        changeSnapTemporaryButtonPressed = false;
    }

    private void ChangeSnapTemporaryActivated()
    {
        if (!mouseMainBtnPressed || !gizmoManager.HasAxisHover())
            return;

        changeSnapTemporaryButtonPressed = true;

        SetSnapActive(!isSnapActive);
    }

    private void EntitiesTransfromByGizmos(Vector3 transformValue)
    {
        if (gizmoManager.GetSelectedGizmo() != BuilderInWorldSettings.ROTATE_GIZMO_NAME)
            return;

        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            entity.AddRotation(transformValue);
        }
    }

    public void UpdateSelectionPosition(Vector3 newPosition)
    {
        if (selectedEntities.Count != 1)
            return;

        editionGO.transform.position = WorldStateUtils.ConvertSceneToUnityPosition(newPosition, sceneToEdit);
        UpdateGizmosToSelectedEntities();
    }

    public void UpdateSelectionRotation(Vector3 rotation)
    {
        if (selectedEntities.Count != 1)
            return;

        selectedEntities[0].transform.rotation = Quaternion.Euler(rotation);
    }

    public void UpdateSelectionScale(Vector3 scale)
    {
        if (selectedEntities.Count != 1)
            return;

        editionGO.transform.localScale = scale;
    }

    public void UpdateGizmosToSelectedEntities()
    {
        List<EditableEntity> editableEntities = new List<EditableEntity>();
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            editableEntities.Add(entity);
        }

        gizmoManager.SetSelectedEntities(editionGO.transform, editableEntities);
    }

    public override void Init(GameObject goToEdit, GameObject undoGo, GameObject snapGO, GameObject freeMovementGO, List<DCLBuilderInWorldEntity> selectedEntities)
    {
        base.Init(goToEdit, undoGo, snapGO, freeMovementGO, selectedEntities);
        voxelController.SetEditionGO(goToEdit);

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnTranslateSelectedAction += TranslateMode;
            HUDController.i.builderInWorldMainHud.OnRotateSelectedAction += RotateMode;
            HUDController.i.builderInWorldMainHud.OnScaleSelectedAction += ScaleMode;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange += UpdateSelectionPosition;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange += UpdateSelectionRotation;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange += UpdateSelectionScale;
        }
    }

    #region Mouse

    private void OnInputMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (Vector3.Distance(lastMousePosition, mousePosition) <= BuilderInWorldSettings.MOUSE_THRESHOLD_FOR_DRAG && !isMouseDragging)
            return;

        isMouseDragging = true;
        if (buttonId != 0 ||
            selectedEntities.Count <= 0 ||
            BuilderInWorldUtils.IsPointerOverMaskElement(layerToStopClick) ||
            isSquareMultiSelectionInputActive ||
            multiSelectionButtonPressed)
            return;

        if (!isDraggingStarted)
            StarDraggingSelectedEntities();

        if (canDragSelectedEntities)
        {
            Vector3 destination;
            Vector3 currentPoint = GetFloorPointAtMouse(mousePosition);


            if (isSnapActive)
            {
                currentPoint.x = Mathf.Round(currentPoint.x / snapDragFactor) * snapDragFactor;
                currentPoint.z = Mathf.Round(currentPoint.z / snapDragFactor) * snapDragFactor;
                destination = currentPoint;
            }
            else
            {
                destination = currentPoint - dragStartedPoint + editionGO.transform.position;
            }

            editionGO.transform.position = destination;
            dragStartedPoint = currentPoint;
        }
    }

    private void OnInputMouseUp(int buttonID, Vector3 position)
    {
        if (buttonID == 1)
        {
            mouseSecondaryBtnPressed = false;
            if (CanCancelAction(position))
                builderInWorldEntityHandler.CancelSelection();

            freeCameraController.StopDetectingMovement();
        }

        if (buttonID != 0)
            return;

        EndDraggingSelectedEntities();

        if (isSquareMultiSelectionInputActive && mouseMainBtnPressed )
        {
            if (Vector3.Distance(lastMousePosition, position) >= BuilderInWorldSettings.MOUSE_THRESHOLD_FOR_DRAG)
                EndBoundMultiSelection();

            isSquareMultiSelectionInputActive = false;
            mouseMainBtnPressed = false;
        }
        outlinerController.SetOutlineCheckActive(true);

        isMouseDragging = false;
    }

    void OnInputMouseDown(int buttonID, Vector3 position)
    {
        lastMousePosition = position;

        if (buttonID == 1)
        {
            mouseSecondaryBtnPressed = true;
            freeCameraController.StartDectectingMovement();
        }

        if (buttonID != 0)
            return;

        dragStartedPoint = GetFloorPointAtMouse(position);

        if (isPlacingNewObject)
            return;

        var entity = builderInWorldEntityHandler.GetEntityOnPointer();
        if ((entity == null
             || (entity != null && !entity.IsSelected))
            && !BuilderInWorldUtils.IsPointerOverMaskElement(layerToStopClick))
        {
            isSquareMultiSelectionInputActive = true;
            isTypeOfBoundSelectionSelected = false;
            isVoxelBoundMultiSelection = false;
            outlinerController.SetOutlineCheckActive(false);
        }
        mouseMainBtnPressed = true;
        freeCameraController.SetCameraCanMove(false);
    }

    #endregion

    private bool CanCancelAction(Vector3 currentMousePosition) { return Vector3.Distance(lastMousePosition, currentMousePosition) <= BuilderInWorldSettings.MOUSE_THRESHOLD_FOR_DRAG && !freeCameraController.HasBeenMovement; }

    private void StarDraggingSelectedEntities()
    {
        if (!builderInWorldEntityHandler.IsPointerInSelectedEntity() ||
            gizmoManager.HasAxisHover() ||
            voxelController.IsActive())
            return;

        if (gizmoManager.isActiveAndEnabled)
        {
            gizmoManager.HideGizmo();
            wasGizmosActive = true;
        }

        freeCameraController.SetCameraCanMove(false);
        isDraggingStarted = true;

        canDragSelectedEntities = true;
    }

    void EndDraggingSelectedEntities()
    {
        if (wasGizmosActive && !isPlacingNewObject)
        {
            gizmoManager.ShowGizmo();
        }

        wasGizmosActive = false;

        freeCameraController.SetCameraCanMove(true);
        isDraggingStarted = false;

        canDragSelectedEntities = false;
    }

    private void EndBoundMultiSelection()
    {
        freeCameraController.SetCameraCanMove(true);
        List<DCLBuilderInWorldEntity> allEntities = null;
        if (!isVoxelBoundMultiSelection)
            allEntities = builderInWorldEntityHandler.GetAllEntitiesFromCurrentScene();
        else
            allEntities = builderInWorldEntityHandler.GetAllVoxelsEntities();

        List<DCLBuilderInWorldEntity> selectedInsideBoundsEntities = new List<DCLBuilderInWorldEntity>();
        int alreadySelectedEntities = 0;

        if (!isMultiSelectionActive)
            builderInWorldEntityHandler.DeselectEntities();

        foreach (DCLBuilderInWorldEntity entity in allEntities)
        {
            if ((entity.isVoxel && !isVoxelBoundMultiSelection) ||
                !entity.IsVisible)
                continue;

            if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
            {
                if (BuilderInWorldUtils.IsWithInSelectionBounds(entity.rootEntity.meshesInfo.mergedBounds.center, lastMousePosition, Input.mousePosition)
                    && !entity.IsLocked)
                {
                    if (entity.IsSelected)
                        alreadySelectedEntities++;

                    builderInWorldEntityHandler.SelectEntity(entity);
                    selectedInsideBoundsEntities.Add(entity);
                }
            }
        }
        if (selectedInsideBoundsEntities.Count == alreadySelectedEntities && alreadySelectedEntities > 0)
        {
            foreach (DCLBuilderInWorldEntity entity in selectedInsideBoundsEntities)
            {
                builderInWorldEntityHandler.DeselectEntity(entity);
            }
        }

        outlinerController.CancelAllOutlines();
    }

    #region Voxel

    public void ActivateVoxelMode() { voxelController.SetActiveMode(true); }

    public void DesactivateVoxelMode() { voxelController.SetActiveMode(false); }

    #endregion

    public override void Activate(ParcelScene scene)
    {
        base.Activate(scene);
        gameObject.SetActive(true);
        sceneToEdit = scene;
        voxelController.SetSceneToEdit(scene);

        if (activateCamera)
            ActivateCamera(scene);

        if (gizmoManager.GetSelectedGizmo() == DCL.Components.DCLGizmos.Gizmo.NONE)
            gizmoManager.SetGizmoType(BuilderInWorldSettings.TRANSLATE_GIZMO_NAME);
        mouseCatcher.enabled = false;
        Environment.i.world.sceneController.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();

        gizmoManager.HideGizmo();
        editionGO.transform.SetParent(null);
        avatarRenderer.SetAvatarVisibility(false);

        HUDController.i.builderInWorldMainHud?.ActivateGodModeUI();
    }

    public void ActivateCamera(ParcelScene parcelScene)
    {
        freeCameraController.gameObject.SetActive(true);
        SetLookAtObject(parcelScene);

        // NOTE(Adrian): Take into account that right now to get the relative scale of the gizmos, we set the gizmos in the player position and the camera
        Vector3 cameraPosition = DCLCharacterController.i.characterPosition.unityPosition;
        freeCameraController.SetPosition(cameraPosition + Vector3.up * distanceEagleCamera);
        //

        freeCameraController.LookAt(lookAtT);

        cameraController.SetCameraMode(CameraMode.ModeId.BuildingToolGodMode);

        gizmoManager.InitializeGizmos(Camera.main, freeCameraController.transform);
        gizmoManager.ForceRelativeScaleRatio();
    }

    public override void OnDeleteEntity(DCLBuilderInWorldEntity entity)
    {
        base.OnDeleteEntity(entity);
        if (selectedEntities.Count == 0)
            gizmoManager.HideGizmo();
    }

    public override void OnDeselectedEntities()
    {
        base.OnDeselectedEntities();
        gizmoManager.SetSelectedEntities(editionGO.transform, new List<EditableEntity>());
    }

    public override void Deactivate()
    {
        base.Deactivate();
        mouseCatcher.enabled = true;
        Utils.LockCursor();
        cameraController.SetCameraMode(CameraMode.ModeId.FirstPerson);

        Environment.i.world.sceneController.ReIntegrateIsolatedScene();

        gizmoManager.HideGizmo();
        RenderSettings.fog = true;
        avatarRenderer.SetAvatarVisibility(true);
    }

    public override void StartMultiSelection()
    {
        base.StartMultiSelection();

        snapGO.transform.SetParent(null);
        freeMovementGO.transform.SetParent(null);
    }

    public override void SetDuplicationOffset(float offset)
    {
        base.SetDuplicationOffset(offset);
        editionGO.transform.position += Vector3.right * offset;
    }

    public override void CreatedEntity(DCLBuilderInWorldEntity createdEntity)
    {
        base.CreatedEntity(createdEntity);

        if (!createdEntity.isFloor)
            isPlacingNewObject = true;

        gizmoManager.HideGizmo();
        if (createdEntity.isVoxel)
        {
            createdEntity.rootEntity.gameObject.tag = BuilderInWorldSettings.VOXEL_TAG;
            voxelController.SetVoxelSelected(createdEntity);
            ActivateVoxelMode();
        }
    }

    public override Vector3 GetCreatedEntityPoint() { return GetFloorPointAtMouse(Input.mousePosition); }

    public override void SelectedEntity(DCLBuilderInWorldEntity selectedEntity)
    {
        base.SelectedEntity(selectedEntity);

        List<EditableEntity> editableEntities = new List<EditableEntity>();
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            editableEntities.Add(entity);
        }

        gizmoManager.SetSelectedEntities(editionGO.transform, editableEntities);

        if (!isMultiSelectionActive && !selectedEntity.IsNew)
            LookAtEntity(selectedEntity.rootEntity);

        snapGO.transform.SetParent(null);
        if (selectedEntity.isVoxel && selectedEntities.Count == 0)
        {
            editionGO.transform.position = voxelController.ConverPositionToVoxelPosition(editionGO.transform.position);
        }
        UpdateActionsInteractable();
    }

    public override void EntityDeselected(DCLBuilderInWorldEntity entityDeselected)
    {
        base.EntityDeselected(entityDeselected);
        if (selectedEntities.Count <= 0)
        {
            gizmoManager.HideGizmo();
            UpdateActionsInteractable();
        }

        isPlacingNewObject = false;
        DesactivateVoxelMode();
    }

    private void UpdateActionsInteractable()
    {
        bool areInteratable = selectedEntities.Count > 0;
        HUDController.i.builderInWorldMainHud.SetActionsButtonsInteractable(areInteratable);
    }

    public override bool ShouldCancelUndoAction()
    {
        if (isPlacingNewObject)
        {
            builderInWorldEntityHandler.DestroyLastCreatedEntities();
            isPlacingNewObject = false;
            return true;
        }

        return false;
    }

    public override void SetSnapActive(bool isActive)
    {
        base.SetSnapActive(isActive);

        if (isSnapActive)
        {
            gizmoManager.SetSnapFactor(snapFactor, snapRotationDegresFactor, snapScaleFactor);
        }
        else
        {
            gizmoManager.SetSnapFactor(0, 0, 0);
        }
    }

    public void FocusOnSelectedEntitiesInput()
    {
        if (isModeActive)
        {
            FocusGameObject(selectedEntities);
            InputDone();
        }
    }

    public void LookAtEntity(IDCLEntity entity)
    {
        if (entity.meshRootGameObject == null
            || entity.meshesInfo == null
            || BuilderInWorldUtils.IsBoundInsideCamera(entity.meshesInfo.mergedBounds))
            return;

        Vector3 pointToLook = entity.gameObject.transform.position;
        if (entity.meshesInfo.renderers.Length > 0)
        {
            Vector3 midPointFromEntityMesh = Vector3.zero;
            foreach (Renderer render in entity.renderers)
            {
                midPointFromEntityMesh += render.bounds.center;
            }

            midPointFromEntityMesh /= entity.renderers.Length;
            pointToLook = midPointFromEntityMesh;
        }

        freeCameraController.SmoothLookAt(pointToLook);
    }

    #region Gizmos

    public void TranslateMode() { GizmosMode( BuilderInWorldSettings.TRANSLATE_GIZMO_NAME); }

    public void RotateMode() { GizmosMode( BuilderInWorldSettings.ROTATE_GIZMO_NAME); }

    public void ScaleMode() { GizmosMode( BuilderInWorldSettings.SCALE_GIZMO_NAME); }

    void GizmosMode(string gizmos)
    {
        if ((!isModeActive && isPlacingNewObject) || mouseSecondaryBtnPressed)
            return;
        if (gizmoManager.GetSelectedGizmo() != gizmos)
        {
            HUDController.i.builderInWorldMainHud?.SetGizmosActive(gizmos);
            gizmoManager.SetGizmoType(gizmos);
            if (selectedEntities.Count > 0 )
                gizmoManager.ShowGizmo();
        }
        else
        {
            gizmoManager.HideGizmo(true);
            HUDController.i.builderInWorldMainHud?.SetGizmosActive(BuilderInWorldSettings.EMPTY_GIZMO_NAME);
        }
    }

    void OnGizmosTransformStart(string gizmoType)
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            TransformActionStarted(entity.rootEntity, gizmoType);
        }
    }

    void OnGizmosTransformEnd(string gizmoType)
    {
        foreach (DCLBuilderInWorldEntity entity in selectedEntities)
        {
            TransformActionEnd(entity.rootEntity, gizmoType);
        }

        switch (gizmoType)
        {
            case BuilderInWorldSettings.TRANSLATE_GIZMO_NAME:

                ActionFinish(BuildInWorldCompleteAction.ActionType.MOVE);
                break;
            case BuilderInWorldSettings.ROTATE_GIZMO_NAME:

                ActionFinish(BuildInWorldCompleteAction.ActionType.ROTATE);
                break;
            case BuilderInWorldSettings.SCALE_GIZMO_NAME:
                ActionFinish(BuildInWorldCompleteAction.ActionType.SCALE);
                break;
        }
    }

    #endregion

    public void FocusGameObject(List<DCLBuilderInWorldEntity> entitiesToFocus) { freeCameraController.FocusOnEntities(entitiesToFocus); }

    void SetLookAtObject(ParcelScene parcelScene)
    {
        Vector3 middlePoint = BuilderInWorldUtils.CalculateUnityMiddlePoint(parcelScene);

        lookAtT.position = middlePoint;
    }

    void SetEditObjectAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, groundLayer))
        {
            Vector3 destination = hit.point;
            if (isSnapActive)
            {
                destination.x = Mathf.Round(destination.x / snapDragFactor) * snapDragFactor;
                destination.z = Mathf.Round(destination.z / snapDragFactor) * snapDragFactor;
            }

            editionGO.transform.position = destination;

            if (selectedEntities.Count > 0 && selectedEntities[0].isNFT)
                editionGO.transform.position += Vector3.up * 2f;
        }
    }

    Vector3 GetFloorPointAtMouse(Vector3 mousePosition)
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}