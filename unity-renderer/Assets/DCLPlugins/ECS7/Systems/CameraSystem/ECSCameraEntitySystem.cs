using DCL;
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
        private readonly RendererState rendererState;
        private readonly Vector3Variable worldOffset;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly IECSComponentWriter componentsWriter;
        private readonly BaseVariableAsset<CameraMode.ModeId> cameraMode;

        private Vector3 lastCameraPosition = Vector3.zero;
        private Quaternion lastCameraRotation = Quaternion.identity;
        private bool newSceneAdded = false;

        public ECSCameraEntitySystem(IECSComponentWriter componentsWriter,  PBCameraMode reusableCameraMode, PBPointerLock reusablePointerLock)
        {
            cameraTransform = DataStore.i.camera.transform;
            rendererState = CommonScriptableObjects.rendererState;
            worldOffset = CommonScriptableObjects.worldOffset;
            loadedScenes = DataStore.i.ecs7.scenes;
            this.componentsWriter = componentsWriter;
            cameraMode = CommonScriptableObjects.cameraMode;
            this.reusableCameraMode = reusableCameraMode;
            this.reusablePointerLock = reusablePointerLock;

            loadedScenes.OnAdded += LoadedScenesOnOnAdded;
        }

        public void Dispose()
        {
            loadedScenes.OnAdded -= LoadedScenesOnOnAdded;
        }

        public void Update()
        {
            if (!rendererState.Get())
                return;

            Transform cameraT = cameraTransform.Get();

            Vector3 cameraPosition = cameraT.position;
            Quaternion cameraRotation = cameraT.rotation;

            bool updateTransform = newSceneAdded || (lastCameraPosition != cameraPosition || lastCameraRotation != cameraRotation);
            newSceneAdded = false;

            lastCameraPosition = cameraPosition;
            lastCameraRotation = cameraRotation;

            reusableCameraMode.Mode = ProtoConvertUtils.UnityEnumToPBCameraEnum(cameraMode.Get());
            reusablePointerLock.IsPointerLocked = Utils.IsCursorLocked;

            Vector3 worldOffset = this.worldOffset.Get();
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
    }
}
