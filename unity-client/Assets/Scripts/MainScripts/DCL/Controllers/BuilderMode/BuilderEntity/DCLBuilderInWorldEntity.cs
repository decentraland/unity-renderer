using DCL;
using DCL.Models;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Configuration;
using System;

public class DCLBuilderInWorldEntity : EditableEntity
{
    public string entityUniqueId;

    public string descriptiveName { get; private set; }

    public event System.Action<DCLBuilderInWorldEntity> onStatusUpdate;
    public event System.Action<DCLBuilderInWorldEntity> OnDelete;

    private bool isLockedValue = false;

    public bool IsLocked
    {
        get { return GetIsLockedValue(); }
        set
        {
            SetIsLockedValue(value);
            onStatusUpdate?.Invoke(this);
        }
    }

    private bool isSelectedValue = false;

    public bool IsSelected
    {
        get { return isSelectedValue; }
        set
        {
            isSelectedValue = value;
            onStatusUpdate?.Invoke(this);
        }
    }

    private bool isNewValue = false;

    public bool IsNew
    {
        get { return isNewValue; }
        set
        {
            isNewValue = value;
            onStatusUpdate?.Invoke(this);
        }
    }

    private bool isVisibleValue = true;

    public bool IsVisible
    {
        get { return isVisibleValue; }
        set
        {
            isVisibleValue = value;
            onStatusUpdate?.Invoke(this);
        }
    }

    public bool isVoxel { get; set; } = false;

    SceneObject associatedSceneObject;
    public bool isFloor { get; set; } = false;
    public bool isNFT { get; set; } = false;

    private bool isShapeComponentSet = false;

    Transform originalParent;

    Material[] originalMaterials;

    Material editMaterial;

    Dictionary<string, GameObject> collidersDictionary = new Dictionary<string, GameObject>();

    public void Init(DecentralandEntity entity, Material editMaterial)
    {
        rootEntity = entity;
        rootEntity.OnShapeUpdated += OnShapeUpdate;
        rootEntity.OnNameChange += OnNameUpdate;

        this.editMaterial = editMaterial;
        isVoxel = false;


        entityUniqueId = rootEntity.scene.sceneData.id + rootEntity.entityId;
        IsVisible = rootEntity.gameObject.activeSelf;

        descriptiveName = GetDescriptiveName();
        isShapeComponentSet = false;

        if (rootEntity.meshRootGameObject && rootEntity.meshesInfo.renderers.Length > 0)
        {
            ShapeInit();
        }
    }

