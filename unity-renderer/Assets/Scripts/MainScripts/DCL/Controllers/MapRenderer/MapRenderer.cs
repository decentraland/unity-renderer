using DCL.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace DCL
{
    public class MapRenderer : MonoBehaviour
    {
        public static MapRenderer i { get; private set; }

        private Vector3Variable playerWorldPosition => CommonScriptableObjects.playerWorldPosition;
        private Vector3Variable playerRotation => CommonScriptableObjects.cameraForward;

        public Vector3 playerGridPosition => Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get());

        public MapAtlas atlas;

        public Image playerPositionIcon;

        private void Awake()
        {
            i = this;
        }

        void Start()
        {
            playerWorldPosition.OnChange += OnCharacterMove;
            playerRotation.OnChange += OnCharacterRotate;
        }

        public void OnDestroy()
        {
            playerWorldPosition.OnChange -= OnCharacterMove;
            playerRotation.OnChange -= OnCharacterRotate;
        }

        private void OnCharacterMove(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(current));
        }

        private void OnCharacterRotate(Vector3 current, Vector3 previous)
        {
            UpdateRendering(Utils.WorldToGridPositionUnclamped(playerWorldPosition.Get()));
        }

        public void OnCharacterSetPosition(Vector2Int newCoords, Vector2Int oldCoords)
        {
            UpdateRendering(new Vector2((float)newCoords.x, (float)newCoords.y));
        }

        public void UpdateRendering(Vector2 newCoords)
        {
            UpdateBackgroundLayer(newCoords);
            UpdateSelectionLayer();
            UpdateOverlayLayer();
        }

        void UpdateBackgroundLayer(Vector2 newCoords)
        {
            atlas.CenterToTile(newCoords);
        }

        void UpdateSelectionLayer()
        {
            //TODO(Brian): Build and place here the scene highlight if applicable.
        }

        void UpdateOverlayLayer()
        {
            //NOTE(Brian): Player icon
            Vector3 f = CommonScriptableObjects.cameraForward.Get();
            Quaternion playerAngle = Quaternion.Euler(0, 0, Mathf.Atan2(-f.x, f.z) * Mathf.Rad2Deg);

            var gridPosition = this.playerGridPosition;
            playerPositionIcon.transform.localPosition = MapUtils.GetTileToLocalPosition(gridPosition.x, gridPosition.y);
            playerPositionIcon.transform.rotation = playerAngle;

            //TODO(Brian): Update POI icons, avatar icons, etc.
        }

        public Vector3 GetViewportCenter()
        {
            return atlas.viewport.TransformPoint(atlas.viewport.rect.center);
        }
    }
}
