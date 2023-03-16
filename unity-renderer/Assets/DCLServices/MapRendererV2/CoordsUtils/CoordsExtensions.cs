using UnityEngine;

namespace DCLServices.MapRendererV2.CoordsUtils
{
    internal static class CoordsExtensions
    {
        public static void SetObjectScale(this ICoordsUtils coordsUtils, MonoBehaviour obj)
        {
            var parcelSize = coordsUtils.ParcelSize;
            obj.transform.localScale = new Vector3(parcelSize, parcelSize, 1);
        }
    }
}
