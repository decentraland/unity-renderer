using UnityEngine;

namespace DCL.Helpers
{
    public static class MapUtils
    {
        public static readonly Vector2Int WORLD_PARCELS_OFFSET_MIN = new Vector2Int(-150, -150);
        public static readonly Vector2Int WORLD_PARCELS_OFFSET_MAX = new Vector2Int(175, 175); //NOTE(Brian): We use 175 instead of 150 to make the chunks look even.
        public static readonly Vector2Int CHUNK_SIZE = new Vector2Int(1020, 1020);
        public static readonly int PARCEL_SIZE = 20;

        public static Vector2Int GetTileFromLocalPosition(Vector3 position)
        {
            return new Vector2Int((int)(position.x / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.x, (int)(position.y / PARCEL_SIZE) + WORLD_PARCELS_OFFSET_MIN.y);
        }

        public static Vector3 GetTileToLocalPosition(float x, float y)
        {
            x -= WORLD_PARCELS_OFFSET_MIN.x;
            y -= WORLD_PARCELS_OFFSET_MIN.y;

            Vector3 result = new Vector3(x * PARCEL_SIZE, y * PARCEL_SIZE, 0);
            return result;
        }

        public static string GetMarketPlaceThumbnailUrl(Vector2Int[] parcels, int width, int height, int sizeFactor)
        {
            string parcelsStr = "";
            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            Vector2Int coord;

            for (int i = 0; i < parcels.Length; i++)
            {
                coord = parcels[i];
                parcelsStr += string.Format("{0},{1}", coord.x, coord.y);
                if (i < parcels.Length - 1) parcelsStr += ";";

                if (coord.x < min.x) min.x = coord.x;
                if (coord.y < min.y) min.y = coord.y;
                if (coord.x > max.x) max.x = coord.x;
                if (coord.y > max.y) max.y = coord.y;
            }

            int centerX = (int)(min.x + (max.x - min.x) * 0.5f);
            int centerY = (int)(min.y + (max.y - min.y) * 0.5f);
            int sceneMaxSize = Mathf.Clamp(Mathf.Max(max.x - min.x, max.y - min.y), 1, int.MaxValue);
            int size = sizeFactor / sceneMaxSize;

            return $"https://api.decentraland.org/v1/map.png?width={width}&height={height}&size={size}&center={centerX},{centerY}&selected={parcelsStr}";
        }
    }
}
