using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public abstract class BaseParametrizedShape<T> : BaseShape where T : BaseShape.Model, new()
    {
        public T model = new T();

        public abstract Mesh GenerateGeometry();

        public Mesh currentMesh { get; private set; }

        public BaseParametrizedShape(ParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            entity.EnsureMeshGameObject(componentName + " mesh");

            MeshFilter meshFilter = entity.meshGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshGameObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = currentMesh;

            if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
            {
                MaterialTransitionController transition = entity.meshGameObject.AddComponent<MaterialTransitionController>();
                Material finalMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
                transition.delay = 0;
                transition.useHologram = false;
                transition.fadeThickness = 20;
                transition.OnDidFinishLoading(finalMaterial);
            }
            else
            {
                meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
            }

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }

            ConfigureColliders(entity.meshGameObject, model.withCollisions);
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            if (attachedEntities.Count == 0)
            {
                if (currentMesh != null)
                {
                    GameObject.Destroy(currentMesh);
                }

                Utils.CleanMaterials(entity.meshGameObject.GetComponent<Renderer>());
            }

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;

        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            bool hadCollisions = model.withCollisions;
            bool isVisible = model.visible;
            model = JsonUtility.FromJson<T>(newJson);

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
                        ConfigureColliders(entity.meshGameObject, model.withCollisions);
                    }
                }
            }

            bool visibilityDirty = isVisible != model.visible;
            if (visibilityDirty)
            {
                foreach (var entity in attachedEntities)
                {
                    ConfigureVisibility(entity.meshGameObject, model.visible);
                }
            }

            return null;
        }

        public override void AttachTo(DecentralandEntity entity)
        {
            base.AttachTo(entity);
            ConfigureVisibility(entity.meshGameObject, model.visible);
        }
    }
}
