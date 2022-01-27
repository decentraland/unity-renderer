using System.Collections.Generic;
using UnityEngine;

namespace DCL.Camera
{
    public interface IFreeCameraMovement
    {
        /// <summary>
        /// GameObject of the camera
        /// </summary>
        GameObject gameObject { get; }
        
        /// <summary>
        /// This will get the position of the virtual camera
        /// </summary>
        Vector3 GetCameraPosition { get; }
        
        /// <summary>
        /// This will get the forward vector of the virtual camera
        /// </summary>
        Vector3 GetCameraFoward { get; }

        /// <summary>
        /// It will focus the entities in the list and maintain them in the screen
        /// </summary>
        /// <param name="entitiesToFocus"></param>
        void FocusOnEntities(List<BIWEntity> entitiesToFocus);

        /// <summary>
        /// Camera will go smoothly to the position
        /// </summary>
        /// <param name="position"></param>
        void SmoothLookAt(Vector3 position);

        /// <summary>
        /// It will start detecting movement of the camera
        /// </summary>
        void StartDetectingMovement();

        /// <summary>
        /// Stop detecting movement of the camera
        /// </summary>
        void StopDetectingMovement();

        /// <summary>
        /// True is has been movement between start and stop detecting movement
        /// </summary>
        /// <returns></returns>
        bool HasBeenMovement();

        /// <summary>
        /// Enable the camera movement
        /// </summary>
        /// <param name="canMove"></param>
        void SetCameraCanMove(bool canMove);

        /// <summary>
        /// Set the position of the camera
        /// </summary>
        /// <param name="position"></param>
        void SetPosition(Vector3 position);

        /// <summary>
        /// Look at the transform
        /// </summary>
        /// <param name="transformToLookAt"></param>
        void LookAt(Transform transformToLookAt);

        /// <summary>
        /// Look at the point
        /// </summary>
        /// <param name="pointToLookAt"></param>
        void LookAt(Vector3 pointToLookAt);

        /// <summary>
        /// Set the initial configuration to apply you reset the camera
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        void SetResetConfiguration(Vector3 position, Transform lookAt);

        /// <summary>
        /// Set the initial configuration to apply you reset the camera
        /// </summary>
        /// <param name="position"></param>
        /// <param name="lookAt"></param>
        void SetResetConfiguration(Vector3 position, Vector3 pointToLook);

        /// <summary>
        /// Reset the camera to the initial state
        /// </summary>
        void ResetCameraPosition();
    }
}