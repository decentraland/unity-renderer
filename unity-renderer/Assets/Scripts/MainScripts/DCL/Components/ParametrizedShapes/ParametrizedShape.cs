using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using UnityEngine;

namespace DCL.Components
{
    public abstract class ParametrizedShape<T> : BaseShape where T : BaseShape.Model, new()
    {
        public T model = new T();
        bool visibilityDirty = false;
        bool collisionsDirty = false;

        public abstract Mesh GenerateGeometry();

        protected virtual void DestroyGeometry()
        {
            if (currentMesh != null)
            {
                GameObject.Destroy(currentMesh);
                currentMesh = null;
            }
        }

        public Mesh currentMesh { get; protected set; }

        public ParametrizedShape(IParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void UpdateRenderer(DecentralandEntity entity)
        {
            if (visibilityDirty)
            {
                ConfigureVisibility(entity.meshRootGameObject, model.visible, entity.meshesInfo.renderers);
                visibilityDirty = false;
            }

            if (collisionsDirty)
            {
                CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, model.withCollisions, false, entity, CalculateCollidersLayer(model));
                collisionsDirty = false;
            }

            if (entity.meshesInfo.meshFilters.Length > 0 && entity.meshesInfo.meshFilters[0].sharedMesh != currentMesh)
            {
                entity.meshesInfo.UpdateExistingMeshAtIndex(currentMesh, 0);
            }
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            if (entity == null)
                return;

            entity.EnsureMeshGameObject(componentName + " mesh");

            if (currentMesh == null)
                currentMesh = GenerateGeometry();

            MeshFilter meshFilter = entity.meshRootGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshRootGameObject.AddComponent<MeshRenderer>();
            entity.meshesInfo.renderers = new Renderer[] {meshRenderer};
            entity.meshesInfo.currentShape = this;

            meshFilter.sharedMesh = currentMesh;

            if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
            {
                MaterialTransitionController transition = entity.meshRootGameObject.AddComponent<MaterialTransitionController>();
                Material finalMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
                transition.delay = 0;
                transition.useHologram = false;
                transition.fadeThickness = 20;
                transition.OnDidFinishLoading(finalMaterial);

                transition.onFinishedLoading += () => { entity.OnShapeUpdated?.Invoke(entity); };
            }
            else
            {
                meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
            }

            visibilityDirty = true;
            collisionsDirty = true;
            UpdateRenderer(entity);

            entity.OnShapeUpdated?.Invoke(entity);
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null) return;

            if (attachedEntities.Count == 0)
            {
                DestroyGeometry();
                Utils.CleanMaterials(entity.meshRootGameObject.GetComponent<Renderer>());
                currentMesh = null;
            }

            Utils.SafeDestroy(entity.meshRootGameObject);
            entity.meshesInfo.CleanReferences();
        }

        public override object GetModel()
        {
            return model;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            var newModel = Utils.SafeFromJson<T>(newJson);
            visibilityDirty = newModel.visible != model.visible;
            collisionsDirty = newModel.withCollisions != model.withCollisions || newModel.isPointerBlocker != model.isPointerBlocker;
            bool shouldGenerateMesh = ShouldGenerateNewMesh(newModel);
            model = newModel;

            //NOTE(Brian): Only generate meshes here if they already are attached to something.
            //             Otherwise, the mesh will be created on the OnShapeAttached.
            if (attachedEntities.Count > 0)
            {
                if (shouldGenerateMesh)
                    currentMesh = GenerateGeometry();

                using (var iterator = attachedEntities.GetEnumerator())
                {
                    bool cachedVisibilityDirty = visibilityDirty;
                    bool cachedCollisionDirty = collisionsDirty;
                    while (iterator.MoveNext())
                    {
                        //NOTE(Alex): Since UpdateRenderer updates the dirty flags as well we have to make sure every entity
                        //            gets updated accordingly to the original flags.
                        visibilityDirty = cachedVisibilityDirty;
                        collisionsDirty = cachedCollisionDirty;

                        var entity = iterator.Current;
                        UpdateRenderer(entity);

                        entity.OnShapeUpdated?.Invoke(entity);
                    }
                }
            }

            return null;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity)) return;

            base.AttachTo(entity);
        }

        public override bool IsVisible()
        {
            return model.visible;
        }

        public override bool HasCollisions()
        {
            return model.withCollisions;
        }

        protected virtual bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            return true;
        }
    }
}