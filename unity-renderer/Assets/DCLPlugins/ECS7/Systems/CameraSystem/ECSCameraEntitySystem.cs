using System;
using System.Collections.Generic;
using DCL;
using DCL.CameraTool;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public static class ECSCameraEntitySystem
    {
        private static readonly PBCameraMode reusableCameraMode = new PBCameraMode();
        private static readonly PBPointerLock reusablePointerLock = new PBPointerLock();

        private class State
        {
            public BaseVariable<Transform> cameraTransform;
            public RendererState rendererState;
            public Vector3Variable worldOffset;
            public IReadOnlyList<IParcelScene> loadedScenes;
            public IECSComponentWriter componentsWriter;
            public BaseVariableAsset<CameraMode.ModeId> cameraMode;
            public UnityEngine.Vector3 lastCameraPosition = UnityEngine.Vector3.zero;
            public Quaternion lastCameraRotation = Quaternion.identity;
        }

        public static Action CreateSystem(IECSComponentWriter componentsWriter)
        {
            var state = new State()
            {
                cameraTransform = DataStore.i.camera.transform,
                rendererState = CommonScriptableObjects.rendererState,
                worldOffset = CommonScriptableObjects.worldOffset,
                loadedScenes = DataStore.i.ecs7.scenes,
                componentsWriter = componentsWriter,
                cameraMode = CommonScriptableObjects.cameraMode
            };
            return () => Update(state);
        }

        private static void Update(State state)
        {
            if (!state.rendererState.Get())
                return;

            Transform cameraT = state.cameraTransform.Get();

            UnityEngine.Vector3 cameraPosition = cameraT.position;
            Quaternion cameraRotation = cameraT.rotation;

            bool updateTransform = (state.lastCameraPosition != cameraPosition || state.lastCameraRotation != cameraRotation);

            state.lastCameraPosition = cameraPosition;
            state.lastCameraRotation = cameraRotation;

            reusableCameraMode.Mode = ProtoConvertUtils.UnityEnumToPBCameraEnum(state.cameraMode.Get());
            reusablePointerLock.IsPointerLocked = Utils.IsCursorLocked;

            UnityEngine.Vector3 worldOffset = state.worldOffset.Get();

            var loadedScenes = state.loadedScenes;
            var componentsWriter = state.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.CAMERA_MODE,
                    reusableCameraMode, ECSComponentWriteType.SEND_TO_SCENE);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.POINTER_LOCK,
                    reusablePointerLock, ECSComponentWriteType.SEND_TO_SCENE);                

                if (!updateTransform)
                    continue;

                var transform = TransformHelper.SetTransform(scene, ref cameraPosition, ref cameraRotation, ref worldOffset);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM,
                    transform);
            }
        }
    }
}