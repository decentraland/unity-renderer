/*
* Scene utils file for type and unit convertion
*/

using DCL.Configuration;
using UnityEngine;

public static partial class UtilsScene
{
    public static Vector3 GlobalToScenePosition(ref Vector2Int parcelBasePosition, ref Vector3 position, ref Vector3 worldOffset)
    {
        return new Vector3(
            x: (position.x + worldOffset.x) - (parcelBasePosition.x * ParcelSettings.PARCEL_SIZE),
            y: (position.y + worldOffset.y),
            z: (position.z + worldOffset.z) - (parcelBasePosition.y * ParcelSettings.PARCEL_SIZE)
        );
    }
}
