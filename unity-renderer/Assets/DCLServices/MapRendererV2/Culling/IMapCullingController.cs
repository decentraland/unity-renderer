using DCLServices.MapRendererV2.MapCameraController;
using System;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2.Culling
{
    internal interface IMapCullingController : IDisposable
    {
        IReadOnlyDictionary<IMapPositionProvider, MapCullingController.TrackedState> TrackedObjects { get; }

        IReadOnlyList<CameraState> CameraStates { get; }

        /// <summary>
        /// Adds a camera to the controller
        /// </summary>
        /// <param name="cameraController"></param>
        void OnCameraAdded(IMapCameraControllerInternal cameraController);

        /// <summary>
        /// Removes a camera from the controller.
        /// Does nothing if the camera is not tracked
        /// </summary>
        /// <param name="cameraController"></param>
        void OnCameraRemoved(IMapCameraControllerInternal cameraController);

        /// <summary>
        /// Marks camera as dirty. <see cref="OnCameraAdded"/> must have been called for the camera before.
        /// </summary>
        /// <param name="cameraController"></param>
        void SetCameraDirty(IMapCameraControllerInternal cameraController);

        /// <summary>
        /// Marks position of the object as dirty. <see cref="StartTracking{T}"/> must have been called for the object before.
        /// </summary>
        void SetTrackedObjectPositionDirty<T>(T obj) where T: IMapPositionProvider;

        /// <summary>
        /// Starts tracking visibility of the object represented by a single position.
        /// Does nothing if the object is already tracked
        /// </summary>
        /// <param name="obj">Object to track</param>
        /// <param name="listener">Controller - listener of the culling change</param>
        void StartTracking<T>(T obj, IMapCullingListener<T> listener) where T: IMapPositionProvider;

        /// <summary>
        /// Stops tracking visibility of the object represented by a single position.
        /// Does nothing if the object is not tracked
        /// </summary>
        /// <param name="obj">Object to stop tracking</param>
        void StopTracking<T>(T obj) where T: IMapPositionProvider;
    }
}
