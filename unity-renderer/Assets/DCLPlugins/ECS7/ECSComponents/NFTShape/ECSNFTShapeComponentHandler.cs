using System;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using DCL.Helpers;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSNFTShapeComponentHandler : IECSComponentHandler<PBNFTShape>, IShape
    {
        private INFTInfoLoadHelper infoLoadHelper;
        private INFTAssetLoadHelper assetLoadHelper;
        internal MeshesInfo meshesInfo;
        internal Rendereable rendereable;
        
        private PBNFTShape previousModel;
        private PBNFTShape currentModel;
        private bool isLoaded = false;
        private IParcelScene scene;

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity) { }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            DisposeMesh(scene);
            LoadWrapper loadWrapper = Environment.i.world.state.GetLoaderForEntity(entity);
            loadWrapper?.Unload();
            Environment.i.world.state.RemoveLoaderForEntity(entity);
            entity.meshesInfo.CleanReferences();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBNFTShape model)
        {
            this.scene = scene;
            this.currentModel = model;
            entity.meshesInfo.meshRootGameObject = NFTShapeFactory.InstantiateLoaderController(model.Style);
            entity.meshesInfo.currentShape = this;
            
            entity.meshRootGameObject.name = "NFT mesh";
            entity.meshRootGameObject.transform.SetParent(entity.gameObject.transform);
            entity.meshRootGameObject.transform.ResetLocalTRS();

            var loaderController = entity.meshRootGameObject.GetComponent<NFTShapeLoaderController>();

            if (loaderController)
                loaderController.Initialize(infoLoadHelper, assetLoadHelper);    
            
            var loadableShape = Environment.i.world.state.GetOrAddLoaderForEntity<LoadWrapper_NFT>(entity);

            loadableShape.entity = entity;
            loadableShape.initialVisibility = model.Visible;

            loadableShape.withCollisions = model.WithCollisions;
            loadableShape.backgroundColor = new UnityEngine.Color(model.Color.Red, model.Color.Green,model.Color.Blue,1);

            DataStore.i.ecs7.AddPendingResource(scene.sceneData.id, model);
            
            loadableShape.Load(model.Src, OnLoadCompleted, OnLoadFailed);
        }

        private void UpdateBackgroundColor(IDCLEntity entity, PBNFTShape model)
        {
            if (previousModel != null && model.Color.Equals(previousModel.Color))
                return;

            var loadableShape = Environment.i.world.state.GetLoaderForEntity(entity) as LoadWrapper_NFT;
            loadableShape?.loaderController.UpdateBackgroundColor(new UnityEngine.Color(model.Color.Red, model.Color.Green,model.Color.Blue,1));
        }
        
        internal void DisposeMesh(IParcelScene scene)
        {
            if(meshesInfo != null)
                ECSComponentsUtils.DisposeMeshInfo(meshesInfo);
            if(rendereable != null)
                ECSComponentsUtils.RemoveRendereableFromDataStore( scene.sceneData.id,rendereable);

            meshesInfo = null;
            rendereable = null;
        }
        
        protected void OnLoadFailed(LoadWrapper loadWrapper, Exception exception)
        {
            if (loadWrapper != null)
                CleanFailedWrapper(loadWrapper);
            
            DataStore.i.ecs7.RemovePendingResource(scene.sceneData.id, currentModel);
        }
        
        void CleanFailedWrapper(LoadWrapper loadWrapper)
        {
            if (loadWrapper == null)
                return;
            if (loadWrapper.entity == null)
                return;
            if (loadWrapper.entity.gameObject == null)
                return;

            GameObject go = loadWrapper.entity.gameObject;

            go.name += " - Failed loading";

            MaterialTransitionController[] transitionController =
                go.GetComponentsInChildren<MaterialTransitionController>(true);

            for (int i = 0; i < transitionController.Length; i++)
            {
                MaterialTransitionController material = transitionController[i];
                GameObject.Destroy(material);
            }
        }
        
        protected void OnLoadCompleted(LoadWrapper loadWrapper)
        {
            IDCLEntity entity = loadWrapper.entity;

            if (entity.meshesInfo.currentShape == null)
            {
                OnLoadFailed(loadWrapper, new Exception($"Entity {entity.entityId} current shape of mesh information is null"));
                return;
            }

            isLoaded = true;
            
            entity.meshesInfo.meshRootGameObject = entity.meshRootGameObject;

            ConfigureVisibility(entity.meshRootGameObject, currentModel.Visible, loadWrapper.entity.meshesInfo.renderers);
            ConfigureColliders(entity);
            
            UpdateBackgroundColor(entity,currentModel);
            
            RaiseOnShapeUpdated(entity);
            RaiseOnShapeLoaded(entity);
            
            DataStore.i.ecs7.RemovePendingResource(scene.sceneData.id, currentModel);
            
            previousModel = currentModel;
        }
        
        private void RaiseOnShapeUpdated(IDCLEntity entity)
        {
            if (!isLoaded)
                return;

            entity.OnShapeUpdated?.Invoke(entity);
        }

        private void RaiseOnShapeLoaded(IDCLEntity entity)
        {
            if (!isLoaded)
                return;

            entity.OnShapeLoaded?.Invoke(entity);
        }

        public static void ConfigureVisibility(GameObject meshGameObject, bool shouldBeVisible, Renderer[] meshRenderers = null)
        {
            if (meshGameObject == null)
                return;

            if (!shouldBeVisible)
            {
                MaterialTransitionController[] materialTransitionControllers = meshGameObject.GetComponentsInChildren<MaterialTransitionController>();

                for (var i = 0; i < materialTransitionControllers.Length; i++)
                {
                    GameObject.Destroy(materialTransitionControllers[i]);
                }
            }

            if (meshRenderers == null)
                meshRenderers = meshGameObject.GetComponentsInChildren<Renderer>(true);

            Collider onPointerEventCollider;

            for (var i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].enabled = shouldBeVisible;

                if (meshRenderers[i].transform.childCount > 0)
                {
                    onPointerEventCollider = meshRenderers[i].transform.GetChild(0).GetComponent<Collider>();

                    if (onPointerEventCollider != null && onPointerEventCollider.gameObject.layer == PhysicsLayers.onPointerEventLayer)
                        onPointerEventCollider.enabled = shouldBeVisible;
                }
            }
        }

        private void ConfigureColliders(IDCLEntity entity)
        {
            CollidersManager.i.ConfigureColliders(entity.meshRootGameObject, currentModel.WithCollisions, true, entity, ECSComponentsUtils.CalculateNFTCollidersLayer(currentModel.WithCollisions, currentModel.IsPointerBlocker));
        }

        public bool IsVisible() { return currentModel.Visible; }
        public bool HasCollisions() { return currentModel.WithCollisions; }
    }
}