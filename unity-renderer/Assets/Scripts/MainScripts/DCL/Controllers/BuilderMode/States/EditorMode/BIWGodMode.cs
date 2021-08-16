using DCL;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using DCL.Camera;
using UnityEngine;
using Environment = DCL.Environment;

public class BIWGodMode : BIWMode
{
    private float initialEagleCameraHeight = 10f;
    private float initialEagleCameraDistance = 10f;
    private float initialEagleCameraLookAtHeight = 0f;

    private float snapDragFactor = 5f;
    internal IFreeCameraMovement freeCameraController;

    private CameraController cameraController;
    private Transform lookAtTransform;
    private MouseCatcher mouseCatcher;
    private PlayerAvatarController avatarRenderer;
    private IBIWGizmosController gizmoManager;
    private IBIWOutlinerController outlinerController;

    private InputAction_Trigger focusOnSelectedEntitiesInputAction;
    private InputAction_Hold multiSelectionInputAction;

    private IParcelScene sceneToEdit;

    internal bool isPlacingNewObject = false;
    internal bool mouseMainBtnPressed = false;
    internal bool mouseSecondaryBtnPressed = false;
    internal bool isSquareMultiSelectionInputActive = false;
    internal bool isMouseDragging = false;
    internal bool changeSnapTemporaryButtonPressed = false;

    internal bool wasGizmosActive = false;
    internal bool isDraggingStarted = false;
    internal bool canDragSelectedEntities = false;

    private bool activateCamera = true;
    private CameraMode.ModeId avatarCameraModeBeforeEditing;

    internal Vector3 lastMousePosition;
    internal Vector3 dragStartedPoint;

    public const float RAYCAST_MAX_DISTANCE = 10000f;

