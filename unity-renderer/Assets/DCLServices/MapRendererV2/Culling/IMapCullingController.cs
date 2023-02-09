namespace DCLServices.MapRendererV2.Culling
{
    public interface IMapCullingController
    {
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
