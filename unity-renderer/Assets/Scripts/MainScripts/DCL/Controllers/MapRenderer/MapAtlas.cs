using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class MapAtlas : MonoBehaviour
    {
        public RectTransform viewport;
        public GameObject container;
        public RectTransform chunksParent;
        public GameObject overlayLayerGameobject;

        public void Cleanup() { }

        public void CenterToTile(Vector2 tilePosition)
        {
            if (viewport == null)
                return;

            Vector3 center = viewport.transform.TransformPoint(viewport.rect.center);
            Vector3 delta = center - container.transform.TransformPoint(MapUtils.GetTileToLocalPosition(tilePosition.x, tilePosition.y));

            container.transform.position += delta;
            UpdateCulling();
        }

        [ContextMenu("Force Update Culling")]
        public void UpdateCulling() { }

        public void InitializeChunks() { }
    }
}