    public override void Init(BIWContext context)
    {
        base.Init(context);

        lookAtTransform = new GameObject("BIWGodModeTransform").transform;
        maxDistanceToSelectEntitiesValue = context.godModeDynamicVariablesAsset.maxDistanceToSelectEntities;

        snapFactor = context.godModeDynamicVariablesAsset.snapFactor;
        snapRotationDegresFactor = context.godModeDynamicVariablesAsset.snapRotationDegresFactor;
        snapScaleFactor =  context.godModeDynamicVariablesAsset.snapScaleFactor;
        snapDistanceToActivateMovement =  context.godModeDynamicVariablesAsset.snapDistanceToActivateMovement;

        initialEagleCameraHeight = context.godModeDynamicVariablesAsset.initialEagleCameraHeight;
        initialEagleCameraDistance = context.godModeDynamicVariablesAsset.initialEagleCameraDistance;
        initialEagleCameraLookAtHeight = context.godModeDynamicVariablesAsset.initialEagleCameraLookAtHeight;

        snapDragFactor = context.godModeDynamicVariablesAsset.snapDragFactor;

        outlinerController = context.outlinerController;
        gizmoManager = context.gizmosController;

        if (HUDController.i.builderInWorldMainHud != null)
        {
            HUDController.i.builderInWorldMainHud.OnTranslateSelectedAction += TranslateMode;
            HUDController.i.builderInWorldMainHud.OnRotateSelectedAction += RotateMode;
            HUDController.i.builderInWorldMainHud.OnScaleSelectedAction += ScaleMode;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange += UpdateSelectionPosition;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange += UpdateSelectionRotation;
            HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange += UpdateSelectionScale;
            HUDController.i.builderInWorldMainHud.OnResetCameraAction += ResetCamera;
            HUDController.i.builderInWorldMainHud.OnPublishAction += TakeSceneScreenshotForPublish;
        }

        if (context.sceneReferences.cameraController.TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
            freeCameraController = (FreeCameraMovement) cameraState;
        mouseCatcher = context.sceneReferences.mouseCatcher;
        avatarRenderer = context.sceneReferences.playerAvatarController;
        cameraController = context.sceneReferences.cameraController;

        BIWInputWrapper.OnMouseDown += OnInputMouseDown;
        BIWInputWrapper.OnMouseUp += OnInputMouseUp;
        BIWInputWrapper.OnMouseUpOnUI += OnInputMouseUpOnUi;
        BIWInputWrapper.OnMouseDrag += OnInputMouseDrag;

        focusOnSelectedEntitiesInputAction = context.inputsReferencesAsset.focusOnSelectedEntitiesInputAction;
        multiSelectionInputAction = context.inputsReferencesAsset.multiSelectionInputAction;

        focusOnSelectedEntitiesInputAction.OnTriggered += (o) => FocusOnSelectedEntitiesInput();

        multiSelectionInputAction.OnStarted += (o) => ChangeSnapTemporaryActivated();
        multiSelectionInputAction.OnFinished += (o) => ChangeSnapTemporaryDeactivated();

        gizmoManager.OnChangeTransformValue += EntitiesTransfromByGizmos;
        gizmoManager.OnGizmoTransformObjectEnd += OnGizmosTransformEnd;
        gizmoManager.OnGizmoTransformObjectStart += OnGizmosTransformStart;
    }

    public override void Dispose()
    {
        base.Dispose();

        isPlacingNewObject = false;

        gizmoManager.OnGizmoTransformObjectEnd -= OnGizmosTransformEnd;
        gizmoManager.OnGizmoTransformObjectStart -= OnGizmosTransformStart;

        BIWInputWrapper.OnMouseDown -= OnInputMouseDown;
        BIWInputWrapper.OnMouseUp -= OnInputMouseUp;
        BIWInputWrapper.OnMouseUpOnUI -= OnInputMouseUpOnUi;
        BIWInputWrapper.OnMouseDrag -= OnInputMouseDrag;

        gizmoManager.OnChangeTransformValue -= EntitiesTransfromByGizmos;

        if (lookAtTransform.gameObject != null)
            GameObject.Destroy(lookAtTransform.gameObject);

        if (HUDController.i.builderInWorldMainHud == null)
            return;

        HUDController.i.builderInWorldMainHud.OnSelectedObjectPositionChange -= UpdateSelectionPosition;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectRotationChange -= UpdateSelectionRotation;
        HUDController.i.builderInWorldMainHud.OnSelectedObjectScaleChange -= UpdateSelectionScale;
        HUDController.i.builderInWorldMainHud.OnTranslateSelectedAction -= TranslateMode;
        HUDController.i.builderInWorldMainHud.OnRotateSelectedAction -= RotateMode;
        HUDController.i.builderInWorldMainHud.OnScaleSelectedAction -= ScaleMode;
        HUDController.i.builderInWorldMainHud.OnResetCameraAction -= ResetCamera;
        HUDController.i.builderInWorldMainHud.OnPublishAction -= TakeSceneScreenshotForPublish;
    }

    public override void Update()
    {
        base.Update();
        if (isPlacingNewObject)
        {
            SetEditObjectAtMouse();
        }
        else if (isSquareMultiSelectionInputActive && isMouseDragging)
        {
            CheckOutlineEntitiesInSquareSelection();
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (mouseMainBtnPressed && isSquareMultiSelectionInputActive)
        {
            var rect = BIWUtils.GetScreenRect(lastMousePosition, Input.mousePosition);
            BIWUtils.DrawScreenRect(rect, new Color(1f, 1f, 1f, 0.25f));
            BIWUtils.DrawScreenRectBorder(rect, 1, Color.white);
        }
    }

    internal void CheckOutlineEntitiesInSquareSelection()
    {
        List<BIWEntity> allEntities = null;

        allEntities = entityHandler.GetAllEntitiesFromCurrentScene();

        foreach (BIWEntity entity in allEntities)
        {
            if (!entity.rootEntity.meshRootGameObject || entity.rootEntity.meshesInfo.renderers.Length <= 0)
                continue;

            if (BIWUtils.IsWithinSelectionBounds(entity.rootEntity.meshesInfo.mergedBounds.center, lastMousePosition, Input.mousePosition))
                outlinerController.OutlineEntity(entity);
            else
                outlinerController.CancelEntityOutline(entity);
        }
    }

    private void ChangeSnapTemporaryDeactivated()
    {
        if (changeSnapTemporaryButtonPressed)
            SetSnapActive(!isSnapActiveValue);

        changeSnapTemporaryButtonPressed = false;
    }

    private void ChangeSnapTemporaryActivated()
    {
        if (selectedEntities.Count == 0)
            return;

        changeSnapTemporaryButtonPressed = true;

        SetSnapActive(!isSnapActiveValue);
    }

    internal void EntitiesTransfromByGizmos(Vector3 transformValue)
    {
        if (gizmoManager.GetSelectedGizmo() != BIWSettings.ROTATE_GIZMO_NAME)
            return;

        foreach (BIWEntity entity in selectedEntities)
        {
            entity.AddRotation(transformValue);
        }
    }

    public void UpdateSelectionPosition(Vector3 newPosition)
    {
        if (selectedEntities.Count != 1)
            return;

        TransformActionStarted(selectedEntities[0].rootEntity, BIWSettings.TRANSLATE_GIZMO_NAME);
        editionGO.transform.position = WorldStateUtils.ConvertSceneToUnityPosition(newPosition, sceneToEdit);
        gizmoManager.SetSelectedEntities(editionGO.transform, selectedEntities);
        TransformActionEnd(selectedEntities[0].rootEntity, BIWSettings.TRANSLATE_GIZMO_NAME);
        ActionFinish(BIWCompleteAction.ActionType.MOVE);
        entityHandler.ReportTransform(true);
        saveController.TryToSave();
    }

    public void UpdateSelectionRotation(Vector3 rotation)
    {
        if (selectedEntities.Count != 1)
            return;

        TransformActionStarted(selectedEntities[0].rootEntity, BIWSettings.ROTATE_GIZMO_NAME);
        selectedEntities[0].rootEntity.gameObject.transform.rotation = Quaternion.Euler(rotation);
        TransformActionEnd(selectedEntities[0].rootEntity, BIWSettings.ROTATE_GIZMO_NAME);
        ActionFinish(BIWCompleteAction.ActionType.ROTATE);
        entityHandler.ReportTransform(true);
        saveController.TryToSave();
    }

    public void UpdateSelectionScale(Vector3 scale)
    {
        if (selectedEntities.Count != 1)
            return;

        var entityToUpdate = selectedEntities[0];

        TransformActionStarted(entityToUpdate.rootEntity, BIWSettings.SCALE_GIZMO_NAME);
        // Before change the scale, we unparent the entity to not to make it dependant on the editionGO and after that, reparent it
        entityToUpdate.rootEntity.gameObject.transform.SetParent(null);
        entityToUpdate.rootEntity.gameObject.transform.localScale = scale;
        editionGO.transform.localScale = Vector3.one;
        entityToUpdate.rootEntity.gameObject.transform.SetParent(editionGO.transform);

        TransformActionEnd(entityToUpdate.rootEntity, BIWSettings.SCALE_GIZMO_NAME);
        ActionFinish(BIWCompleteAction.ActionType.SCALE);
        entityHandler.ReportTransform(true);
        saveController.TryToSave();
    }

    internal void DragEditionGameObject(Vector3 mousePosition)
    {
        Vector3 currentPoint = raycastController.GetFloorPointAtMouse(mousePosition);
        Vector3 initialEntityPosition = editionGO.transform.position;

        if (isSnapActiveValue)
        {
            currentPoint = GetPositionRoundedToSnapFactor(currentPoint);
            initialEntityPosition = GetPositionRoundedToSnapFactor(initialEntityPosition);
        }

        Vector3 move = currentPoint - dragStartedPoint;
        Vector3 destination = initialEntityPosition + move;

        editionGO.transform.position = destination;
        dragStartedPoint = currentPoint;
    }

    #region Mouse

    public override void MouseClickDetected()
    {
        if (isPlacingNewObject)
        {
            entityHandler.DeselectEntities();
            saveController.TryToSave();
            return;
        }

        base.MouseClickDetected();
    }

    internal void OnInputMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (Vector3.Distance(lastMousePosition, mousePosition) <= BIWSettings.MOUSE_THRESHOLD_FOR_DRAG && !isMouseDragging)
            return;

        isMouseDragging = true;
        if (buttonId != 0 ||
            selectedEntities.Count <= 0 ||
            BIWUtils.IsPointerOverMaskElement(BIWSettings.GIZMOS_LAYER) ||
            isSquareMultiSelectionInputActive)
            return;

        if (!isDraggingStarted)
            StarDraggingSelectedEntities();

        if (canDragSelectedEntities)
            DragEditionGameObject(mousePosition);
    }

    internal Vector3 GetPositionRoundedToSnapFactor(Vector3 position)
    {
        position = new Vector3(
            Mathf.Round(position.x / snapDragFactor) * snapDragFactor,
            position.y,
            Mathf.Round(position.z / snapDragFactor) * snapDragFactor);

        return position;
    }

    internal void OnInputMouseUpOnUi(int buttonID, Vector3 position)
    {
        if (buttonID == 1)
        {
            mouseSecondaryBtnPressed = false;
            freeCameraController.StopDetectingMovement();
        }

        if (buttonID != 0)
            return;

        CheckEndBoundMultiselection(position);
        isMouseDragging = false;
    }

    internal void OnInputMouseUp(int buttonID, Vector3 position)
    {
        if (buttonID == 1)
        {
            mouseSecondaryBtnPressed = false;
            if (CanCancelAction(position))
                entityHandler.CancelSelection();

            freeCameraController.StopDetectingMovement();
        }

        if (buttonID != 0)
            return;

        EndDraggingSelectedEntities();

        CheckEndBoundMultiselection(position);

        outlinerController.SetOutlineCheckActive(true);

        isMouseDragging = false;
    }

    internal void OnInputMouseDown(int buttonID, Vector3 position)
    {
        lastMousePosition = position;

        if (buttonID == 1)
        {
            mouseSecondaryBtnPressed = true;
            freeCameraController.StartDectectingMovement();
        }

        if (buttonID != 0)
            return;

        dragStartedPoint = raycastController.GetFloorPointAtMouse(position);

        if (isSnapActiveValue)
        {
            dragStartedPoint.x = Mathf.Round(dragStartedPoint.x);
            dragStartedPoint.z = Mathf.Round(dragStartedPoint.z);
        }

        if (isPlacingNewObject)
            return;

        var entity = raycastController.GetEntityOnPointer();
        if ((entity == null
             || (entity != null && !entity.isSelected))
            && !BIWUtils.IsPointerOverMaskElement(BIWSettings.GIZMOS_LAYER))
        {
            isSquareMultiSelectionInputActive = true;
            outlinerController.SetOutlineCheckActive(false);
        }

        mouseMainBtnPressed = true;
        freeCameraController.SetCameraCanMove(false);
    }

    #endregion

    private void CheckEndBoundMultiselection(Vector3 position)
    {
        if (isSquareMultiSelectionInputActive && mouseMainBtnPressed )
        {
            if (Vector3.Distance(lastMousePosition, position) >= BIWSettings.MOUSE_THRESHOLD_FOR_DRAG)
                EndBoundMultiSelection();

            isSquareMultiSelectionInputActive = false;
            mouseMainBtnPressed = false;
        }
    }

    private bool CanCancelAction(Vector3 currentMousePosition) { return Vector3.Distance(lastMousePosition, currentMousePosition) <= BIWSettings.MOUSE_THRESHOLD_FOR_DRAG && !freeCameraController.HasBeenMovement(); }

    internal void StarDraggingSelectedEntities()
    {
        if (!entityHandler.IsPointerInSelectedEntity() ||
            gizmoManager.HasAxisHover())
            return;

        if (gizmoManager.IsGizmoActive())
        {
            gizmoManager.HideGizmo();
            wasGizmosActive = true;
        }

        freeCameraController.SetCameraCanMove(false);
        isDraggingStarted = true;

        canDragSelectedEntities = true;
    }

    internal void EndDraggingSelectedEntities()
    {
        if (wasGizmosActive && !isPlacingNewObject)
        {
            gizmoManager.ShowGizmo();
            saveController.TryToSave();
        }

        wasGizmosActive = false;

        freeCameraController.SetCameraCanMove(true);
        isDraggingStarted = false;

        canDragSelectedEntities = false;
    }

    internal void EndBoundMultiSelection()
    {
        freeCameraController.SetCameraCanMove(true);
        List<BIWEntity> allEntities = null;

        allEntities = entityHandler.GetAllEntitiesFromCurrentScene();

        List<BIWEntity> selectedInsideBoundsEntities = new List<BIWEntity>();
        int alreadySelectedEntities = 0;

        if (!isMultiSelectionActive)
            entityHandler.DeselectEntities();

        foreach (BIWEntity entity in allEntities)
        {
            if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
            {
                if (BIWUtils.IsWithinSelectionBounds(entity.rootEntity.meshesInfo.mergedBounds.center, lastMousePosition, Input.mousePosition)
                    && !entity.isLocked)
                {
                    if (entity.isSelected)
                        alreadySelectedEntities++;

                    entityHandler.SelectEntity(entity);
                    selectedInsideBoundsEntities.Add(entity);
                }
            }
        }

        if (selectedInsideBoundsEntities.Count == alreadySelectedEntities && alreadySelectedEntities > 0)
        {
            foreach (BIWEntity entity in selectedInsideBoundsEntities)
            {
                entityHandler.DeselectEntity(entity);
            }
        }

        outlinerController.CancelAllOutlines();
    }

    public override void Activate(IParcelScene scene)
    {
        base.Activate(scene);
        sceneToEdit = scene;

        if (activateCamera)
            ActivateCamera(scene);

        if (gizmoManager.GetSelectedGizmo() == DCL.Components.DCLGizmos.Gizmo.NONE)
            gizmoManager.SetGizmoType(BIWSettings.TRANSLATE_GIZMO_NAME);
        mouseCatcher.enabled = false;
        Environment.i.world.sceneController.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();

        gizmoManager.HideGizmo();
        editionGO.transform.SetParent(null);
        avatarRenderer.SetAvatarVisibility(false);

        HUDController.i.builderInWorldMainHud?.ActivateGodModeUI();
    }

    public void ActivateCamera(IParcelScene parcelScene)
    {
        freeCameraController.gameObject.SetActive(true);
        SetLookAtObject(parcelScene);

        Vector3 cameraPosition = GetInitialCameraPosition(parcelScene);
        freeCameraController.SetPosition(cameraPosition);
        freeCameraController.LookAt(lookAtTransform);
        freeCameraController.SetResetConfiguration(cameraPosition, lookAtTransform);

        if (cameraController.currentCameraState.cameraModeId != CameraMode.ModeId.BuildingToolGodMode)
            avatarCameraModeBeforeEditing = cameraController.currentCameraState.cameraModeId;

        cameraController.SetCameraMode(CameraMode.ModeId.BuildingToolGodMode);
    }

    public override void OnDeleteEntity(BIWEntity entity)
    {
        base.OnDeleteEntity(entity);
        saveController.TryToSave();

        if (selectedEntities.Count == 0)
            gizmoManager.HideGizmo();
    }

    public override void OnDeselectedEntities()
    {
        base.OnDeselectedEntities();
        gizmoManager.SetSelectedEntities(editionGO.transform, new List<BIWEntity>());
    }

    public override void Deactivate()
    {
        base.Deactivate();
        mouseCatcher.enabled = true;
        Utils.LockCursor();
        cameraController.SetCameraMode(avatarCameraModeBeforeEditing);

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

    public override void CreatedEntity(BIWEntity createdEntity)
    {
        base.CreatedEntity(createdEntity);

        if (!createdEntity.isFloor)
        {
            isPlacingNewObject = true;
            outlinerController.SetOutlineCheckActive(false);

            SetEditObjectAtMouse();
        }

        gizmoManager.HideGizmo();
        if (createdEntity.isVoxel)
            createdEntity.rootEntity.gameObject.tag = BIWSettings.VOXEL_TAG;
    }

    public override Vector3 GetCreatedEntityPoint() { return raycastController.GetFloorPointAtMouse(Input.mousePosition); }

    public override void SelectedEntity(BIWEntity selectedEntity)
    {
        base.SelectedEntity(selectedEntity);

        gizmoManager.SetSelectedEntities(editionGO.transform, selectedEntities);

        if (!isMultiSelectionActive && !selectedEntity.isNew)
            TryLookAtEntity(selectedEntity.rootEntity);

        snapGO.transform.SetParent(null);

        UpdateActionsInteractable();
    }

    public override void EntityDeselected(BIWEntity entityDeselected)
    {
        base.EntityDeselected(entityDeselected);
        if (selectedEntities.Count <= 0)
        {
            gizmoManager.HideGizmo();
            UpdateActionsInteractable();
        }

        if (isPlacingNewObject)
            outlinerController.SetOutlineCheckActive(true);
        isPlacingNewObject = false;
    }

    public override void EntityDoubleClick(BIWEntity entity)
    {
        base.EntityDoubleClick(entity);
        if (!entity.isLocked)
            LookAtEntity(entity.rootEntity);
    }

    private void UpdateActionsInteractable()
    {
        bool areInteratable = selectedEntities.Count > 0;
        if (HUDController.i.builderInWorldMainHud != null)
            HUDController.i.builderInWorldMainHud.SetActionsButtonsInteractable(areInteratable);
    }

    public override bool ShouldCancelUndoAction()
    {
        if (isPlacingNewObject)
        {
            entityHandler.DestroyLastCreatedEntities();
            isPlacingNewObject = false;
            return true;
        }

        return false;
    }

    public override void SetSnapActive(bool isActive)
    {
        base.SetSnapActive(isActive);

        if (isSnapActiveValue)
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
            FocusEntities(selectedEntities);
            InputDone();
        }
    }

    public void TryLookAtEntity(IDCLEntity entity)
    {
        if (entity.meshRootGameObject == null
            || entity.meshesInfo == null
            || BIWUtils.IsBoundInsideCamera(entity.meshesInfo.mergedBounds))
            return;

        LookAtEntity(entity);
    }

    public void LookAtEntity(IDCLEntity entity)
    {
        Vector3 pointToLook = entity.gameObject.transform.position;
        if (entity.meshesInfo != null && entity.meshesInfo.renderers.Length > 0)
            pointToLook = CalculateEntityMidPoint(entity);

        freeCameraController.SmoothLookAt(pointToLook);
    }

    internal Vector3 CalculateEntityMidPoint(IDCLEntity entity)
    {
        Vector3 midPointFromEntityMesh = Vector3.zero;
        foreach (Renderer render in entity.renderers)
        {
            if (render == null)
                continue;
            midPointFromEntityMesh += render.bounds.center;
        }

        midPointFromEntityMesh /= entity.renderers.Length;
        return midPointFromEntityMesh;
    }

    #region Gizmos

    public void TranslateMode() { GizmosMode( BIWSettings.TRANSLATE_GIZMO_NAME); }

    public void RotateMode() { GizmosMode( BIWSettings.ROTATE_GIZMO_NAME); }

    public void ScaleMode() { GizmosMode( BIWSettings.SCALE_GIZMO_NAME); }

    private void GizmosMode(string gizmos)
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
        //TODO: Free-Movement tool, This could be re-enabled in the future so let the code there 
        // else
        // {
        //     gizmoManager.HideGizmo(true);
        //     HUDController.i.builderInWorldMainHud?.SetGizmosActive(BIWSettings.EMPTY_GIZMO_NAME);
        // }
    }

