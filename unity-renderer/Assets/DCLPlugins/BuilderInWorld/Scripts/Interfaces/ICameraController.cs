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
        void TakeSceneScreenshot(IFreeCameraMovement.OnSnapshotsReady onSuccess);

        /// <summary>
        /// Take screenshot from the initial position
        /// </summary>
        /// <param name="onSuccess"></param>
        void TakeSceneScreenshotFromResetPosition(IFreeCameraMovement.OnSnapshotsReady onSuccess);
        
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