using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MapAtlas : MonoBehaviour
    {
        private static readonly int MOUSE_POSITION_ID = Shader.PropertyToID("_MousePosition");
        private static readonly int GRID_THICKNESS_ID = Shader.PropertyToID("_GridThickness");

        public RectTransform viewport;
        public GameObject container;
        public RectTransform chunksParent;
        public GameObject overlayLayerGameobject;
        [SerializeField] private Image mapRenderer;

        private void Awake() { SetHighlightedScene(null); }

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

        public void SetGridThickness(float zoom)
        {
            if(zoom > 0.19f && zoom < 0.21f)
            {
                mapRenderer.material.SetFloat(GRID_THICKNESS_ID, 0.55f);
            }
            else if (zoom > 0.27f && zoom < 0.29f)
            {
                mapRenderer.material.SetFloat(GRID_THICKNESS_ID, 0.5f);
            }
            else if (zoom > 0.39f && zoom < 0.41f)
            {
                mapRenderer.material.SetFloat(GRID_THICKNESS_ID, 0.35f);
            }
            else if (zoom > 0.54f && zoom < 0.56f)
            {
                mapRenderer.material.SetFloat(GRID_THICKNESS_ID, 0.25f);
            }
            else if (zoom > 0.79f && zoom < 0.81f)
            {
                mapRenderer.material.SetFloat(GRID_THICKNESS_ID, 0.2f);
            }

            mapRenderer.RecalculateMasking();
        }
    }
}