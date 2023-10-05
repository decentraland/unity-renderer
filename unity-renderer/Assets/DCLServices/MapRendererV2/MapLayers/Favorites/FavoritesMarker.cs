using DCLServices.MapRendererV2.CommonBehavior;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.Favorites
{
    internal class FavoritesMarker : IFavoritesMarker
    {
        internal const int MAX_TITLE_LENGTH = 29;

        public Vector3 CurrentPosition => poolableBehavior.currentPosition;

        public bool IsVisible => poolableBehavior.isVisible;

        public Vector2 Pivot => poolableBehavior.objectsPool.Prefab.pivot;

        internal string title { get; private set; }

        private MapMarkerPoolableBehavior<FavoriteMarkerObject> poolableBehavior;

        private readonly IMapCullingController cullingController;

        public FavoritesMarker(IUnityObjectPool<FavoriteMarkerObject> objectsPool, IMapCullingController cullingController)
        {
            poolableBehavior = new MapMarkerPoolableBehavior<FavoriteMarkerObject>(objectsPool);
            this.cullingController = cullingController;
        }

        public void SetData(string title, Vector3 position)
        {
            poolableBehavior.SetCurrentPosition(position);
            this.title = title.Length > MAX_TITLE_LENGTH ? title.Substring(0, MAX_TITLE_LENGTH) : title;
        }

        public void OnBecameVisible()
        {
            poolableBehavior.OnBecameVisible().title.text = title;
        }

        public void OnBecameInvisible()
        {
            poolableBehavior.OnBecameInvisible();
        }

        public void Dispose()
        {
            OnBecameInvisible();
            cullingController.StopTracking(this);
        }
    }
}
