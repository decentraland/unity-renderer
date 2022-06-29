using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class CameraModeAreaComponentHandler : IECSComponentHandler<PBCameraModeArea>
    {
        private readonly DataStore_Player dataStore;
        internal readonly IUpdateEventHandler updateEventHandler;

        private IDCLEntity entity;
        private IParcelScene scene;
        private PBCameraModeArea lastModel;
        
        private static CameraModeAreasController areasController { get; } = new CameraModeAreasController();

        private Collider playerCollider;

        internal int validCameraModes = 1 << (int)CameraMode.ModeId.FirstPerson | 1 << (int)CameraMode.ModeId.ThirdPerson;

        internal bool isPlayerInside = false;
        
        public CameraModeAreaComponentHandler(IUpdateEventHandler updateEventHandler, DataStore_Player dataStore)
        {
            this.updateEventHandler = updateEventHandler;
            this.dataStore = dataStore;
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            Initialize(scene, entity, Environment.i.platform.updateEventHandler, dataStore.ownPlayer.Get()?.collider);
            dataStore.ownPlayer.OnChange += OnOwnPlayerChange;
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Dispose();
            DataStore.i.player.ownPlayer.OnChange -= OnOwnPlayerChange;
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBCameraModeArea model)
        {
            bool cameraModeChanged = model.Mode != lastModel.Mode;
            lastModel = model;

            if (cameraModeChanged && isPlayerInside)
            {
                areasController.ChangeAreaMode(this, model.Mode);
            }
        }
        
        internal void Initialize(in IParcelScene scene, in IDCLEntity entity, in Collider playerCollider)
        {
            this.playerCollider = playerCollider;

            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        }

        internal void Update()
        {
            bool playerInside = IsPlayerInsideArea();

            switch (playerInside)
            {
                case true when !isPlayerInside:
                    areasController.AddInsideArea(this);
                    break;
                case false when isPlayerInside:
                    areasController.RemoveInsideArea(this);
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

            Collider[] colliders = Physics.OverlapBox(center, lastModel.Area * 0.5f, rotation,
                PhysicsLayers.avatarTriggerMask, QueryTriggerInteraction.Collide);

            if (colliders.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] == playerCollider)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnAreaDisabled()
        {
            if (!isPlayerInside)
            {
                return;
            }
            areasController.RemoveInsideArea(this);
            isPlayerInside = false;
        }

        private void OnOwnPlayerChange(Player current, Player previous)
        {
            playerCollider = current.collider;
        }
    }
}