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

        public ParametrizedShape(ParcelScene scene) : base(scene)
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        void UpdateRenderer(DecentralandEntity entity)
        {
            if (visibilityDirty)
            {
                ConfigureVisibility(entity.meshGameObject, model.visible);
                visibilityDirty = false;
            }

            if (collisionsDirty)
            {
                CollidersManager.i.ConfigureColliders(entity.meshGameObject, model.withCollisions, false, entity);
                collisionsDirty = false;
            }
        }

        void OnShapeAttached(DecentralandEntity entity)
        {
            if (entity == null)
                return;

            entity.EnsureMeshGameObject(componentName + " mesh");

            if (currentMesh == null)
                currentMesh = GenerateGeometry();

            MeshFilter meshFilter = entity.meshGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshGameObject.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = currentMesh;

            if (Configuration.ParcelSettings.VISUAL_LOADING_ENABLED)
            {
                MaterialTransitionController transition =
                    entity.meshGameObject.AddComponent<MaterialTransitionController>();
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

            visibilityDirty = true;
            collisionsDirty = true;
            UpdateRenderer(entity);

            if (entity.OnShapeUpdated != null)
            {
                entity.OnShapeUpdated.Invoke(entity);
            }
        }

        void OnShapeDetached(DecentralandEntity entity)
        {
            if (entity == null || entity.meshGameObject == null)
            {
                return;
            }

            if (attachedEntities.Count == 0)
            {
                DestroyGeometry();
                Utils.CleanMaterials(entity.meshGameObject.GetComponent<Renderer>());
                currentMesh = null;
            }

            Utils.SafeDestroy(entity.meshGameObject);
            entity.meshGameObject = null;
        }

        public override IEnumerator ApplyChanges(string newJson)
        {
            var newModel = SceneController.i.SafeFromJson<T>(newJson);
            visibilityDirty = newModel.visible != model.visible;
            collisionsDirty = newModel.withCollisions != model.withCollisions;
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
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        UpdateRenderer(entity);
                    }
                }
            }

            return null;
        }

        public override void AttachTo(DecentralandEntity entity, System.Type overridenAttachedType = null)
        {
            base.AttachTo(entity);
        }

        protected virtual bool ShouldGenerateNewMesh(BaseShape.Model newModel)
        {
            return true;
        }
    }
}
