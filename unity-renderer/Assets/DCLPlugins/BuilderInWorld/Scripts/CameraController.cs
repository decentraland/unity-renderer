using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using DCL.Camera;
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
        internal IContext context;
        internal Texture2D lastScreenshot;

        public void Initialize(IContext context)
        {
            this.context = context;
            if (context.sceneReferences.cameraController != null)
            {
                if(context.sceneReferences.cameraController.GetComponent<Camera.CameraController>().TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
                    freeCameraController = (FreeCameraMovement) cameraState;
                
                cameraController = context.sceneReferences.cameraController.GetComponent<Camera.CameraController>();
            }

            initialEagleCameraHeight = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraHeight;
            initialEagleCameraDistance = context.editorContext.godModeDynamicVariablesAsset.initialEagleCameraDistance;
        }

        public void Dispose() { DeactivateCamera(); }

        public void TakeSceneScreenshot(IFreeCameraMovement.OnSnapshotsReady onSuccess)
        {
            //We deselect the entities to take better photos
            context.editorContext.entityHandler.DeselectEntities();
            freeCameraController.TakeSceneScreenshot((sceneSnapshot) =>
            {
                lastScreenshot = sceneSnapshot;
                onSuccess?.Invoke(sceneSnapshot);
            });
        }
        
        public void TakeSceneScreenshotFromResetPosition(IFreeCameraMovement.OnSnapshotsReady onSuccess)
        {
            //We deselect the entities to take better photos
            context.editorContext.entityHandler.DeselectEntities();
            freeCameraController.TakeSceneScreenshotFromResetPosition((sceneSnapshot) =>
            {
                lastScreenshot = sceneSnapshot;
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

        public Texture2D GetLastScreenshot() { return lastScreenshot;}

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