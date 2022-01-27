using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;

namespace DCL.Builder
{
    public interface IScreenshotCameraController
    {
        void Init(IContext context);

        void Dispose();
        
        /// <summary>
        /// Delegate of the screenshot
        /// </summary>
        /// <param name="sceneSnapshot"></param>
        delegate void OnSnapshotsReady(Texture2D sceneSnapshot);
        
        /// <summary>
        /// Take screenshot from the initial position of the camera
        /// </summary>
        /// <param name="onSuccess"></param>
        void TakeSceneScreenshot(OnSnapshotsReady onSuccess);
        
        /// <summary>
        /// Take screenshot from the camPosition looking at pointToLooKAt
        /// </summary>
        /// <param name="camPosition"></param>
        /// <param name="pointToLookAt"></param>
        /// <param name="onSuccess"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, OnSnapshotsReady onSuccess);
        
        /// <summary>
        /// Take screenshot from the camPosition looking at pointToLooKAt
        /// </summary>
        /// <param name="camPosition"></param>
        /// <param name="pointToLookAt"></param>
        /// <param name="onSuccess"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        void TakeSceneScreenshot(Vector3 camPosition, Vector3 pointToLookAt, int width, int height, OnSnapshotsReady onSuccess);

        /// <summary>
        /// Take screenshot from the areal position of the scene
        /// </summary>
        /// <param name="onSuccess"></param>
        void TakeSceneAerialScreenshot(IParcelScene parcelScene, OnSnapshotsReady onSuccess);
    }
}