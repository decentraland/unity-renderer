using System;
using UnityEngine;
using DCL.Models;
using DCL.Components;

namespace Builder
{
    public class DCLBuilderEntity : MonoBehaviour
    {
        public DecentralandEntity rootEntity { private set; get; }
        public bool hasGizmoComponent
        {
            get
            {
                if (rootEntity != null)
                {
                    return rootEntity.components.ContainsKey(CLASS_ID_COMPONENT.GIZMOS);
                }
                else
                {
                    return false;
                }
            }
        }

        private MeshCollider[] meshColliders;

        public void SetEntity(DecentralandEntity entity)
        {
            rootEntity = entity;

            entity.OnShapeUpdated -= ProcessEntityShape;
            entity.OnShapeUpdated += ProcessEntityShape;

            if (entity.meshesInfo.currentShape != null)
            {
                ProcessEntityShape(entity);
            }
        }

        private void OnDestroy()
        {
            rootEntity.OnShapeUpdated -= ProcessEntityShape;
        }

        private void ProcessEntityShape(DecentralandEntity entity)
        {
            if (entity.meshRootGameObject && entity.meshesInfo.renderers.Length > 0 && hasGizmoComponent)
            {
                CreateColliders(entity.meshesInfo);
            }
        }

        private void CreateColliders(DecentralandEntity.MeshesInfo meshInfo)
        {
            gameObject.layer = LayerMask.NameToLayer(OnPointerEventColliders.COLLIDER_LAYER);

            meshColliders = new MeshCollider[meshInfo.renderers.Length];
            for (int i = 0; i < meshInfo.renderers.Length; i++)
            {
                meshColliders[i] = gameObject.AddComponent<MeshCollider>();
                SetupMeshCollider(meshColliders[i], meshInfo.renderers[i]);
            }
        }

        private void SetupMeshCollider(MeshCollider meshCollider, Renderer renderer)
        {
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;
        }
    }
}