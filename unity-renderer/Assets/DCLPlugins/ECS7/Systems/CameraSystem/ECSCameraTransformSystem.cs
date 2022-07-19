using System;
using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.Models;
using ECSSystems.Helpers;
using UnityEngine;

namespace ECSSystems.CameraSystem
{
    public static class ECSCameraTransformSystem
    {
        private class State
        {
            public BaseVariable<Transform> cameraTransform;
            public RendererState rendererState;
            public Vector3Variable worldOffset;
            public UnityEngine.Vector3 lastCameraPosition = UnityEngine.Vector3.zero;
            public Quaternion lastCameraRotation = Quaternion.identity;
        }

        public static Action CreateSystem()
        {
            var state = new State()
            {
                cameraTransform = DataStore.i.camera.transform,
                rendererState = CommonScriptableObjects.rendererState,
                worldOffset = CommonScriptableObjects.worldOffset

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

            if (state.lastCameraPosition == cameraPosition && state.lastCameraRotation == cameraRotation)
            {
                return;
            }

            state.lastCameraPosition = cameraPosition;
            state.lastCameraRotation = cameraRotation;

            UnityEngine.Vector3 worldOffset = state.worldOffset.Get();

            var loadedScenes = ECSSystemsReferencesContainer.loadedScenes;
            var componentsWriter = ECSSystemsReferencesContainer.componentsWriter;

            IParcelScene scene;
            for (int i = 0; i < loadedScenes.Count; i++)
            {
                scene = loadedScenes[i];

                var transform = TransformHelper.SetTransform(scene, ref cameraPosition, ref cameraRotation, ref worldOffset);
                componentsWriter.PutComponent(scene.sceneData.id, SpecialEntityId.CAMERA_ENTITY, ComponentID.TRANSFORM,
                    transform);
            }
        }
    }
}