    public SceneObject GetSceneObjectAssociated()
    {
        if (associatedSceneObject != null)
            return associatedSceneObject;

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.GLTF_SHAPE)
            {
                string assetId = ((GLTFShape)keyValuePairBaseDisposable.Value).model.assetId;
                associatedSceneObject = AssetCatalogBridge.GetSceneObjectById(assetId);
                return associatedSceneObject;
            }
        }

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.NFT_SHAPE)
            {
                //TODO: Implement NFT SceneObject and return them
            }
        }
        return null;
    }

    public bool HasShape()
    {
        return isShapeComponentSet;
    }

    public void Select()
    {
        IsSelected = true;
        originalParent = rootEntity.gameObject.transform.parent;
        SaveOriginalMaterialAndSetEditMaterials();
        DCL.Environment.i.world.sceneBoundsChecker.AddPersistent(rootEntity);
    }

    public void Deselect()
    {
        if (!IsSelected) return;

        IsSelected = false;
        if (rootEntity.gameObject != null)
            rootEntity.gameObject.transform.SetParent(originalParent);
        
        DCL.Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(rootEntity);
        SetOriginalMaterials();
    }

    public void ToggleShowStatus()
    {
        rootEntity.gameObject.SetActive(!gameObject.activeSelf);
        IsVisible = gameObject.activeSelf;
        onStatusUpdate?.Invoke(this);
    }

    public void ToggleLockStatus()
    {
        IsLocked = !IsLocked;
    }

    public void Delete()
    {
        rootEntity.OnShapeUpdated -= OnShapeUpdate;
        rootEntity.OnNameChange -= OnNameUpdate;

        Deselect();
        DestroyColliders();

        if(isNFT)
        {
            foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
            {
                if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.NFT_SHAPE)
                {
                    BuilderInWorldNFTController.i.StopUsingNFT(((NFTShape.Model)keyValuePairBaseDisposable.Value.GetModel()).assetId);
                    break;
                }
            }
        }

        DCL.Environment.i.world.sceneBoundsChecker.RemoveEntityToBeChecked(rootEntity);
        OnDelete?.Invoke(this);
    }

    public void CreateColliders()
    {
        if (rootEntity.meshRootGameObject && rootEntity.meshesInfo.renderers.Length > 0)
        {
            CreateCollidersForEntity(rootEntity);
        }
    }

    public void DestroyColliders()
    {
        foreach (GameObject entityCollider in collidersDictionary.Values)
        {
            GameObject.Destroy(entityCollider);
        }

        collidersDictionary.Clear();
    }

    #region Components

    public bool GetIsLockedValue()
    {
        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.LOCKED_ON_EDIT)
            {
                return ((DCLLockedOnEdit.Model) keyValuePairBaseDisposable.Value.GetModel()).isLocked;
            }
        }
        return isFloor;
    }

    public void SetIsLockedValue(bool isLocked)
    {
        bool foundComponent = false;

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.LOCKED_ON_EDIT)
            {
                ((DCLLockedOnEdit) keyValuePairBaseDisposable.Value).SetIsLocked(isLocked);
                foundComponent = true;
            }
        }

        if (!foundComponent)
        {
            DCLLockedOnEdit entityLocked = (DCLLockedOnEdit)rootEntity.scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.LOCKED_ON_EDIT));
            entityLocked.SetIsLocked(isLocked);
            rootEntity.scene.SharedComponentAttach(rootEntity.entityId, entityLocked.id);
        }
    }

    public void SetDescriptiveName(string newName)
    {
        bool foundComponent = false;

        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NAME)
            {
                ((DCLName) keyValuePairBaseDisposable.Value).SetNewName(newName);
                foundComponent = true;
            }
        }

        if (!foundComponent)
        {
            DCLName name = (DCLName) rootEntity.scene.SharedComponentCreate(Guid.NewGuid().ToString(), Convert.ToInt32(CLASS_ID.NAME));
            name.SetNewName(newName);
            rootEntity.scene.SharedComponentAttach(rootEntity.entityId, name.id);
        }
        onStatusUpdate?.Invoke(this);
    }

    public string GetDescriptiveName()
    {
        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NAME)
            {
                return descriptiveName = ((DCLName.Model) keyValuePairBaseDisposable.Value.GetModel()).value;
            }
        }
        return "";
    }

    #endregion

    void ShapeInit()
    {
        isShapeComponentSet = true;

        isFloor = IsEntityAFloor();
        isNFT = IsEntityNFT();

        CreateCollidersForEntity(rootEntity);

        if (isFloor) IsLocked = true;
        if (IsEntityAVoxel()) SetEntityAsVoxel();
        if(isNFT)
        {
            foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
            {
                if (keyValuePairBaseDisposable.Value.GetClassId() == (int)CLASS_ID.NFT_SHAPE)
                {
                    BuilderInWorldNFTController.i.UseNFT(((NFTShape.Model) keyValuePairBaseDisposable.Value.GetModel()).assetId);
                    break;
                }
            }
        }

        DCL.Environment.i.world.sceneBoundsChecker.AddPersistent(rootEntity);
    }

    void SetOriginalMaterials()
    {
        if (rootEntity.meshesInfo.renderers == null) return;
        if (isNFT) return;

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

    void SaveOriginalMaterialAndSetEditMaterials()
    {
        if (rootEntity.meshesInfo.renderers == null && rootEntity.meshesInfo.renderers.Length < 1) return;

        if (isNFT) return;

        int totalMaterials = 0;
        foreach (Renderer renderer in rootEntity.meshesInfo.renderers)
            totalMaterials += renderer.materials.Length;

        if(!isNFT || (isNFT && originalMaterials == null))
            originalMaterials = new Material[totalMaterials];

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

                if (renderer.materials[i] != editMaterial)
                    originalMaterials[matCont] = renderer.materials[i];

                materials[i] = editMaterial;
                matCont++;
            }

            renderer.sharedMaterials = materials;
        }
    }

    void OnNameUpdate(DCLName.Model model)
    {
        descriptiveName = model.value;
        onStatusUpdate?.Invoke(this);
    }

    void OnShapeUpdate(DecentralandEntity decentralandEntity)
    {
        if (IsSelected)
            SaveOriginalMaterialAndSetEditMaterials();

        ShapeInit();
    }

    void CreateCollidersForEntity(DecentralandEntity entity)
    {
        MeshesInfo meshInfo = entity.meshesInfo;
        if (meshInfo == null ||
            meshInfo.currentShape == null ||
            !meshInfo.currentShape.IsVisible())
            return;

        if (collidersDictionary.ContainsKey(entity.scene.sceneData.id + entity.entityId) && !isNFT) return;

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

        GameObject entityCollider = new GameObject(entity.entityId);
        entityCollider.layer = LayerMask.NameToLayer("OnBuilderPointerClick");

        for (int i = 0; i < meshInfo.renderers.Length; i++)
        {


            Transform t = entityCollider.transform;
            t.SetParent(meshInfo.renderers[i].transform);
            t.ResetLocalTRS();

            var meshCollider = entityCollider.AddComponent<MeshCollider>();

            if (meshInfo.renderers[i] is SkinnedMeshRenderer)
            {
                Mesh meshColliderForSkinnedMesh = new Mesh();
                (meshInfo.renderers[i] as SkinnedMeshRenderer).BakeMesh(meshColliderForSkinnedMesh);
                meshCollider.sharedMesh = meshColliderForSkinnedMesh;
                t.localScale = new Vector3(1 / entity.gameObject.transform.lossyScale.x, 1 / entity.gameObject.transform.lossyScale.y, 1 / entity.gameObject.transform.lossyScale.z);
            }
            else
            {
                meshCollider.sharedMesh = meshInfo.renderers[i].GetComponent<MeshFilter>().sharedMesh;
            }

            meshCollider.enabled = meshInfo.renderers[i].enabled;

            if (isNFT)
            {
                if (collidersDictionary.ContainsKey(entity.scene.sceneData.id + entity.entityId))
                    collidersDictionary.Remove(entity.scene.sceneData.id + entity.entityId);

                collidersDictionary.Add(entity.scene.sceneData.id + entity.entityId, entityCollider);

                entityCollider = new GameObject(entity.entityId);
                entityCollider.layer = LayerMask.NameToLayer("OnBuilderPointerClick");
            }
        }

        if(!isNFT) collidersDictionary.Add(entity.scene.sceneData.id + entity.entityId, entityCollider);
    }

    bool IsEntityNFT()
    {
        foreach (KeyValuePair<Type, BaseDisposable> keyValuePairBaseDisposable in rootEntity.GetSharedComponents())
        {
            if (keyValuePairBaseDisposable.Value.GetClassId() == (int) CLASS_ID.NFT_SHAPE)
                return true;
        }
        return false;
    }

    bool IsEntityAFloor()
    {
        if (rootEntity.meshesInfo?.currentShape == null)
            return false;
        if (rootEntity.meshesInfo.renderers?.Length <= 0)
            return false;
        if (rootEntity.meshesInfo.mergedBounds.size.y >= 0.05f)
            return false;
        if (rootEntity.gameObject.transform.position.y >= 0.05f)
            return false;

        if (Mathf.Abs(rootEntity.meshesInfo.mergedBounds.extents.x - 8) > 0.001f)
            return false;
        if (Mathf.Abs(rootEntity.meshesInfo.mergedBounds.extents.z - 8) > 0.001f)
            return false;

        return true;
    }

    bool IsEntityAVoxel()
    {
        if (rootEntity.meshesInfo?.currentShape == null) return false;
        if (rootEntity.meshesInfo.renderers?.Length <= 0) return false;
        if (rootEntity.meshesInfo.mergedBounds.size != Vector3.one) return false;
        return true;
    }
}