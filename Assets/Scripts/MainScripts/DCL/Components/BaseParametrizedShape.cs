using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseParametrizedShape<T> : BaseShape where T : new()
    {
        public T model = new T();

        public abstract Mesh GenerateGeometry();
        public abstract bool HasCollisions();

        private Mesh currentMesh = null;

        public BaseParametrizedShape(ParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            var meshFilter = Helpers.Utils.GetOrCreateComponent<MeshFilter>(entity.meshGameObject);
            var meshRenderer = entity.meshGameObject.GetComponent<MeshRenderer>();

            if (meshRenderer == null)
            {
                meshRenderer = entity.meshGameObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Default");
            }

            meshFilter.sharedMesh = currentMesh;

            if (entity.OnShapeUpdated != null)
                entity.OnShapeUpdated.Invoke(entity);

            ConfigureCollision(entity, HasCollisions());
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            var meshFilter = entity.meshGameObject.GetComponent<MeshFilter>();
            if (meshFilter)
            {
                meshFilter.sharedMesh = null;
                Utils.SafeDestroy(meshFilter);
            }
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            JsonUtility.FromJsonOverwrite(newJson, model);

            var newMesh = GenerateGeometry();

            if (currentMesh != newMesh)
            {
                currentMesh = newMesh;
                foreach (var entity in this.attachedEntities)
                {
                    OnShapeAttached(entity);
                }
            }

            return null;
        }
    }
}
