using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseParametrizedShape<T> : BaseShape where T : BaseShape.Model, new()
    {
        public T model = new T();

        public abstract Mesh GenerateGeometry();

        private Mesh currentMesh = null;

        public BaseParametrizedShape(ParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            if (entity == null)
                return;

            entity.EnsureMeshGameObject(componentName + " mesh");

            MeshFilter meshFilter = entity.meshGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshGameObject.AddComponent<MeshRenderer>();


            meshFilter.sharedMesh = currentMesh;

            if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
            {
                MaterialTransitionController transition = entity.meshGameObject.AddComponent<MaterialTransitionController>();
                Material finalMaterial = Resources.Load<Material>("Materials/Default");
                transition.delay = 0;
                transition.useHologram = false;
                transition.fadeThickness = 20;
                transition.OnDidFinishLoading(finalMaterial);
            }
            else
            {
                meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Default");
            }

            if (entity.OnShapeUpdated != null)
                entity.OnShapeUpdated.Invoke(entity);

            ConfigureColliders(entity, model.withCollisions);
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
                return;

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            bool hadCollisions = model.withCollisions;
            JsonUtility.FromJsonOverwrite(newJson, model);

            var newMesh = GenerateGeometry();

            if (currentMesh != newMesh)
            {
                currentMesh = newMesh;
                foreach (var entity in this.attachedEntities)
                {
                    OnShapeDetached(entity);
                    OnShapeAttached(entity);
                }
            }
            else
            {
                bool collisionsDirty = hadCollisions != model.withCollisions;

                if (collisionsDirty)
                {
                    foreach (var entity in this.attachedEntities)
                    {
                        ConfigureColliders(entity, model.withCollisions);
                    }
                }
            }
            return null;
        }
    }
}