    internal void OnGizmosTransformStart(string gizmoType)
    {
        outlinerController.SetOutlineCheckActive(false);
        foreach (BIWEntity entity in selectedEntities)
        {
            TransformActionStarted(entity.rootEntity, gizmoType);
        }
    }

    internal void OnGizmosTransformEnd(string gizmoType)
    {
        outlinerController.SetOutlineCheckActive(true);
        foreach (BIWEntity entity in selectedEntities)
        {
            TransformActionEnd(entity.rootEntity, gizmoType);
        }

        switch (gizmoType)
        {
            case BIWSettings.TRANSLATE_GIZMO_NAME:

                ActionFinish(BIWCompleteAction.ActionType.MOVE);
                break;
            case BIWSettings.ROTATE_GIZMO_NAME:
                ActionFinish(BIWCompleteAction.ActionType.ROTATE);
                break;
            case BIWSettings.SCALE_GIZMO_NAME:
                ActionFinish(BIWCompleteAction.ActionType.SCALE);
                break;
        }

        saveController.TryToSave();
    }

    #endregion

    public void FocusEntities(List<BIWEntity> entitiesToFocus) { freeCameraController.FocusOnEntities(entitiesToFocus); }

    internal Vector3 GetInitialCameraPosition(IParcelScene parcelScene)
    {
        Vector3 middlePoint = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
        Vector3 direction = (parcelScene.GetSceneTransform().position - middlePoint).normalized;

        return parcelScene.GetSceneTransform().position
               + direction * initialEagleCameraDistance
               + Vector3.up * initialEagleCameraHeight;
    }

