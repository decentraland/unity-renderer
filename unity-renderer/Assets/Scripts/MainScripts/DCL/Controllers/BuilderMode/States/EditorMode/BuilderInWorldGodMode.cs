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

    bool isPlacingNewObject = false,
        mousePressed = false,
        isMakingSquareMultiSelection = false,
        isTypeOfBoundSelectionSelected = false,
        isVoxelBoundMultiSelection = false,
        squareMultiSelectionButtonPressed = false;

    Vector3 lastMousePosition;

    const float RAYCAST_MAX_DISTANCE = 10000f;

    private void Start()
    {
        DCLBuilderGizmoManager.OnGizmoTransformObjectEnd += OnGizmosTransformEnd;
        DCLBuilderGizmoManager.OnGizmoTransformObjectStart += OnGizmosTransformStart;

        builderInputWrapper.OnMouseDown += MouseDown;
        builderInputWrapper.OnMouseUp += MouseUp;


        focusOnSelectedEntitiesInputAction.OnTriggered += (o) => FocusOnSelectedEntitiesInput();

        squareMultiSelectionInputAction.OnStarted += (o) => squareMultiSelectionButtonPressed = true;
        squareMultiSelectionInputAction.OnFinished += (o) => squareMultiSelectionButtonPressed = false;
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
        else if (isMakingSquareMultiSelection)
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
        if (mousePressed && isMakingSquareMultiSelection)
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

        HUDController.i.buildModeHud.OnTranslateSelectedAction += TranslateMode;
        HUDController.i.buildModeHud.OnRotateSelectedAction += RotateMode;
        HUDController.i.buildModeHud.OnScaleSelectedAction += ScaleMode;
    }

    private void MouseUp(int buttonID, Vector3 position)
    {
        if (mousePressed && buttonID == 0)
        {
            if (isMakingSquareMultiSelection)
            {
                EndBoundMultiSelection();
            }
        }
    }

    void MouseDown(int buttonID, Vector3 position)
    {
        if (buttonID == 0)
        {
            if (squareMultiSelectionButtonPressed)
            {
                isMakingSquareMultiSelection = true;
                isTypeOfBoundSelectionSelected = false;
                isVoxelBoundMultiSelection = false;
                lastMousePosition = position;
                mousePressed = true;
                freeCameraController.SetCameraCanMove(false);
                buildModeController.SetOutlineCheckActive(false);
            }
        }
    }

    public void EndBoundMultiSelection()
    {
        isMakingSquareMultiSelection = false;
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

        SetLookAtObject();


        // NOTE(Adrian): Take into account that right now to get the relative scale of the gizmos, we set the gizmos in the player position and the camera
        Vector3 cameraPosition = DCLCharacterController.i.characterPosition.unityPosition;
        freeCameraController.SetPosition(cameraPosition + Vector3.up * distanceEagleCamera);
        //

        freeCameraController.LookAt(lookAtT);


        cameraController.SetCameraMode(CameraMode.ModeId.BuildingToolGodMode);

        gizmoManager.InitializeGizmos(Camera.main, freeCameraController.transform);
        gizmoManager.SetAllGizmosInPosition(cameraPosition);
        if (gizmoManager.GetSelectedGizmo() == DCL.Components.DCLGizmos.Gizmo.NONE)
            gizmoManager.SetGizmoType("MOVE");
        mouseCatcher.enabled = false;
        Environment.i.world.sceneController.IsolateScene(sceneToEdit);
        Utils.UnlockCursor();

        RenderSettings.fog = false;
        gizmoManager.HideGizmo();
        editionGO.transform.SetParent(null);
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
        return GetFloorPointAtMouse();
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

    void SetLookAtObject()
    {
        Vector3 middlePoint = CalculateMiddlePoint(sceneToEdit.sceneData.parcels);

        lookAtT.position = Environment.i.world.state.ConvertSceneToUnityPosition(middlePoint);
    }

    Vector3 CalculateMiddlePoint(Vector2Int[] positions)
    {
        Vector3 position;

        float totalX = 0f;
        float totalY = 0f;
        float totalZ = 0f;

        int minX = 9999;
        int minY = 9999;
        int maxX = -9999;
        int maxY = -9999;

        foreach (Vector2Int vector in positions)
        {
            totalX += vector.x;
            totalZ += vector.y;
            if (vector.x < minX) minX = vector.x;
            if (vector.y < minY) minY = vector.y;
            if (vector.x > maxX) maxX = vector.x;
            if (vector.y > maxY) maxY = vector.y;
        }

        float centerX = totalX / positions.Length;
        float centerZ = totalZ / positions.Length;

        position.x = centerX;
        position.y = totalY;
        position.z = centerZ;

        int amountParcelsX = Mathf.Abs(maxX - minX) + 1;
        int amountParcelsZ = Mathf.Abs(maxY - minY) + 1;

        position.x += ParcelSettings.PARCEL_SIZE / 2 * amountParcelsX;
        position.z += ParcelSettings.PARCEL_SIZE / 2 * amountParcelsZ;

        return position;
    }

    void SetEditObjectAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, groundLayer))
        {
            editionGO.transform.position = hit.point;
        }
    }

    Vector3 GetFloorPointAtMouse()
    {
        RaycastHit hit;
        UnityEngine.Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, RAYCAST_MAX_DISTANCE, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }
}