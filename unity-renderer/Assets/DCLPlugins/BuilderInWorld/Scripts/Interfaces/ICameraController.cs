using System.Collections;
using System.Collections.Generic;
using DCL.Camera;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public interface ICameraController
    {
        void Initialize(IContext context);
        void Dispose();

        /// <summary>
        /// Take screenshot from the initial position of the camera
        /// </summary>
        /// <param name="onSuccess"></param>
        void TakeSceneScreenshot(IScreenshotCameraController.OnSnapshotsReady onSuccess);

        /// <summary>
        /// Take screenshot from the initial position
        /// </summary>
        /// <param name="onSuccess"></param>
        void TakeSceneScreenshotFromResetPosition(IScreenshotCameraController.OnSnapshotsReady onSuccess);

        /// <summary>
        /// Take screenshot from the camPosition looking at pointToLooKAt
        /// </summary>
        /// <param name="camPosition"></param>
        /// <param name="pointToLookAt"></param>
        /// <param name="onSuccess"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, int width, int height, IScreenshotCameraController.OnSnapshotsReady onSuccess);

        /// <summary>
        /// Take screenshot from the areal position of the scene
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <summary>
        void TakeSceneAerialScreenshot(IParcelScene parcelScene, IScreenshotCameraController.OnSnapshotsReady onSuccess);


        /// <summary>
        /// Activate the camera in the eagle mode to have a more editor feel
        /// </summary>
        /// <param name="parcelScene"></param>
        void ActivateCamera(IParcelScene parcelScene);

        /// <summary>
        /// Go back to the last camera used before entering the builder
        /// </summary>
        void DeactivateCamera();

        /// <summary>
        /// Get the last screenshot made by the camera
        /// </summary>
        /// <returns></returns>
        Texture2D GetLastScreenshot();
    }
}