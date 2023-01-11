using DCL.CameraTool;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using ECSSystems.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public class ECSCameraEntitySystem : IDisposable
    {
        private readonly PBCameraMode reusableCameraMode;
        private readonly PBPointerLock reusablePointerLock;
        private readonly BaseVariable<Transform> cameraTransform;
        private readonly Vector3Variable worldOffset;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IECSComponentWriter componentsWriter;
        private readonly BaseVariableAsset<CameraMode.ModeId> cameraMode;

        private Vector3 lastCameraPosition = Vector3.zero;
        private Quaternion lastCameraRotation = Quaternion.identity;
        private bool newSceneAdded = false;

        private Transform currentCameraTransform;
        private Vector3 currentWorldOffset;
        private CameraMode.ModeId currentCameraMode;

        public ECSCameraEntitySystem(IECSComponentWriter componentsWriter, PBCameraMode reusableCameraMode, PBPointerLock reusablePointerLock,
            BaseList<IParcelScene> loadedScenes, BaseVariable<Transform> cameraTransform, Vector3Variable worldOffset,
            BaseVariableAsset<CameraMode.ModeId> cameraMode)
        {
            this.cameraTransform = cameraTransform;
            this.worldOffset = worldOffset;
            this.loadedScenes = loadedScenes;
            this.componentsWriter = componentsWriter;
            this.cameraMode = cameraMode;
            this.reusableCameraMode = reusableCameraMode;
            this.reusablePointerLock = reusablePointerLock;

            loadedScenes.OnAdded += LoadedScenesOnOnAdded;
            cameraTransform.OnChange += OnCameraTransformChanged;
            worldOffset.OnChange += OnWorldOffsetChanged;
            cameraMode.OnChange += OnCameraModeChanged;

            OnCameraTransformChanged(cameraTransform.Get(), null);
            OnWorldOffsetChanged(worldOffset.Get(), Vector3.zero);
            OnCameraModeChanged(cameraMode.Get(), CameraMode.ModeId.ThirdPerson);
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= LoadedScenesOnOnAdded;
            cameraTransform.OnChange -= OnCameraTransformChanged;
            worldOffset.OnChange -= OnWorldOffsetChanged;
            cameraMode.OnChange -= OnCameraModeChanged;
        }

        public void Update()
        {
            if (currentCameraTransform == null)
                return;

            Transform cameraT = currentCameraTransform;

            Vector3 cameraPosition = cameraT.position;
            Quaternion cameraRotation = cameraT.rotation;

            bool updateTransform = newSceneAdded || (lastCameraPosition != cameraPosition || lastCameraRotation != cameraRotation);
            newSceneAdded = false;

            lastCameraPosition = cameraPosition;
            lastCameraRotation = cameraRotation;

            reusableCameraMode.Mode = ProtoConvertUtils.UnityEnumToPBCameraEnum(currentCameraMode);
            reusablePointerLock.IsPointerLocked = Utils.IsCursorLocked;

            Vector3 worldOffset = currentWorldOffset;
            IReadOnlyList<IParcelScene> loadedScenes = this.loadedScenes;

            IParcelScene scene;

            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.CAMERA_ENTITY, ComponentID.CAMERA_MODE,
                    reusableCameraMode, ECSComponentWriteType.SEND_TO_SCENE);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.CAMERA_ENTITY, ComponentID.POINTER_LOCK,
                    reusablePointerLock, ECSComponentWriteType.SEND_TO_SCENE);

                if (!updateTransform)
                    continue;

                var transform = TransformHelper.SetTransform(scene, ref cameraPosition, ref cameraRotation, ref worldOffset);

                componentsWriter.PutComponent(scene.sceneData.sceneNumber, SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM,
                    transform);
            }
        }

        private void LoadedScenesOnOnAdded(IParcelScene obj)
        {
            newSceneAdded = true;
        }

        private void OnCameraModeChanged(CameraMode.ModeId current, CameraMode.ModeId previous)
        {
            currentCameraMode = current;
        }

        private void OnWorldOffsetChanged(Vector3 current, Vector3 previous)
        {
            currentWorldOffset = current;
        }

        private void OnCameraTransformChanged(Transform current, Transform previous)
        {
            currentCameraTransform = current;
        }
    }
}
