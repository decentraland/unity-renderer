using System;
using System.Collections;
using System.Linq;
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

        private static CameraModeAreasController areasController => new CameraModeAreasController();

        private ISceneReferences sceneReferences => SceneReferences.i;
        private Collider playerCollider => sceneReferences?.playerAvatarController.avatarCollider;
        private IUpdateEventHandler updateEventHandler => Environment.i?.platform.updateEventHandler;

        private bool isPlayerInside = false;

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

        Transform IMonoBehaviour.GetTransform() => areaEntity?.gameObject.transform;

        void ICleanable.Cleanup()
        {
            Dispose();
        }

        void IEntityComponent.Initialize(IParcelScene scene, IDCLEntity entity)
        {
            areaScene = scene;
            areaEntity = entity;
            updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, Update);
        }

        private void OnModelUpdated(Model newModel)
        {
            bool cameraModeChanged = newModel.cameraMode != areaModel.cameraMode;
            areaModel = newModel;

            if (cameraModeChanged && isPlayerInside)
            {
                areasController.ChangeAreaMode(this, areaModel.cameraMode);
            }
        }

        private void Update()
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

        private void Dispose()
        {
            OnAreaDisabled();
            updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, Update);
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
                PhysicsLayers.avatarTriggerLayer, QueryTriggerInteraction.Collide);

            if (colliders.Length == 0)
            {
                return false;
            }

            return colliders.Any(collider => collider == playerCollider);
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
    }
}