    internal void SetLookAtObject(IParcelScene parcelScene)
    {
        Vector3 middlePoint = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
        lookAtTransform.position = middlePoint + Vector3.up * initialEagleCameraLookAtHeight;
    }

    internal void SetEditObjectAtMouse()
    {
        if (raycastController.RayCastFloor(out Vector3 destination))
        {
            if (isSnapActiveValue)
            {
                destination.x = Mathf.Round(destination.x / snapDragFactor) * snapDragFactor;
                destination.z = Mathf.Round(destination.z / snapDragFactor) * snapDragFactor;
            }

            editionGO.transform.position = destination;

            if (selectedEntities.Count > 0 && selectedEntities[0].isNFT)
                editionGO.transform.position += Vector3.up * 2f;
        }
    }

    internal void ResetCamera() { freeCameraController.ResetCameraPosition(); }

    internal void TakeSceneScreenshotForPublish()
    {
        entityHandler.DeselectEntities();

        freeCameraController.TakeSceneScreenshot((sceneSnapshot) =>
        {
            HUDController.i.builderInWorldMainHud?.SetBuilderProjectScreenshot(sceneSnapshot);
        });
    }

    public void TakeSceneScreenshotForExit()
    {
        entityHandler.DeselectEntities();

        freeCameraController.TakeSceneScreenshotFromResetPosition((sceneSnapshot) =>
        {
            HUDController.i.builderInWorldMainHud?.SetBuilderProjectScreenshot(sceneSnapshot);
        });
    }

    public void OpenNewProjectDetails()
    {
        entityHandler.DeselectEntities();

        freeCameraController.TakeSceneScreenshot((sceneSnapshot) =>
        {
            HUDController.i.builderInWorldMainHud?.NewProjectStart(sceneSnapshot);
        });
    }
}