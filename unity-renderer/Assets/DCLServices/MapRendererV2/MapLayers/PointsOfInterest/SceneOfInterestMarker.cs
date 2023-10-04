using DCLServices.MapRendererV2.CommonBehavior;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.PointsOfInterest
{
    internal class SceneOfInterestMarker : ISceneOfInterestMarker
    {
        internal const int MAX_TITLE_LENGTH = 29;

        public Vector3 CurrentPosition => poolableBehavior.currentPosition;

        public bool IsVisible => poolableBehavior.isVisible;

        public Vector2 Pivot => poolableBehavior.objectsPool.Prefab.pivot;

        internal string title { get; private set; }

        private MapMarkerPoolableBehavior<SceneOfInterestMarkerObject> poolableBehavior;

        private readonly IMapCullingController cullingController;

        public SceneOfInterestMarker(IUnityObjectPool<SceneOfInterestMarkerObject> objectsPool, IMapCullingController cullingController)
        {
            poolableBehavior = new MapMarkerPoolableBehavior<SceneOfInterestMarkerObject>(objectsPool);
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

        private bool isInit;
        private float baseZoom;
        private float baseScale;

        public void SetZoom(float zoom)
        {
            if (!isInit)
            {
                baseZoom = zoom;
                if(poolableBehavior.instance != null)
                    baseScale = poolableBehavior.instance.transform.localScale.x;
                else
                {
                    baseScale = 20;
                }
                isInit = true;

                Debug.Log($" base Scale {baseScale}");
            }
            else
            {
                float newScale = Math.Max(zoom/baseZoom * baseScale, baseScale);
                Debug.Log($" new Scale {newScale}");

                if(poolableBehavior.instance != null)
                poolableBehavior.instance.SetScale(newScale);
            }
        }

        public void ResetToBaseScale()
        {
            if(poolableBehavior.instance != null)
                poolableBehavior.instance.SetScale(baseScale);
        }

        public void Dispose()
        {
            OnBecameInvisible();
            cullingController.StopTracking(this);
        }
    }
}
