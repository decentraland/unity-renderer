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
        public Dictionary<string, Rendereable> attachedRendereables = new Dictionary<string, Rendereable>();
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

            AddOrUpdateRendereableToDataStore(entity);
        }

        void OnShapeAttached(IDCLEntity entity)
        {
            if (entity == null)
                return;

            entity.EnsureMeshGameObject(componentName + " mesh");

            if (currentMesh == null)
            {
                currentMesh = GenerateGeometryAndUpdateDataStore();
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
                DestroyGeometryAndUpdateDataStore();
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
                if (shouldGenerateMesh)
                {
                    DestroyGeometryAndUpdateDataStore();
                    currentMesh = GenerateGeometryAndUpdateDataStore();
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

        private void DestroyGeometryAndUpdateDataStore()
        {
            DataStore.i.sceneWorldObjects.RemoveMesh(scene.sceneData.id, currentMesh);
            DestroyGeometry();
        }

        private Mesh GenerateGeometryAndUpdateDataStore()
        {
            Mesh mesh = GenerateGeometry();
            DataStore.i.sceneWorldObjects.AddMesh(scene.sceneData.id, mesh);
            return mesh;
        }

        private void RemoveRendereableFromDataStore(IDCLEntity entity)
        {
            string sceneId = entity.scene.sceneData.id;

            if ( attachedRendereables.ContainsKey(sceneId) )
            {
                DataStore.i.sceneWorldObjects.RemoveRendereable(sceneId, attachedRendereables[sceneId]);
                attachedRendereables.Remove(sceneId);
            }
        }

        private void AddOrUpdateRendereableToDataStore(IDCLEntity entity)
        {
            string sceneId = entity.scene.sceneData.id;

            int triangleCount = currentMesh.triangles.Length;

            var newRendereable = new Rendereable()
            {
                renderers = new List<Renderer>(entity.meshesInfo.renderers),
                container = entity.meshRootGameObject,
                totalTriangleCount = triangleCount,
                meshes = new List<Mesh>() { currentMesh },
                meshToTriangleCount = new Dictionary<Mesh, int>() { { currentMesh, triangleCount } }
            };

            if ( !attachedRendereables.ContainsKey(sceneId) )
            {
                attachedRendereables.Add(sceneId, newRendereable);
                DataStore.i.sceneWorldObjects.AddRendereable(sceneId, newRendereable);
            }
            else if ( !newRendereable.Equals(attachedRendereables[sceneId]))
            {
                // NOTE(Brian): We remove and set the new one to trigger the change events
                DataStore.i.sceneWorldObjects.RemoveRendereable(sceneId, attachedRendereables[sceneId]);
                DataStore.i.sceneWorldObjects.AddRendereable(sceneId, newRendereable);
                attachedRendereables.Remove(sceneId);
                attachedRendereables.Add(sceneId, newRendereable);
            }
        }
    }
}