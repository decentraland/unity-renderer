using DCL.Camera;
using DCL.Configuration;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public class ScreenshotCameraController : IScreenshotCameraController
    {
        private IContext context;
        private UnityEngine.Camera screenshotCamera;
        internal IFreeCameraMovement freeCameraMovement;

        public void Init(IContext context)
        {
            this.context = context;
            GameObject screenshotCameraGameObject = new GameObject("BuilderScreenshotCamera");
            screenshotCamera = screenshotCameraGameObject.AddComponent<UnityEngine.Camera>();
            screenshotCamera.depth = -9999;
       
            screenshotCamera.gameObject.SetActive(false);
            
            if (context.sceneReferences.cameraController != null)
            {
                var cameraController = context.sceneReferences.cameraController.GetComponent<Camera.CameraController>();
                // We assign the same culling configuration as the camera
                var biwCulling = BIWUtils.GetBIWCulling(cameraController.GetCulling());
                screenshotCamera.cullingMask = biwCulling;
                
                if(cameraController.TryGetCameraStateByType<FreeCameraMovement>(out CameraStateBase cameraState))
                    freeCameraMovement = (FreeCameraMovement) cameraState;
            } 
        }

        public void Dispose() { GameObject.Destroy(screenshotCamera.gameObject); }

        public void TakeSceneAerialScreenshot(IParcelScene parcelScene, IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
            Vector3 pointToLookAt = BIWUtils.CalculateUnityMiddlePoint(parcelScene);
            float heightPosition = context.editorContext.godModeDynamicVariablesAsset.aerialScreenshotHeight * Mathf.Sqrt(parcelScene.sceneData.parcels.Length);
            Vector3 cameraPosition = pointToLookAt  + Vector3.up * heightPosition;

            TakeSceneScreenshot(cameraPosition, pointToLookAt, BIWSettings.AERIAL_SCREENSHOT_WIDTH, BIWSettings.AERIAL_SCREENSHOT_HEIGHT, onSuccess);
        }

        public void TakeSceneScreenshot(IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
            screenshotCamera.transform.position = freeCameraMovement.GetCameraPosition;
            screenshotCamera.transform.rotation = freeCameraMovement.gameObject.transform.rotation;

            TakeScreenshot(onSuccess, BIWSettings.SCENE_SNAPSHOT_WIDTH_RES, BIWSettings.SCENE_SNAPSHOT_HEIGHT_RES);
        }

        public void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, IScreenshotCameraController.OnSnapshotsReady onSuccess) { TakeSceneScreenshot(camPosition, pointToLookAt, BIWSettings.SCENE_SNAPSHOT_WIDTH_RES, BIWSettings.SCENE_SNAPSHOT_HEIGHT_RES , onSuccess); }

        public void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, int width, int height, IScreenshotCameraController.OnSnapshotsReady onSuccess)
        {
            screenshotCamera.transform.position = camPosition;
            screenshotCamera.transform.LookAt(pointToLookAt);
            TakeScreenshot(onSuccess, width, height);
        }

        private void TakeScreenshot(IScreenshotCameraController.OnSnapshotsReady callback, int width, int height)
        {
            if (UnityEngine.Camera.main == null)
                return;

            //We deselect the entities to take better photos
            context.editorContext.entityHandler.DeselectEntities();
            
            screenshotCamera.gameObject.SetActive(true);

            var current = screenshotCamera.targetTexture;
            screenshotCamera.targetTexture = null;

            Texture2D sceneScreenshot = ScreenshotFromCamera(width, height);
            screenshotCamera.targetTexture = current;
            screenshotCamera.gameObject.SetActive(false);
            
            callback?.Invoke(sceneScreenshot);
        }

        private Texture2D ScreenshotFromCamera(int width, int height)
        {
            RenderTexture rt = new RenderTexture(width, height, 32);
            screenshotCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            screenshotCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenShot.Apply();
            return screenShot;
        }
    }
}