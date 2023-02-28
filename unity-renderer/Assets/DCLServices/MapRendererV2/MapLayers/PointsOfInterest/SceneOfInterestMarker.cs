using DCLServices.MapRendererV2.Culling;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal class SceneOfInterestMarker : ISceneOfInterestMarker
    {
        internal const int MAX_TITLE_LENGTH = 29;

        public Vector3 CurrentPosition { get; private set; }

        private readonly IObjectPool<SceneOfInterestMarkerObject> objectsPool;
        private readonly IMapCullingController cullingController;

        private string title;

        private SceneOfInterestMarkerObject instance;

        public SceneOfInterestMarker(IObjectPool<SceneOfInterestMarkerObject> objectsPool, IMapCullingController cullingController)
        {
            this.objectsPool = objectsPool;
            this.cullingController = cullingController;
        }

        public void SetData(string title, Vector3 position)
        {
            CurrentPosition = position;
            this.title = title.Length > MAX_TITLE_LENGTH ? title.Substring(0, MAX_TITLE_LENGTH) : title;
        }

        public void OnBecameVisible()
        {
            instance = objectsPool.Get();
            instance.title.text = title;
        }

        public void OnBecameInvisible()
        {
            if (instance)
            {
                objectsPool.Release(instance);
                instance = null;
            }
        }

        public void Dispose()
        {
            OnBecameInvisible();
            cullingController.StopTracking(this);
        }
    }
}
