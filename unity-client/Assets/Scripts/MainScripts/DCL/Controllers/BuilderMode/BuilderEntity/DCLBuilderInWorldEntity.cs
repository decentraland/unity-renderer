using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DCLBuilderInWorldEntity : EditableEntity
{
    public string entityUniqueId;

    public event System.Action<DCLBuilderInWorldEntity> OnShapeFinishLoading;
    public event System.Action<DCLBuilderInWorldEntity> OnStatusUpdate;
    public event System.Action<DCLBuilderInWorldEntity> OnDelete;

    private bool isLockedValue = false;

    public bool IsLocked
    {
        get { return GetIsLockedValue(); }
        set
        {
            SetIsLockedValue(value);
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isSelectedValue = false;

    public bool IsSelected
    {
        get { return isSelectedValue; }
        set
        {
            isSelectedValue = value;
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isNewValue = false;

    public bool IsNew
    {
        get { return isNewValue; }
        set
        {
            isNewValue = value;
            OnStatusUpdate?.Invoke(this);
        }
    }

    private bool isVisibleValue = true;

    public bool IsVisible
    {
        get { return isVisibleValue; }
        set
        {
            isVisibleValue = value;
            OnStatusUpdate?.Invoke(this);
        }
    }

    public bool isVoxel { get; set; } = false;

    CatalogItem associatedCatalogItem;

    public bool isFloor { get; set; } = false;
    public bool isNFT { get; set; } = false;

    private bool isShapeComponentSet = false;

    private Animation[] meshAnimations;

    private Vector3 currentRotation;
    Transform originalParent;

    Material[] originalMaterials;

    Material editMaterial;

    Dictionary<string, List<GameObject>> collidersGameObjectDictionary = new Dictionary<string, List<GameObject>>();

    public void Init(DecentralandEntity entity, Material editMaterial)
    {
        rootEntity = entity;
        rootEntity.OnShapeUpdated += OnShapeUpdate;
        rootEntity.OnNameChange += OnNameUpdate;

        this.editMaterial = editMaterial;
        isVoxel = false;


        entityUniqueId = rootEntity.scene.sceneData.id + rootEntity.entityId;
        IsVisible = rootEntity.gameObject.activeSelf;

        isShapeComponentSet = false;
        InitRotation();

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

        IAssetCatalogReferenceHolder catalogHolder = rootEntity.TryGetComponent<IAssetCatalogReferenceHolder>();
        if (catalogHolder == null)
            return null;

        string assetId = catalogHolder.GetAssetId();

        if (!string.IsNullOrEmpty(assetId) && DataStore.i.builderInWorld.catalogItemDict.TryGetValue(assetId, out associatedCatalogItem))
            return associatedCatalogItem;

        return null;
    }

    public bool HasShape() { return isShapeComponentSet; }

    public void Select()
    {
        IsSelected = true;
        originalParent = rootEntity.gameObject.transform.parent;
        SetEditMaterial();
    }

    public void Deselect()
    {
        if (!IsSelected)
            return;

        IsSelected = false;
        if (rootEntity.gameObject != null)
            rootEntity.gameObject.transform.SetParent(originalParent);

        SetOriginalMaterials();
    }

    public void ToggleShowStatus()
    {
        rootEntity.gameObject.SetActive(!gameObject.activeSelf);
        IsVisible = gameObject.activeSelf;
        OnStatusUpdate?.Invoke(this);
    }

    public void ToggleLockStatus() { IsLocked = !IsLocked; }

    public void ShapeLoadFinish(ISharedComponent component) { OnShapeFinishLoading?.Invoke(this); }

    public void Delete()
    {
        rootEntity.OnShapeUpdated -= OnShapeUpdate;
        rootEntity.OnNameChange -= OnNameUpdate;

        Deselect();
        DestroyColliders();

        if (isNFT)
        {
            foreach (KeyValuePair<Type, ISharedComponent> kvp in rootEntity.GetSharedComponents())
            {
                if (kvp.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
                {
                    BuilderInWorldNFTController.i.StopUsingNFT(((NFTShape.Model) kvp.Value.GetModel()).assetId);
                    break;
                }
            }
        }

        associatedCatalogItem = null;
        DCL.Environment.i.world.sceneBoundsChecker.EvaluateEntityPosition(rootEntity);
        DCL.Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(rootEntity);
        OnDelete?.Invoke(this);
    }

    public void DestroyColliders()
    {
        foreach (List<GameObject> entityColliderGameObject in collidersGameObjectDictionary.Values)
        {
            for (int i = entityColliderGameObject.Count - 1; i > 0; i--)
            {
                Destroy(entityColliderGameObject[i]);
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
        currentRotation = rootEntity.gameObject.transform.eulerAngles;
    }

    #endregion

    #region SmartItem

    public bool HasSmartItemComponent()
    {
        if (rootEntity == null)
            return false;

        return rootEntity.components.ContainsKey(CLASS_ID_COMPONENT.SMART_ITEM);
    }

    public bool HasSmartItemActions() { return GetCatalogItemAssociated().HasActions(); }

    public SmartItemParameter[] GetSmartItemParameters() { return GetCatalogItemAssociated().parameters; }

    public SmartItemAction[] GetSmartItemActions() { return GetCatalogItemAssociated().actions; }

    #endregion

    #region Locked

    public bool GetIsLockedValue()
    {
        foreach (KeyValuePair<Type, ISharedComponent> kvp in rootEntity.GetSharedComponents())
        {
            if (kvp.Value.GetClassId() == (int) CLASS_ID.LOCKED_ON_EDIT)
            {
                return ((DCLLockedOnEdit.Model) kvp.Value.GetModel()).isLocked;
            }
        }

        return isFloor;
    }

    public void SetIsLockedValue(bool isLocked)
    {
        bool foundComponent = false;

        foreach (KeyValuePair<Type, ISharedComponent> kvp in rootEntity.GetSharedComponents())
        {
            if (kvp.Value.GetClassId() == (int) CLASS_ID.LOCKED_ON_EDIT)
            {
                ((DCLLockedOnEdit) kvp.Value).SetIsLocked(isLocked);
                foundComponent = true;
            }
        }

        if (!foundComponent)
        {
            ParcelScene scene = rootEntity.scene as ParcelScene;
            DCLLockedOnEdit entityLocked = (DCLLockedOnEdit) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
            entityLocked.SetIsLocked(isLocked);
            scene.SharedComponentAttach(rootEntity.entityId, entityLocked.id);
        }
    }

    #endregion

    #region DescriptiveName

    public void SetDescriptiveName(string newName)
    {
        if (rootEntity.TryGetSharedComponent(CLASS_ID.NAME, out ISharedComponent nameComponent))
        {
            ((DCLName) nameComponent).SetNewName(newName);
        }
        else
        {
            ParcelScene scene = rootEntity.scene as ParcelScene;
            DCLName name = (DCLName) scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
            name.SetNewName(newName);
            scene.SharedComponentAttach(rootEntity.entityId, name.id);
        }

        OnStatusUpdate?.Invoke(this);
    }

    public string GetDescriptiveName()
    {
        if (rootEntity != null && rootEntity.TryGetSharedComponent(CLASS_ID.NAME, out ISharedComponent nameComponent))
        {
            return ((DCLName.Model) nameComponent.GetModel()).value;
        }

        return "";
    }

    #endregion

    #endregion

    void ShapeInit()
    {
        isShapeComponentSet = true;

        isFloor = IsEntityAFloor();
        isNFT = IsEntityNFT();

        CreateCollidersForEntity(rootEntity);

        if (isFloor)
            IsLocked = true;

        if (IsEntityAVoxel())
            SetEntityAsVoxel();

        HandleAnimation();

        if (isNFT)
        {
            foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
            {
                if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
                {
                    BuilderInWorldNFTController.i.UseNFT(((NFTShape.Model) keyValuePairBaseDisposable.Value.GetModel()).assetId);
                    break;
                }
            }
        }

        SaveOriginalMaterial();

        DCL.Environment.i.world.sceneBoundsChecker.AddPersistent(rootEntity);
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
        gameObject.tag = BuilderInWorldSettings.VOXEL_TAG;
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
            totalMaterials += renderer.materials.Length;

        if (!isNFT || (isNFT && originalMaterials == null))
            originalMaterials = new Material[totalMaterials];

        int matCont = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
        {
            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
            {
                if (isNFT && matCont == 0)
                {
                    matCont++;
                    continue;
                }

                originalMaterials[matCont] = renderer.materials[i];
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

    void OnNameUpdate(DCLName.Model model) { OnStatusUpdate?.Invoke(this); }

    void OnShapeUpdate(DecentralandEntity decentralandEntity)
    {
        ShapeInit();

        if (IsSelected)
            SetEditMaterial();
    }

    void CreateCollidersForEntity(DecentralandEntity entity)
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

        List<GameObject> colliderList = new List<GameObject>();

        for (int i = 0; i < meshInfo.renderers.Length; i++)
        {
            GameObject entityColliderChildren = new GameObject(entity.entityId);
            entityColliderChildren.layer = BuilderInWorldSettings.COLLIDER_SELECTION_LAYER;

            Transform t = entityColliderChildren.transform;
            t.SetParent(meshInfo.renderers[i].transform);
            t.ResetLocalTRS();

            var meshCollider = entityColliderChildren.AddComponent<MeshCollider>();

            if (meshInfo.renderers[i] is SkinnedMeshRenderer)
            {
                Mesh meshColliderForSkinnedMesh = new Mesh();
                (meshInfo.renderers[i] as SkinnedMeshRenderer).BakeMesh(meshColliderForSkinnedMesh);
                meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                t.localScale = new Vector3(1 / meshInfo.renderers[i].gameObject.transform.lossyScale.x, 1 / meshInfo.renderers[i].gameObject.transform.lossyScale.y, 1 / meshInfo.renderers[i].gameObject.transform.lossyScale.z);
            }
            else
            {
                meshCollider.sharedMesh = meshInfo.renderers[i].GetComponent<MeshFilter>().sharedMesh;
            }

            meshCollider.enabled = meshInfo.renderers[i].enabled;
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
        foreach (KeyValuePair<Type, ISharedComponent> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
                return true;
        }

        return false;
    }

    bool IsEntityAFloor() { return GetCatalogItemAssociated()?.category == BuilderInWorldSettings.FLOOR_CATEGORY; }

    bool IsEntityAVoxel()
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