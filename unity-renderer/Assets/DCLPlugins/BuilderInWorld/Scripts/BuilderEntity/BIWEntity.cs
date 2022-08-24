using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public class BIWEntity
{
    public GameObject gameObject => rootEntity.gameObject;
    public Transform transform => rootEntity.gameObject.transform;

    public IDCLEntity rootEntity { protected set; get; }
    public string entityUniqueId;

    public event Action<BIWEntity> OnShapeFinishLoading;
    public event Action<BIWEntity> OnStatusUpdate;
    public event Action<BIWEntity> OnDelete;
    public event Action<BIWEntity> OnErrorStatusChange;

    public bool isDeleted { get; private set; }

    public bool isLocked
    {
        get { return GetIsLockedValue(); }
        set
        {
            SetIsLockedValue(value);
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isSelectedValue = false;

    public bool isSelected
    {
        get { return isSelectedValue; }
        set
        {
            isSelectedValue = value;
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isNewValue = false;

    public bool isNew
    {
        get { return isNewValue; }
        set
        {
            isNewValue = value;
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isVisibleValue = true;

    public bool isVisible
    {
        get { return isVisibleValue; }
        set
        {
            isVisibleValue = value;

            if (rootEntity != null && rootEntity.gameObject != null)
            {
                rootEntity.gameObject.SetActive(isVisibleValue);
            }

            OnStatusUpdate?.Invoke(this);
        }
    }

    public bool isLoaded { get; internal set; } = false;
    
    public bool isVoxel { get; set; } = false;

    private CatalogItem associatedCatalogItem;

    public bool isFloor { get; set; } = false;
    public bool isNFT { get; private set; } = false;

    private bool isShapeComponentSet = false;

    private Animation[] meshAnimations;

    private Vector3 currentRotation;
    private Transform originalParent;

    private Material[] originalMaterials;

    private Material editMaterial;

    private Dictionary<string, List<GameObject>> collidersGameObjectDictionary = new Dictionary<string, List<GameObject>>();

    private Vector3 lastPositionReported;
    private Vector3 lastScaleReported;
    private Quaternion lastRotationReported;

    #region Error Handler definition

    public bool hasError  { get; private set; } = false;

    public bool hasMissingCatalogItemError { get; private set; } = false;
    public bool isInsideBoundariesError { get; private set; } = false;

    #endregion

    public void Initialize(IDCLEntity entity, Material editMaterial)
    {
        rootEntity = entity;
        rootEntity.OnShapeUpdated += OnShapeUpdate;
        rootEntity.OnNameChange += OnNameUpdate;

        this.editMaterial = editMaterial;
        isVoxel = false;


        entityUniqueId = rootEntity.scene.sceneData.id + rootEntity.entityId;
        if (rootEntity.gameObject != null)
            isVisible = rootEntity.gameObject.activeSelf;

        isLoaded = false;
        isShapeComponentSet = false;
        InitRotation();
        
        isFloor = IsEntityAFloor();
        isNFT = IsEntityNFT();

        if (rootEntity.meshRootGameObject && rootEntity.meshesInfo.renderers.Length > 0)
        {
            ShapeInit();
        }
    }

    public CatalogItem GetCatalogItemAssociated()
    {
        if (associatedCatalogItem != null)
            return associatedCatalogItem;

        if (rootEntity == null)
            return null;

        //We get the catalog reference  
        IAssetCatalogReferenceHolder catalogHolder = rootEntity.scene.componentsManagerLegacy.TryGetComponent<IAssetCatalogReferenceHolder>(rootEntity);
        if (catalogHolder == null)
            return null;

        //we get the assetId to search in the catalog for the item
        string assetId = catalogHolder.GetAssetId();

        //We try to get the item from the main catalog that is usable right now
        if (!string.IsNullOrEmpty(assetId) && DataStore.i.builderInWorld.catalogItemDict.TryGetValue(assetId, out associatedCatalogItem))
            return associatedCatalogItem;

        //If the item doesn't exist in the catalog, we fallback to the catalog of the scene 
        if (!string.IsNullOrEmpty(assetId) && DataStore.i.builderInWorld.currentSceneCatalogItemDict.TryGetValue(assetId, out associatedCatalogItem))
            return associatedCatalogItem;

        //Error 404: Item not found, we show a pink box to represent the item
        return null;
    }

    public bool HasShape() { return isShapeComponentSet; }

    public bool HasMovedSinceLastReport() { return Vector3.Distance(lastPositionReported, rootEntity.gameObject.transform.position) >= BIWSettings.ENTITY_POSITION_REPORTING_THRESHOLD; }

    public bool HasScaledSinceLastReport() { return Math.Abs(lastScaleReported.magnitude - rootEntity.gameObject.transform.lossyScale.magnitude) >= BIWSettings.ENTITY_SCALE_REPORTING_THRESHOLD; }

    public bool HasRotatedSinceLastReport() { return Quaternion.Angle(lastRotationReported, rootEntity.gameObject.transform.rotation) >= BIWSettings.ENTITY_ROTATION_REPORTING_THRESHOLD; }

    public void PositionReported() { lastPositionReported = rootEntity.gameObject.transform.position; }

    public void ScaleReported() { lastScaleReported = rootEntity.gameObject.transform.lossyScale; }

    public void RotationReported() { lastRotationReported = rootEntity.gameObject.transform.rotation; }

    #region Error Handling

    public void CheckErrors()
    {
        bool isCurrentlyWithError = false;

        //If the entity doesn't have a catalog item associated, we can be sure that the item is deleted
        if (GetCatalogItemAssociated() == null)
        {
            hasMissingCatalogItemError = true;
            isCurrentlyWithError = true;
        }

        //If entity is not inside boundaries it has an error
        if (isInsideBoundariesError)
            isCurrentlyWithError = true;

        if (isCurrentlyWithError != hasError)
        {
            hasError = isCurrentlyWithError;
            OnErrorStatusChange?.Invoke(this);
        }
    }

    public void SetEntityBoundariesError(bool isInsideBoundaries)
    {
        isInsideBoundariesError = !isInsideBoundaries;
        CheckErrors();
    }

    #endregion

    public void Select()
    {
        isSelected = true;
        originalParent = rootEntity.gameObject.transform.parent;
        SetEditMaterial();
        lastPositionReported = rootEntity.gameObject.transform.position;
        lastScaleReported = rootEntity.gameObject.transform.lossyScale;
        lastRotationReported = rootEntity.gameObject.transform.rotation;
    }

    public void Deselect()
    {
        if (!isSelected)
            return;

        isNew = false;
        isSelected = false;
        if (rootEntity.gameObject != null)
            rootEntity.gameObject.transform.SetParent(originalParent);

        SetOriginalMaterials();
    }

    public void ToggleShowStatus()
    {
        rootEntity.gameObject.SetActive(!rootEntity.gameObject.activeSelf);
        isVisible = rootEntity.gameObject.activeSelf;
        OnStatusUpdate?.Invoke(this);
    }

    public void ToggleLockStatus() { isLocked = !isLocked; }

    public void ShapeLoadFinish(ISharedComponent component) { OnShapeFinishLoading?.Invoke(this); }

    public void Delete()
    {
        Deselect();
        Dispose();
        isDeleted = true;
        OnDelete?.Invoke(this);
    }

    public void Dispose()
    {
        if (rootEntity != null)
        {
            rootEntity.OnShapeUpdated -= OnShapeUpdate;
            rootEntity.OnNameChange -= OnNameUpdate;

            if (isNFT)
            {
                if (rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.NFT_SHAPE, out ISharedComponent component))
                {
                    BIWNFTController.i.StopUsingNFT(((NFTShape.Model) component.GetModel()).assetId);
                }
            }

            DCL.Environment.i.world.sceneBoundsChecker?.RunEntityEvaluation(rootEntity);
            DCL.Environment.i.world.sceneBoundsChecker?.RemoveEntity(rootEntity, true, true);
        }

        DestroyColliders();

        associatedCatalogItem = null;
    }

    public void DestroyColliders()
    {
        foreach (List<GameObject> entityColliderGameObject in collidersGameObjectDictionary.Values)
        {
            for (int i = 0; i < entityColliderGameObject.Count; i++)
            {
                GameObject.Destroy(entityColliderGameObject[i]);
            }
        }

        collidersGameObjectDictionary.Clear();
    }

    #region Components

    #region Transfrom

    public void AddRotation(Vector3 newRotation) { currentRotation += newRotation; }

    public void SetRotation(Vector3 newRotation) { currentRotation = newRotation; }

    public Vector3 GetEulerRotation() { return currentRotation; }

    public void InitRotation()
    {
        //TODO : We need to implement the initial rotation from the transform component instead of getting the current rotation
        if (rootEntity.gameObject != null)
            currentRotation = rootEntity.gameObject.transform.eulerAngles;
    }

    #endregion

    #region SmartItem

    public bool HasSmartItemComponent()
    {
        if (rootEntity == null)
            return false;

        return rootEntity.scene.componentsManagerLegacy.HasComponent(rootEntity, CLASS_ID_COMPONENT.SMART_ITEM);
    }

    public bool HasSmartItemActions() { return GetCatalogItemAssociated().HasActions(); }

    public SmartItemParameter[] GetSmartItemParameters() { return GetCatalogItemAssociated().parameters; }

    public SmartItemAction[] GetSmartItemActions() { return GetCatalogItemAssociated().actions; }

    #endregion

    #region Locked

    public bool GetIsLockedValue()
    {
        if (rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.LOCKED_ON_EDIT, out ISharedComponent component))
        {
            return ((DCLLockedOnEdit.Model) component.GetModel()).isLocked;
        }
 
        return isFloor;
    }

    public void SetIsLockedValue(bool isLocked)
    {
        if (rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.LOCKED_ON_EDIT, out ISharedComponent component))
        {
            ((DCLLockedOnEdit) component).SetIsLocked(isLocked);
        }
        else
        {
            DCLLockedOnEdit.Model model = new DCLLockedOnEdit.Model();
            model.isLocked = isLocked;
            
            EntityComponentsUtils.AddLockedOnEditComponent(rootEntity.scene , rootEntity, model, Guid.NewGuid().ToString());
        }
    }

    #endregion

    #region DescriptiveName

    public void SetDescriptiveName(string newName)
    {
        if (rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.NAME, out ISharedComponent nameComponent))
        {
            ((DCLName) nameComponent).SetNewName(newName);
        }
        else
        {
            DCLName.Model model = new DCLName.Model();
            model.value = newName;
            EntityComponentsUtils.AddNameComponent(rootEntity.scene , rootEntity,model, Guid.NewGuid().ToString());
        }

        OnStatusUpdate?.Invoke(this);
    }

    public string GetDescriptiveName()
    {
        if (rootEntity != null && rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.NAME, out ISharedComponent nameComponent))
        {
            return ((DCLName.Model) nameComponent.GetModel()).value;
        }

        return "";
    }

    #endregion

    #endregion

    public void ResetTransfrom()
    {
        currentRotation = Vector3.zero;
        rootEntity.gameObject.transform.eulerAngles = currentRotation;
        rootEntity.gameObject.transform.localScale = Vector3.one;
        OnStatusUpdate?.Invoke(this);
    }

    void ShapeInit()
    {
        isShapeComponentSet = true;

        CreateCollidersForEntity(rootEntity);

        if (isFloor)
            isLocked = true;

        if (IsEntityAVoxel())
            SetEntityAsVoxel();

        HandleAnimation();

        if (isNFT)
        {
            if (rootEntity.scene.componentsManagerLegacy.TryGetSharedComponent(rootEntity, CLASS_ID.NFT_SHAPE, out ISharedComponent component))
            {
                BIWNFTController.i.UseNFT(((NFTShape.Model) component.GetModel()).assetId);
            }
        }

        SaveOriginalMaterial();

        DCL.Environment.i.world.sceneBoundsChecker.AddEntityToBeChecked(rootEntity, true, true);
        SetEntityBoundariesError(DCL.Environment.i.world.sceneBoundsChecker.IsEntityMeshInsideSceneBoundaries(rootEntity));

        isLoaded = true;
    }

    private void HandleAnimation()
    {
        // We don't want animation to be running on editor
        meshAnimations = rootEntity.gameObject.GetComponentsInChildren<Animation>();
        if (HasSmartItemComponent())
        {
            DefaultAnimationStop();
        }
        else
        {
            DefaultAnimationSample(0);
        }
    }

    private void DefaultAnimationStop()
    {
        if (meshAnimations == null)
            return;

        for (int i = 0; i < meshAnimations.Length; i++)
        {
            meshAnimations[i].Stop();
        }
    }

    private void DefaultAnimationSample(float time)
    {
        if (meshAnimations == null || meshAnimations.Length == 0)
            return;

        for (int i = 0; i < meshAnimations.Length; i++)
        {
            meshAnimations[i].Stop();
            meshAnimations[i].clip?.SampleAnimation(meshAnimations[i].gameObject, time);
        }
    }

    void SetOriginalMaterials()
    {
        if (rootEntity.meshesInfo.renderers == null)
            return;
        if (isNFT)
            return;

        int matCont = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
        {
            if (renderer == null)
                continue;
            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (isNFT && matCont == 0)
                {
                    materials[i] = renderer.sharedMaterials[i];
                    matCont++;
                    continue;
                }

                materials[i] = originalMaterials[matCont];
                matCont++;
            }

            renderer.sharedMaterials = materials;
        }
    }

    void SetEntityAsVoxel()
    {
        isVoxel = true;
        rootEntity.gameObject.tag = BIWSettings.VOXEL_TAG;
    }

    void SaveOriginalMaterial()
    {
        if (rootEntity.meshesInfo == null ||
            rootEntity.meshesInfo.renderers == null ||
            rootEntity.meshesInfo.renderers.Length == 0)
            return;

        if (isNFT)
            return;

        int totalMaterials = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
        {
            if (renderer == null)
                continue;
            totalMaterials += renderer.sharedMaterials.Length;
        }

        if (!isNFT || (isNFT && originalMaterials == null))
            originalMaterials = new Material[totalMaterials];

        int matCont = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
        {
            if (renderer == null)
                continue;

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (isNFT && matCont == 0)
                {
                    matCont++;
                    continue;
                }

                originalMaterials[matCont] = renderer.sharedMaterials[i];
                matCont++;
            }
        }
    }

    void SetEditMaterial()
    {
        if (rootEntity.meshesInfo == null ||
            rootEntity.meshesInfo.renderers == null ||
            rootEntity.meshesInfo.renderers.Length < 1)
            return;

        if (isNFT)
            return;

        int matCont = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
        {
            if (renderer == null)
                continue;

            Material[] materials = new Material[renderer.sharedMaterials.Length];

            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (isNFT && matCont == 0)
                {
                    materials[i] = renderer.sharedMaterials[i];
                    matCont++;
                    continue;
                }

                materials[i] = editMaterial;
                matCont++;
            }

            renderer.sharedMaterials = materials;
        }
    }

    void OnNameUpdate(object model) { OnStatusUpdate?.Invoke(this); }

    void OnShapeUpdate(IDCLEntity entity)
    {
        ShapeInit();

        if (isSelected)
            SetEditMaterial();
    }

    void CreateCollidersForEntity(IDCLEntity entity)
    {
        MeshesInfo meshInfo = entity.meshesInfo;
        if (meshInfo == null ||
            meshInfo.currentShape == null ||
            !meshInfo.currentShape.IsVisible())
            return;

        if (collidersGameObjectDictionary.ContainsKey(entity.scene.sceneData.id + entity.entityId) && !isNFT)
            return;

        if (entity.children.Count > 0)
        {
            using (var iterator = entity.children.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    CreateCollidersForEntity(iterator.Current.Value);
                }
            }
        }

        //Note: When we are duplicating the GLTF and NFT component, their colliders are duplicated too
        //So we eliminate any previous collider to ensure that only 1 collider remain active
        Transform[] children = rootEntity.gameObject.transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.layer ==  BIWSettings.COLLIDER_SELECTION_LAYER)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        List<GameObject> colliderList = new List<GameObject>();

        for (int i = 0; i < meshInfo.renderers.Length; i++)
        {
            if (meshInfo.renderers[i] == null)
                continue;
            GameObject entityColliderChildren = new GameObject(entity.entityId.ToString());
            entityColliderChildren.layer = BIWSettings.COLLIDER_SELECTION_LAYER_INDEX;

            Transform t = entityColliderChildren.transform;
            t.SetParent(meshInfo.renderers[i].transform);
            t.ResetLocalTRS();

            var meshCollider = entityColliderChildren.AddComponent<MeshCollider>();

            if (meshInfo.renderers[i] is SkinnedMeshRenderer skr)
            {
                Mesh meshColliderForSkinnedMesh = new Mesh();
                skr.BakeMesh(meshColliderForSkinnedMesh);
                meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                t.localScale = new Vector3(1 / meshInfo.renderers[i].gameObject.transform.lossyScale.x, 1 / meshInfo.renderers[i].gameObject.transform.lossyScale.y, 1 / meshInfo.renderers[i].gameObject.transform.lossyScale.z);
            }
            else
            {
                meshCollider.sharedMesh = meshInfo.renderers[i].GetComponent<MeshFilter>().sharedMesh;
            }

            meshCollider.enabled = true;
            colliderList.Add(entityColliderChildren);

            if (isNFT)
            {
                if (collidersGameObjectDictionary.ContainsKey(entity.scene.sceneData.id + entity.entityId))
                    collidersGameObjectDictionary.Remove(entity.scene.sceneData.id + entity.entityId);

                collidersGameObjectDictionary.Add(entity.scene.sceneData.id + entity.entityId, colliderList);

                colliderList = new List<GameObject>();
            }
        }

        if (!isNFT)
            collidersGameObjectDictionary.Add(entity.scene.sceneData.id + entity.entityId, colliderList);
    }

    public bool IsEntityNFT()
    {
        return rootEntity.scene.componentsManagerLegacy.HasSharedComponent(rootEntity, CLASS_ID.NFT_SHAPE);
    }

    private bool IsEntityAFloor() { return GetCatalogItemAssociated()?.category == BIWSettings.FLOOR_CATEGORY; }

    private bool IsEntityAVoxel()
    {
        if (rootEntity.meshesInfo?.currentShape == null)
            return false;
        if (rootEntity.meshesInfo.renderers?.Length <= 0)
            return false;
        if (rootEntity.meshesInfo.mergedBounds.size != Vector3.one)
            return false;
        return true;
    }
}