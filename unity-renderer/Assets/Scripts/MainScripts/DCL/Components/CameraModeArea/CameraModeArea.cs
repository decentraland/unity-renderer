using System;
using System.Collections;
using DCL.CameraTool;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class CameraModeArea : IEntityComponent, ICameraModeArea
    {
        [Serializable]
        public class Model : BaseModel
        {
            [Serializable]
            public class Area
            {
                public Vector3 box;
            }

            public Area area = new Area();
            public CameraMode.ModeId cameraMode = CameraMode.ModeId.ThirdPerson;

            public override BaseModel GetDataFromJSON(string json)
            {
                return Utils.SafeFromJson<Model>(json);
            }
        }

        private static CameraModeAreasController areasController { get; } = new CameraModeAreasController();

        private Collider playerCollider;
        internal IUpdateEventHandler updateEventHandler;
        internal int validCameraModes = 1 << (int)CameraMode.ModeId.FirstPerson | 1 << (int)CameraMode.ModeId.ThirdPerson;

        internal bool isPlayerInside = false;

        public Model areaModel { private set; get; } = new Model();
        public IParcelScene areaScene { private set; get; }
        public IDCLEntity areaEntity { private set; get; }

        CameraMode.ModeId ICameraModeArea.cameraMode => areaModel.cameraMode;

        IDCLEntity IEntityComponent.entity => areaEntity;

        IParcelScene IComponent.scene => areaScene;

        string IComponent.componentName => "CameraModeArea";

        void IComponent.UpdateFromJSON(string json)
        {
            OnModelUpdated(areaModel.GetDataFromJSON(json) as Model);
        }

        void IComponent.UpdateFromModel(BaseModel model)
        {
            OnModelUpdated(model as Model);
        }

        IEnumerator IComponent.ApplyChanges(BaseModel model) => null;

        void IComponent.RaiseOnAppliedChanges() { }

        bool IComponent.IsValid() => true;

        BaseModel IComponent.GetModel() => areaModel;

        int IComponent.GetClassId() => (int)CLASS_ID_COMPONENT.CAMERA_MODE_AREA;

        Transform IMonoBehaviour.GetTransform() => null;

        void ICleanable.Cleanup()
        {
            Dispose();
            DataStore.i.player.ownPlayer.OnChange -= OnOwnPlayerChange;
        }

        void IEntityComponent.Initialize(IParcelScene scene, IDCLEntity entity)
        {
            Initialize(scene, entity, Environment.i.platform.updateEventHandler, DataStore.i.player.ownPlayer.Get()?.collider);
            DataStore.i.player.ownPlayer.OnChange += OnOwnPlayerChange;
        }

        internal void OnModelUpdated(in Model newModel)
        {
            // NOTE: we don't want the ecs to be able to set "builder in world" camera or any other camera
            // that might be added in the future. Only first and third person camera are allowed
            if (!IsValidCameraMode(newModel.cameraMode))
            {
                newModel.cameraMode = CommonScriptableObjects.cameraMode.Get();
            }

            bool cameraModeChanged = newModel.cameraMode != areaModel.cameraMode;
            areaModel = newModel;

            if (cameraModeChanged && isPlayerInside)
            {
                areasController.ChangeAreaMode(this);
            }
        }

        internal void Initialize(in IParcelScene scene, in IDCLEntity entity, in IUpdateEventHandler updateEventHandler, in Collider playerCollider)
        {
            areaScene = scene;
            areaEntity = entity;
            this.updateEventHandler = updateEventHandler;
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

        private bool IsValidCameraMode(in CameraMode.ModeId mode)
        {
            if (validCameraModes == -1)
                return true;

            return ((1 << (int)mode) & validCameraModes) != 0;
        }

        private bool IsPlayerInsideArea()
        {
            if (areaEntity == null || areaScene == null)
            {
                return false;
            }

            if (areaScene.sceneData.id != CommonScriptableObjects.sceneID.Get())
            {
                return false;
            }

            Vector3 center = areaEntity.gameObject.transform.position;
            Quaternion rotation = areaEntity.gameObject.transform.rotation;

            Collider[] colliders = Physics.OverlapBox(center, areaModel.area.box * 0.5f, rotation,
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