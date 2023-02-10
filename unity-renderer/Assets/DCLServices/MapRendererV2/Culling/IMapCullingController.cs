using DCLServices.MapRendererV2.MapCameraController;

namespace DCLServices.MapRendererV2.Culling
{
    internal interface IMapCullingController
    {
        void OnCameraAdded(IMapCameraControllerInternal cameraController);

        void OnCameraRemoved(IMapCameraControllerInternal cameraController);

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
        /// <returns>True if <param name="obj"></param> is visible</returns>
        bool StartTracking<T>(T obj, IMapCullingListener<T> listener) where T: IMapPositionProvider;

        /// <summary>
        /// Stops tracking visibility of the object represented by a single position.
        /// Does nothing if the object is not tracked
        /// </summary>
        /// <param name="obj">Object to stop tracking</param>
        void StopTracking<T>(T obj) where T: IMapPositionProvider;
    }
}
