using System;
using System.Collections.Generic;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

internal class EntityStyle : IShapeListener
{
    internal const string COLLIDERS_GAMEOBJECT_NAME = "ShapeColliderDisplay";
    internal const string ENTITY_MATERIAL_NAME = "DebugMaterialForColliders";

    private readonly Material colliderMaterial;
    private readonly Material entityMaterialResource;
    private readonly List<GameObject> colliders = new List<GameObject>();

    private readonly Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    private readonly Dictionary<Renderer, Material> entityDebugMaterials = new Dictionary<Renderer, Material>();

    private bool isShowingColliders = false;
    private MaterialChangesTracker materialChangesTracker;

    public EntityStyle(Material colliderMaterial, Material entityWithColliderMaterialResource)
    {
        this.colliderMaterial = colliderMaterial;
        this.entityMaterialResource = entityWithColliderMaterialResource;
    }

    void IDisposable.Dispose()
    {
        CleanUp();
    }

    void IShapeListener.OnShapeUpdated(IDCLEntity entity)
    {
        bool entityHasColliders = entity.meshesInfo.currentShape.HasCollisions();
        switch (entityHasColliders)
        {
            case true when !isShowingColliders:
                SetUpColliders(entity.meshesInfo);
                break;
            case false when isShowingColliders:
                CleanCollider();
                break;
        }

        if (materialChangesTracker == null)
        {
            materialChangesTracker = new MaterialChangesTracker(entity.meshesInfo, originalMaterials);
            materialChangesTracker.OnRendererMaterialChanged += OnMaterialChanged;

            CleanMaterials();
            SetUpMaterials(entity.meshesInfo);
        }

        isShowingColliders = entityHasColliders;
    }

    void IShapeListener.OnShapeCleaned(IDCLEntity entity)
    {
        CleanUp();
    }

    private void CleanUp()
    {
        CleanCollider();
        CleanMaterials();

        if (materialChangesTracker != null)
        {
            materialChangesTracker.OnRendererMaterialChanged -= OnMaterialChanged;
            materialChangesTracker.Dispose();
            materialChangesTracker = null;
        }
    }

    private void CleanCollider()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i]?.SetActive(false);
            Object.Destroy(colliders[i]);
        }
        colliders.Clear();
    }

    private void CleanMaterials()
    {
        using (var iterator = entityDebugMaterials.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                Material material = iterator.Current.Value;
                if (material != null)
                {
                    Object.Destroy(material);
                }
            }
        }
        entityDebugMaterials.Clear();

        // Restore original materials
        using (var iterator = originalMaterials.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                Renderer renderer = iterator.Current.Key;
                Material material = iterator.Current.Value;
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }
        originalMaterials.Clear();
    }

    private void SetUpColliders(MeshesInfo meshesInfo)
    {
        for (int i = 0; i < meshesInfo.colliders.Count; i++)
        {
            GameObject colliderGo;
            Collider collider = meshesInfo.colliders[i];

            switch (collider)
            {
                case MeshCollider meshCollider:
                    {
                        colliderGo = new GameObject(COLLIDERS_GAMEOBJECT_NAME);
                        MeshFilter meshFilter = colliderGo.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = meshCollider.sharedMesh;

                        MeshRenderer renderer = colliderGo.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = colliderMaterial;
                        break;
                    }
                case BoxCollider _:
                    {
                        colliderGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        colliderGo.name = COLLIDERS_GAMEOBJECT_NAME;
                        MeshRenderer renderer = colliderGo.GetComponent<MeshRenderer>();
                        renderer.sharedMaterial = colliderMaterial;
                        break;
                    }
                default:
                    continue;
            }

            Transform colliderTransform = collider.gameObject.transform;

            Transform displayColliderTransform = colliderGo.transform;
            displayColliderTransform.SetParent(colliderTransform);

            displayColliderTransform.localPosition = Vector3.zero;
            displayColliderTransform.localRotation = Quaternion.identity;
            displayColliderTransform.localScale = Vector3.one;

            colliders.Add(colliderGo);
        }
    }

    private void SetUpMaterials(MeshesInfo meshesInfo)
    {
        Renderer[] entityRenderers = meshesInfo.renderers;

        if (entityRenderers == null)
            return;

        for (int i = 0; i < entityRenderers.Length; i++)
        {
            SetUpMaterials(entityRenderers[i]);
        }
    }

    private void SetUpMaterials(Renderer renderer)
    {
        if (renderer == null)
            return;

        Material originalMaterial = renderer.sharedMaterial;

        if (originalMaterial == null)
            return;

        originalMaterials.Add(renderer, originalMaterial);

        SetUpDebugMaterial(renderer, originalMaterial.mainTexture);
    }

    private void SetUpDebugMaterial(Renderer renderer, Texture originalMaterialTexture)
    {
        Material newMaterial = new Material(entityMaterialResource);
        newMaterial.mainTexture = originalMaterialTexture;
        newMaterial.name = ENTITY_MATERIAL_NAME;

        entityDebugMaterials.Add(renderer, newMaterial);

        renderer.sharedMaterial = newMaterial;
    }

    // NOTE: we need to cover the scenario where we apply the debugging material to the entity's shape
    // and the material is later changed through the sdk for a new material,
    // overriding the debug material previously applied
    private void OnMaterialChanged(Renderer renderer)
    {
        originalMaterials[renderer] = renderer.sharedMaterial;
        if (entityDebugMaterials.TryGetValue(renderer, out Material material))
        {
            renderer.sharedMaterial = material;
        }
        else
        {
            SetUpDebugMaterial(renderer, renderer.sharedMaterial.mainTexture);
        }
    }
}