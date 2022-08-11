using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
using DCL.CameraTool;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Builder
{
    public class CameraController : ICameraController
    {
        private float initialEagleCameraHeight = 10f;
        private float initialEagleCameraDistance = 10f;

        private CameraMode.ModeId avatarCameraModeBeforeEditing;
        private Camera.CameraController cameraController;
        internal IFreeCameraMovement freeCameraController;
        internal IScreenshotCameraController screenshotCameraController;
        internal IContext context;
        internal Texture2D lastScreenshot;

        internal Vector3 initialCameraPosition;
        internal Vector3 initialPointToLookAt;

        public void Initialize(IContext context)
        {
            this.context = context;
            if (context.sceneReferences.cameraController != null)
            {
                if (context.sceneReferences.cameraController.GetComponent<Camera.CameraController>().TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
                    freeCameraController = (FreeCameraMovement) cameraState;

                cameraController = context.sceneReferences.cameraController.GetComponent<Camera.CameraController>();
            }

            initialEagleCameraHeight = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraHeight;
            initialEagleCameraDistance = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraDistance;

            screenshotCameraController = new ScreenshotCameraController();
            screenshotCameraController.Init(context);
        }

        public void Dispose()
        {
            DeactivateCamera();
            screenshotCameraController.Dispose();
        }

        public void TakeSceneScreenshot(IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
           screenshotCameraController.TakeSceneScreenshot(onSuccess);
        }

        public void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, int width, int height, IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
            screenshotCameraController.TakeSceneScreenshot(camPosition,pointToLookAt,width,height,onSuccess);
        }

        public void TakeSceneAerialScreenshot(IParcelScene parcelScene, IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
           screenshotCameraController.TakeSceneAerialScreenshot(parcelScene,onSuccess); 
        }

        public void TakeSceneScreenshotFromResetPosition(IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
           screenshotCameraController.TakeSceneScreenshot(initialCameraPosition,initialPointToLookAt,onSuccess);
        }

        public void ActivateCamera(IParcelScene parcelScene)
        {
            freeCameraController.gameObject.SetActive(true);
            Vector3 pointToLookAt = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
            Vector3 cameraPosition = GetInitialCameraPosition(parcelScene);

            initialPointToLookAt = pointToLookAt;
            initialCameraPosition = cameraPosition;
            
            freeCameraController.SetPosition(cameraPosition);
            freeCameraController.LookAt(pointToLookAt);
            freeCameraController.SetResetConfiguration(cameraPosition, pointToLookAt);

            if (cameraController != null && cameraController.currentCameraState.cameraModeId != CameraMode.ModeId.BuildingToolGodMode)
                avatarCameraModeBeforeEditing = cameraController.currentCameraState.cameraModeId;

            cameraController?.SetCameraMode(CameraMode.ModeId.BuildingToolGodMode);
        }

        public void DeactivateCamera()
        {
            if (!DataStore.i.common.isApplicationQuitting.Get())
                cameraController?.SetCameraMode(avatarCameraModeBeforeEditing);
        }

        public Texture2D GetLastScreenshot() { return lastScreenshot; }

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