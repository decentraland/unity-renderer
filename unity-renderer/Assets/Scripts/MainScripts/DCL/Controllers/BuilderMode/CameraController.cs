using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public class CameraController
    {
        private float initialEagleCameraHeight = 10f;
        private float initialEagleCameraDistance = 10f;

        private CameraMode.ModeId avatarCameraModeBeforeEditing;
        private Camera.CameraController cameraController;
        internal IFreeCameraMovement freeCameraController;

        public void Initialize(IContext context)
        {
            if (context.sceneReferences.cameraController.GetComponent<Camera.CameraController>().TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
                freeCameraController = (FreeCameraMovement) cameraState;

            initialEagleCameraHeight = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraHeight;
            initialEagleCameraDistance = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraDistance;

            cameraController = context.sceneReferences.cameraController.GetComponent<Camera.CameraController>();
        }

        public void Dispose() { DeactivateCamera(); }

        public void TakeSceneScreenshotFromResetPosition(IFreeCameraMovement.OnSnapshotsReady onSuccess)
        {
            freeCameraController.TakeSceneScreenshot((sceneSnapshot) =>
            {
                onSuccess?.Invoke(sceneSnapshot);
            });
        }

        public void ActivateCamera(IParcelScene parcelScene)
        {
            freeCameraController.gameObject.SetActive(true);
            Vector3 pointToLookAt = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
            Vector3 cameraPosition = GetInitialCameraPosition(parcelScene);
            freeCameraController.SetPosition(cameraPosition);
            freeCameraController.LookAt(pointToLookAt);

            if (cameraController.currentCameraState.cameraModeId != CameraMode.ModeId.BuildingToolGodMode)
                avatarCameraModeBeforeEditing = cameraController.currentCameraState.cameraModeId;

            cameraController.SetCameraMode(CameraMode.ModeId.BuildingToolGodMode);
        }

        public void DeactivateCamera() { cameraController.SetCameraMode(avatarCameraModeBeforeEditing); }

        internal Vector3 GetInitialCameraPosition(IParcelScene parcelScene)
        {
            Vector3 middlePoint = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
            Vector3 direction = (parcelScene.GetSceneTransform().position - middlePoint).normalized;

            return parcelScene.GetSceneTransform().position
                   + direction * initialEagleCameraDistance
                   + Vector3.up * initialEagleCameraHeight;
        }
    }
}