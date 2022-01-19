using System;
using System.Collections.Generic;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using UnityEngine;
using Object = UnityEngine.Object;

internal class EntityCollider : IShapeListener
{
    internal const string COLLIDERS_GAMEOBJECT_NAME = "ShapeColliderDisplay";

    private readonly Material colliderMaterial;
    private readonly Material entityMaterialResource;
    private readonly List<GameObject> colliders = new List<GameObject>();

    private readonly Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    private readonly List<Material> entityWitchColliderMaterials = new List<Material>();

    private bool isShowingColliders = false;

    public EntityCollider(Material colliderMaterial, Material entityWithColliderMaterialResource)
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
                SetUpMaterials(entity.meshesInfo);
                break;
            case false when isShowingColliders:
                CleanUp();
                break;
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
    }

    private void CleanCollider()
    {
        for (int i = 0; i < colliders.Count; i++)
        {
            Object.Destroy(colliders[i]);
        }
        colliders.Clear();
    }

    private void CleanMaterials()
    {
        for (int i = 0; i < entityWitchColliderMaterials.Count; i++)
        {
            if (entityWitchColliderMaterials[i] != null)
            {
                Object.Destroy(entityWitchColliderMaterials[i]);
            }
        }
        entityWitchColliderMaterials.Clear();

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
            if (entityRenderers[i] == null)
                continue;

            Material originalMaterial = entityRenderers[i].sharedMaterial;
            originalMaterials.Add(entityRenderers[i], originalMaterial);

            Material newMaterial = new Material(entityMaterialResource);
            newMaterial.mainTexture = originalMaterial.mainTexture;
            entityWitchColliderMaterials.Add(newMaterial);

            entityRenderers[i].sharedMaterial = newMaterial;
        }
    }
}