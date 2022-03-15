﻿using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCL.Components
{
    public abstract class ParametrizedShape<T> : BaseShape
        where T : BaseShape.Model, new()
    {
        public Dictionary<IDCLEntity, Rendereable> attachedRendereables = new Dictionary<IDCLEntity, Rendereable>();
        bool visibilityDirty = false;
        bool collisionsDirty = false;

        public virtual Mesh GenerateGeometry()
        {
            return null;
        }

        protected virtual void DestroyGeometry()
        {
            if (currentMesh == null)
                return;

            Object.Destroy(currentMesh);
            currentMesh = null;
        }

        public Mesh currentMesh { get; protected set; }
        private Model previousModel;
        private Model cachedModel;

        public ParametrizedShape()
        {
            OnAttach += OnShapeAttached;
            OnDetach += OnShapeDetached;
        }

        public override void UpdateFromModel(BaseModel newModel)
        {
            cachedModel = (Model) newModel;
            base.UpdateFromModel(newModel);
        }

        void UpdateRenderer(IDCLEntity entity, Model model = null)
        {
            if (model == null)
                model = (T) this.model;

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

        void OnShapeAttached(IDCLEntity entity)
        {
            if (entity == null)
                return;

            // First we remove the old rendereable, then we compute and add the new one.
            RemoveRendereableFromDataStore(entity);

            entity.EnsureMeshGameObject(componentName + " mesh");

            if (currentMesh == null)
            {
                currentMesh = GenerateGeometry();
            }

            MeshFilter meshFilter = entity.meshRootGameObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = entity.meshRootGameObject.AddComponent<MeshRenderer>();

            entity.meshesInfo.renderers = new Renderer[] { meshRenderer };
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

                transition.onFinishedLoading += () => { OnShapeFinishedLoading(entity); };
            }
            else
            {
                meshRenderer.sharedMaterial = Utils.EnsureResourcesMaterial("Materials/Default");
            }

            visibilityDirty = true;
            collisionsDirty = true;
            UpdateRenderer(entity);
            OnShapeFinishedLoading(entity);
            AddRendereableToDataStore(entity);
        }

        void OnShapeFinishedLoading(IDCLEntity entity)
        {
            entity.OnShapeUpdated?.Invoke(entity);
        }

        void OnShapeDetached(IDCLEntity entity)
        {
            if (entity == null || entity.meshRootGameObject == null)
                return;

            if (attachedEntities.Count == 0)
            {
                DestroyGeometry();
                Utils.CleanMaterials(entity.meshRootGameObject.GetComponent<Renderer>());
                currentMesh = null;
            }

            Utils.SafeDestroy(entity.meshRootGameObject);
            entity.meshesInfo.CleanReferences();

            RemoveRendereableFromDataStore(entity);
        }

        public override IEnumerator ApplyChanges(BaseModel newModelRaw)
        {
            var newModel = (T) newModelRaw;

            if (previousModel != null)
            {
                visibilityDirty = newModel.visible != previousModel.visible;
                collisionsDirty = newModel.withCollisions != previousModel.withCollisions || newModel.isPointerBlocker != previousModel.isPointerBlocker;
            }

            bool shouldGenerateMesh = ShouldGenerateNewMesh(previousModel);

            //NOTE(Brian): Only generate meshes here if they already are attached to something.
            //             Otherwise, the mesh will be created on the OnShapeAttached.
            if (attachedEntities.Count > 0)
            {
                using (var iterator = attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        RemoveRendereableFromDataStore(entity);
                    }
                }

                if (shouldGenerateMesh)
                {
                    DestroyGeometry();
                    currentMesh = GenerateGeometry();
                }

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
                        UpdateRenderer(entity, newModel);

                        entity.OnShapeUpdated?.Invoke(entity);
                    }
                }

                using (var iterator = attachedEntities.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        var entity = iterator.Current;
                        AddRendereableToDataStore(entity);
                    }
                }
            }

            previousModel = newModel;
            return null;
        }

        public override void AttachTo(IDCLEntity entity, System.Type overridenAttachedType = null)
        {
            if (attachedEntities.Contains(entity))
                return;

            base.AttachTo(entity);
        }

        public override bool IsVisible() { return cachedModel.visible; }

        public override bool HasCollisions() { return cachedModel.withCollisions; }

        protected virtual bool ShouldGenerateNewMesh(BaseShape.Model newModel) { return true; }

        private void RemoveRendereableFromDataStore(IDCLEntity entity)
        {
            if (!attachedRendereables.ContainsKey(entity))
                return;

            DataStore.i.sceneWorldObjects.RemoveRendereable(entity.scene.sceneData.id, attachedRendereables[entity]);
            attachedRendereables.Remove(entity);
        }

        private void AddRendereableToDataStore(IDCLEntity entity)
        {
            if (attachedRendereables.ContainsKey(entity))
                return;

            int triangleCount = currentMesh.triangles.Length;

            var newRendereable =
                new Rendereable()
                {
                    container = entity.meshRootGameObject,
                    totalTriangleCount = triangleCount,
                    meshes = new HashSet<Mesh>() { currentMesh },
                    meshToTriangleCount = new Dictionary<Mesh, int>() { { currentMesh, triangleCount } }
                };

            newRendereable.renderers = MeshesInfoUtils.ExtractUniqueRenderers(entity.meshRootGameObject);
            newRendereable.ownerId = entity.entityId;

            attachedRendereables.Add(entity, newRendereable);
            DataStore.i.sceneWorldObjects.AddRendereable(entity.scene.sceneData.id, newRendereable);
        }
    }
}