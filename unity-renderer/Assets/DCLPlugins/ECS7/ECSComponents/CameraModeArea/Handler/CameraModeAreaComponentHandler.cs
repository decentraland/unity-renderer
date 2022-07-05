using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.Components;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class CameraModeAreaComponentHandler : IECSComponentHandler<PBCameraModeArea>
    {
        private readonly DataStore_Player dataStore;
        internal readonly IUpdateEventHandler updateEventHandler;

        internal CameraModeRepresentantion cameraModeRepresentantion;
        private UnityEngine.Vector3 area; 
        private IDCLEntity entity;
        private IParcelScene scene;
        internal PBCameraModeArea lastModel;
        
        internal ICameraModeAreasController areasController = new CameraModeAreasController();
        private Collider playerCollider;
        internal bool isPlayerInside = false;
        
        public CameraModeAreaComponentHandler(IUpdateEventHandler updateEventHandler, DataStore_Player dataStore)
        {
            this.updateEventHandler = updateEventHandler;
            this.dataStore = dataStore;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            Initialize(scene, entity, dataStore.ownPlayer.Get()?.collider);
            dataStore.ownPlayer.OnChange += OnOwnPlayerChange;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose();
            dataStore.ownPlayer.OnChange -= OnOwnPlayerChange;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBCameraModeArea model)
        {
            // We check if the mode should change, and adjust the area for the new model
            bool cameraModeChanged = lastModel != null || model.Mode != lastModel?.Mode;
            
            area = ProtoConvertUtils.PBVectorToUnityVector(model.Area);
            lastModel = model;
            
            // If the camera mode hasn't changed we skip the model 
            if (!cameraModeChanged)
                return;
            
            // We set the new mode
            cameraModeRepresentantion.SetCameraMode(ProtoConvertUtils.PBCameraEnumToUnityEnum(model.Mode));
            
            // If the mode must change and the player is inside, we change the mode here
            if (isPlayerInside)
                areasController.ChangeAreaMode(cameraModeRepresentantion);
        }
        
        internal void Initialize(in IParcelScene scene, in IDCLEntity entity, in Collider playerCollider)
        {
            this.playerCollider = playerCollider;
            this.scene = scene;
            this.entity = entity;
            
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
            cameraModeRepresentantion = new CameraModeRepresentantion();
        }

        internal void Update()
        {
            if (lastModel == null)
                return;
            
            bool playerInside = IsPlayerInsideArea();
            
            switch (playerInside)
            {
                case true when !isPlayerInside:
                    areasController.AddInsideArea(cameraModeRepresentantion);
                    break;
                case false when isPlayerInside:
                    areasController.RemoveInsideArea(cameraModeRepresentantion);
                    break;
            }
            
            isPlayerInside = playerInside;
        }

        internal void Dispose()
        {
            OnAreaDisabled();
            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
        }

        private bool IsPlayerInsideArea()
        {
            if (entity == null || scene == null ||
                scene.sceneData.id != CommonScriptableObjects.sceneID.Get())
                return false;
            
            UnityEngine.Vector3 center = entity.gameObject.transform.position;
            Quaternion rotation = entity.gameObject.transform.rotation;

            Collider[] colliders = Physics.OverlapBox(center, area * 0.5f, rotation,
                PhysicsLayers.avatarTriggerMask, QueryTriggerInteraction.Collide);

            if (colliders.Length == 0)
                return false;

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == playerCollider)
                    return true;
            }
            return false;
        }

        private void OnAreaDisabled()
        {
            if (!isPlayerInside)
                return;
            
            areasController.RemoveInsideArea(cameraModeRepresentantion);
            isPlayerInside = false;
        }

        private void OnOwnPlayerChange(Player current, Player previous)
        {
            playerCollider = current.collider;
        }
    }
}