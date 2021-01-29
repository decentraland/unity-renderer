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

    [Header("Scenes References")]
    public FreeCameraMovement freeCameraController;

    public DCLBuilderGizmoManager gizmoManager;
    public VoxelController voxelController;
    public BuilderInWorldInputWrapper builderInputWrapper;
    public OutlinerController outlinerController;
    public BuilderInWorldController buildModeController;
    public CameraController cameraController;
    public Transform lookAtT;
    public MouseCatcher mouseCatcher;

    [Header("InputActions")]
    [SerializeField]
    internal InputAction_Trigger focusOnSelectedEntitiesInputAction;

    [SerializeField]
    internal InputAction_Hold squareMultiSelectionInputAction;


    ParcelScene sceneToEdit;

    public LayerMask groundLayer;

    bool isPlacingNewObject = false;
    bool mousePressed = false;
    bool isDoingSquareMultiSelection = false;
    bool isTypeOfBoundSelectionSelected = false;
    bool isVoxelBoundMultiSelection = false;
    bool squareMultiSelectionButtonPressed = false;

    bool wasGizmosActive = false;
    bool isDraggingStarted = false;
    bool canDragSelectedEntities = false;

    bool activateCamera = true;

    Vector3 lastMousePosition;
    Vector3 dragStartedPoint;

    public const float RAYCAST_MAX_DISTANCE = 10000f;

    private void Start()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmosTransformStart;

        builderInputWrapper.OnMouseDown += OnMouseDown;
        builderInputWrapper.OnMouseUp += OnMouseUp;
        builderInputWrapper.OnMouseDrag += OnMouseDrag;


        focusOnSelectedEntitiesInputAction.OnTriggered += (o) => FocusOnSelectedEntitiesInput();

        squareMultiSelectionInputAction.OnStarted += (o) => squareMultiSelectionButtonPressed = true;
        squareMultiSelectionInputAction.OnFinished += (o) => squareMultiSelectionButtonPressed = false;
    }

    private void OnDestroy()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd -= OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart -= OnGizmosTransformStart;

        builderInputWrapper.OnMouseDown -= OnMouseDown;
        builderInputWrapper.OnMouseUp -= OnMouseUp;
        builderInputWrapper.OnMouseDrag -= OnMouseDrag;
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
        else if (isDoingSquareMultiSelection)
        {
            if (!squareMultiSelectionButtonPressed)
            {
                EndBoundMultiSelection();
            }
            else
            {
                List<DCLBuilderInWorldEntity> allEntities = null;
                if (!isTypeOfBoundSelectionSelected || !isVoxelBoundMultiSelection)
                    allEntities = builderInWorldEntityHandler.GetAllEntitiesFromCurrentScene();
                else if (isVoxelBoundMultiSelection)
                    allEntities = builderInWorldEntityHandler.GetAllVoxelsEntities();

                foreach (DCLBuilderInWorldEntity entity in allEntities)
                {
                    if (entity.isVoxel && !isVoxelBoundMultiSelection && isTypeOfBoundSelectionSelected) continue;
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
    }

    private void OnGUI()
    {
        if (mousePressed && isDoingSquareMultiSelection)
        {
            var rect = BuilderInWorldUtils.GetScreenRect(lastMousePosition, Input.mousePosition);
            BuilderInWorldUtils.DrawScreenRect(rect, new Color(1f, 1f, 1f, 0.5f));
            BuilderInWorldUtils.DrawScreenRectBorder(rect, 1, Color.white);
        }
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
        }
    }

    public void SetActivateCamera(bool shouldActivate)
    {
        activateCamera = shouldActivate;
    }

    private void OnMouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (buttonId != 0 ||
            selectedEntities.Count <= 0)
            return;

        if(!isDraggingStarted)
            StarDraggingSelectedEntities();

        if (canDragSelectedEntities)
        {
            Vector3 currentPoint = GetFloorPointAtMouse(mousePosition);
            Vector3 direction = currentPoint - dragStartedPoint;

            editionGO.transform.position += direction;
            dragStartedPoint = currentPoint;
        }

    }

    private void OnMouseUp(int buttonID, Vector3 position)
    {
        if (buttonID != 0)
            return;

        EndDraggingSelectedEntities();

        if (isDoingSquareMultiSelection && mousePressed)
        {
            EndBoundMultiSelection();
        }
    }

    void OnMouseDown(int buttonID, Vector3 position)
    {
        if (buttonID != 0)
            return;

        dragStartedPoint = GetFloorPointAtMouse(position);

        if (!squareMultiSelectionButtonPressed)
            return;

        isDoingSquareMultiSelection = true;
        isTypeOfBoundSelectionSelected = false;
        isVoxelBoundMultiSelection = false;
        lastMousePosition = position;
        mousePressed = true;
        freeCameraController.SetCameraCanMove(false);
        buildModeController.SetOutlineCheckActive(false);
    }

    void StarDraggingSelectedEntities()
    {
        if (!builderInWorldEntityHandler.IsPointerInSelectedEntity() ||
            gizmoManager.HasAxisHover())
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
        if(wasGizmosActive)
        {
            gizmoManager.ShowGizmo();
        }
        wasGizmosActive = false;

        freeCameraController.SetCameraCanMove(true);
        isDraggingStarted = false;

        canDragSelectedEntities = false;
    }

    public void EndBoundMultiSelection()
    {
        isDoingSquareMultiSelection = false;
        mousePressed = false;
        freeCameraController.SetCameraCanMove(true);
        List<DCLBuilderInWorldEntity> allEntities = null;
        if (!isVoxelBoundMultiSelection) allEntities = builderInWorldEntityHandler.GetAllEntitiesFromCurrentScene();
        else allEntities = builderInWorldEntityHandler.GetAllVoxelsEntities();

        foreach (DCLBuilderInWorldEntity entity in allEntities)
        {
            if (entity.isVoxel && !isVoxelBoundMultiSelection) continue;
            if (entity.rootEntity.meshRootGameObject && entity.rootEntity.meshesInfo.renderers.Length > 0)
            {
                if (BuilderInWorldUtils.IsWithInSelectionBounds(entity.rootEntity.meshesInfo.mergedBounds.center, lastMousePosition, Input.mousePosition))
                {
                    builderInWorldEntityHandler.SelectEntity(entity);
                }
            }
        }

        buildModeController.SetOutlineCheckActive(true);
        outlinerController.CancelAllOutlines();
    }

    #region Voxel

    public void ActivateVoxelMode()
    {
        voxelController.SetActiveMode(true);
    }

    public void DesactivateVoxelMode()
    {
        voxelController.SetActiveMode(false);
    }

    #endregion

    public override void Activate(ParcelScene scene)
    {
        base.Activate(scene);
        gameObject.SetActive(true);
        sceneToEdit = scene;
        voxelController.SetSceneToEdit(scene);

        if(activateCamera)ActivateCamera(scene);

        if (gizmoManager.GetSelectedGizmo() == DCL.Components.DCLGizmos.Gizmo.NONE)
            gizmoManager.SetGizmoType("MOVE");
        mouseCatcher.enabled = false;
        Environment.i.world.sceneController.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();

        RenderSettings.fog = false;
        gizmoManager.HideGizmo();
        editionGO.transform.SetParent(null);
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
        gizmoManager.SetAllGizmosInPosition(cameraPosition);
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

        if(!createdEntity.isFloor)
            isPlacingNewObject = true;

        gizmoManager.HideGizmo();
        if (createdEntity.isVoxel)
        {
            createdEntity.rootEntity.gameObject.tag = BuilderInWorldSettings.VOXEL_TAG;
            voxelController.SetVoxelSelected(createdEntity);
            ActivateVoxelMode();
        }
    }

    public override Vector3 GetCreatedEntityPoint()
    {
        return GetFloorPointAtMouse(Input.mousePosition);
    }

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
    }

    public override void EntityDeselected(DCLBuilderInWorldEntity entityDeselected)
    {
        base.EntityDeselected(entityDeselected);
        if (selectedEntities.Count <= 0)
            gizmoManager.HideGizmo();

        if (isPlacingNewObject && !entityDeselected.HasShape())
            builderInWorldEntityHandler.DeleteEntity(entityDeselected);

        isPlacingNewObject = false;
        DesactivateVoxelMode();
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

    public void LookAtEntity(DecentralandEntity entity)
    {
        Vector3 pointToLook = entity.gameObject.transform.position;
        if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0)
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

    public void TranslateMode()
    {
        if (isModeActive)
        {
            gizmoManager.SetGizmoType("MOVE");
            if (selectedEntities.Count > 0)
                ShowGizmos();
            else
                gizmoManager.HideGizmo();
        }
    }

    public void RotateMode()
    {
        if (isModeActive)
        {
            gizmoManager.SetGizmoType("ROTATE");
            if (selectedEntities.Count > 0)
                ShowGizmos();
            else
                gizmoManager.HideGizmo();
        }
    }

    public void ScaleMode()
    {
        if (isModeActive)
        {
            gizmoManager.SetGizmoType("SCALE");
            if (selectedEntities.Count > 0)
                ShowGizmos();
            else
                gizmoManager.HideGizmo();
        }
    }

    public void FocusGameObject(List<DCLBuilderInWorldEntity> entitiesToFocus)
    {
        freeCameraController.FocusOnEntities(entitiesToFocus);
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
            case "MOVE":

                ActionFinish(BuildInWorldCompleteAction.ActionType.MOVE);
                break;
            case "ROTATE":

                ActionFinish(BuildInWorldCompleteAction.ActionType.ROTATE);
                break;
            case "SCALE":
                ActionFinish(BuildInWorldCompleteAction.ActionType.SCALE);
                break;
        }
    }

    void ShowGizmos()
    {
        gizmoManager.ShowGizmo();
    }

    void SetLookAtObject(ParcelScene parcelScene)
    {
        Vector3 middlePoint = CalculatePointToLookAt(parcelScene);

        lookAtT.position = middlePoint;
    }

    Vector3 CalculatePointToLookAt(ParcelScene parcelScene)
    {
        Vector3 position;

        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;

        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (Vector2Int vector in parcelScene.sceneData.parcels)
        {
            totalX += vector.x;
            totalZ += vector.y;
            if (vector.x < minX) minX = vector.x;
            if (vector.y < minY) minY = vector.y;
            if (vector.x > maxX) maxX = vector.x;
            if (vector.y > maxY) maxY = vector.y;
        }
        float centerX = totalX / parcelScene.sceneData.parcels.Length;
        float centerZ = totalZ / parcelScene.sceneData.parcels.Length;

        position.x = centerX;
        position.y = totalY;
        position.z = centerZ;

        position = Environment.i.world.state.ConvertScenePositionToUnityPosition(parcelScene);

        position.x += ParcelSettings.PARCEL_SIZE / 2 ;
        position.z += ParcelSettings.PARCEL_SIZE / 2 ;

        return position;
    }

    void SetEditObjectAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, groundLayer))
        {
            editionGO.transform.position = hit.point;
            if (selectedEntities.Count > 0 && selectedEntities[0].isNFT) editionGO.transform.position += Vector3.up * 2f;
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