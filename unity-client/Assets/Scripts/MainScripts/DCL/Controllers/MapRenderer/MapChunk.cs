using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MapChunk : MonoBehaviour
    {
        private bool VERBOSE = false;
        const string MAP_API_BASE = "https://api.decentraland.org/v1/map.png";

        public RawImage targetImage;

        [System.NonSerialized]
        public Vector2Int center;
        [System.NonSerialized]
        public Vector2Int size;
        [System.NonSerialized]
        public int tileSize;
        [System.NonSerialized]
        public MapAtlas owner;
        protected RectTransform rt;
        protected bool isLoadingOrLoaded = false;

        private void Start()
        {
            targetImage.color = Color.clear;
        }

        public virtual IEnumerator LoadChunkImage()
        {
            if (isLoadingOrLoaded)
                yield break;

            isLoadingOrLoaded = true;

            string url = $"{MAP_API_BASE}?center={center.x},{center.y}&width={size.x}&height={size.y}&size={tileSize}";

            Texture result = null;

            yield return Utils.FetchTexture(url, (x) => result = x);

            result.filterMode = FilterMode.Trilinear;
            result.wrapMode = TextureWrapMode.Clamp;
            result.anisoLevel = 16;
            ((Texture2D)result).Apply(true, true);

            targetImage.texture = result;
            targetImage.SetNativeSize();
            targetImage.color = Color.white;
        }

        public void UpdateCulling()
        {
            if (owner == null)
                return;

            if (rt == null)
                rt = transform as RectTransform;

            Vector3 myMinCoords = rt.TransformPoint(new Vector3(rt.rect.xMin, rt.rect.yMin));
            Vector3 myMaxCoords = rt.TransformPoint(new Vector3(rt.rect.xMax, rt.rect.yMax));

            Vector3 viewMinCoords = owner.viewport.TransformPoint(new Vector3(owner.viewport.rect.xMin, owner.viewport.rect.yMin));
            Vector3 viewMaxCoords = owner.viewport.TransformPoint(new Vector3(owner.viewport.rect.xMax, owner.viewport.rect.yMax));

#if UNITY_EDITOR
            if (VERBOSE)
            {
                var rtWorldRect = new Rect(myMinCoords.x, myMinCoords.y, myMaxCoords.x - myMinCoords.x, myMaxCoords.y - myMinCoords.y);
                Utils.DrawRectGizmo(rtWorldRect, Color.red, 5f);
            }
#endif
            float size = (viewMaxCoords - viewMinCoords).magnitude;

            Rect viewportRect = new Rect(viewMinCoords, viewMaxCoords - viewMinCoords);
            viewportRect.min -= Vector2.one * size;
            viewportRect.max += Vector2.one * size;

#if UNITY_EDITOR
            if (VERBOSE)
            {
                Utils.DrawRectGizmo(viewportRect, Color.blue, 5f);
            }
#endif

            Rect myRect = new Rect(myMinCoords, myMaxCoords - myMinCoords);
            bool visible = viewportRect.Overlaps(myRect, true);

            targetImage.enabled = visible;

            if (!isLoadingOrLoaded && visible)
                CoroutineStarter.Start(LoadChunkImage());
        }
    }
}
