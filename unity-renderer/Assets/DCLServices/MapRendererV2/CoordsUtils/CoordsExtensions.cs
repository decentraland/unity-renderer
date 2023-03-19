using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal static class CoordsExtensions
    {
        public static Vector3 PivotPosition(this ICoordsUtils coordsUtils, IMapRendererMarker marker, Vector3 objPosition)
        {
            Vector2 obj2dPos = objPosition;
            obj2dPos -= (Vector2.one - marker.Pivot) * coordsUtils.ParcelSize;
            return new Vector3(obj2dPos.x, obj2dPos.y, objPosition.z);
        }

        /// <summary>
        /// Translates Coords into position and makes an offset according to the object's pivot
        /// </summary>
        public static Vector3 CoordsToPosition(this ICoordsUtils coordsUtils, Vector2Int coords, IMapRendererMarker marker) =>
            coordsUtils.PivotPosition(marker, coordsUtils.CoordsToPosition(coords));

        public static void SetObjectScale(this ICoordsUtils coordsUtils, MonoBehaviour obj)
        {
            var parcelSize = coordsUtils.ParcelSize;
            obj.transform.localScale = new Vector3(parcelSize, parcelSize, 1);
        }
    }
}
