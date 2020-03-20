using UnityEngine;

namespace DCL.Helpers
{
    public static class MapUtils
    {
        public static readonly Vector2Int WORLD_PARCELS_OFFSET_MIN = new Vector2Int(-150, -150);
        public static readonly Vector2Int WORLD_PARCELS_OFFSET_MAX = new Vector2Int(175, 175); //NOTE(Brian): We use 175 instead of 150 to make the chunks look even.
        public static readonly Vector2Int CHUNK_SIZE = new Vector2Int(1020, 1020);
        public static readonly int PARCEL_SIZE = 20;
        public static readonly Vector3 CENTER_OFFSET = new Vector3(-10.0f, -10.0f);//new Vector3(1, -0.5f);

        public static Vector2Int GetTileFromLocalPosition(Vector3 position)
        {
            return new Vector2Int((int)(position.x / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.x, (int)(position.y / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.y);
        }

        public static Vector3 GetTileToLocalPosition(float x, float y)
        {
            x -= WORLD_PARCELS_OFFSET_MIN.x;
            y -= WORLD_PARCELS_OFFSET_MIN.y;

            Vector3 result = new Vector3(x * PARCEL_SIZE, y * PARCEL_SIZE, 0) + CENTER_OFFSET;
            return result;
        }
    }
}
