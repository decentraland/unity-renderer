using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MapAtlas : MonoBehaviour
    {
        private static readonly int MOUSE_POSITION_ID = Shader.PropertyToID("_MousePosition");

        public RectTransform viewport;
        public GameObject container;
        public RectTransform chunksParent;
        public GameObject overlayLayerGameobject;
        [SerializeField] private Image mapRenderer;

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

        public void SetHighlightedScene(Vector2Int? coords)
        {
            if (coords == null)
            {
                mapRenderer.material.SetVector(MOUSE_POSITION_ID, new Vector2(5000, 5000));
                return;
            }
            mapRenderer.material.SetVector(MOUSE_POSITION_ID, Vector2.one * coords.Value);
            mapRenderer.RecalculateMasking();
        }
    }
}