using DCL.Configuration;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

/*
* Scene utils file for scene bounds related stuff
*/

public static partial class UtilsScene
{
    public static Bounds CalculateOuterBounds(IReadOnlyList<Vector2Int> parcels, Vector3 baseParcelWorldPos)
    {
        Bounds outerBounds = new Bounds();
        outerBounds.SetMinMax(new Vector3(baseParcelWorldPos.x, 0f, baseParcelWorldPos.z),
            new Vector3(baseParcelWorldPos.x + ParcelSettings.PARCEL_SIZE, 0f, baseParcelWorldPos.z + ParcelSettings.PARCEL_SIZE));

        for (int i = 0; i < parcels.Count; i++)
        {
            // Update outer bounds with parcel's size
            var parcel = parcels[i];

            Vector3 parcelWorldPos = PositionUtils.WorldToUnityPosition(Utils.GridToWorldPosition(parcel.x, parcel.y));
            outerBounds.Encapsulate(new Vector3(parcelWorldPos.x, 0, parcelWorldPos.z));
            outerBounds.Encapsulate(new Vector3(parcelWorldPos.x + ParcelSettings.PARCEL_SIZE, 0, parcelWorldPos.z + ParcelSettings.PARCEL_SIZE));
        }

        // Apply outer bounds extra threshold
        outerBounds.SetMinMax(new Vector3(outerBounds.min.x - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD, 0f, outerBounds.min.z - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD),
            new Vector3(outerBounds.max.x + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD, 0f, outerBounds.max.z + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD));

        return outerBounds;
    }

    public static bool IsInsideSceneInnerBounds(HashSet<Vector2Int> sceneParcels, float sceneHeightLimit, Vector3 targetWorldPosition, float height = 0f)
    {
        if (sceneParcels.Count == 0)
            return false;

        if (height > sceneHeightLimit)
            return false;

        int noThresholdZCoordinate = Mathf.FloorToInt(targetWorldPosition.z / ParcelSettings.PARCEL_SIZE);
        int noThresholdXCoordinate = Mathf.FloorToInt(targetWorldPosition.x / ParcelSettings.PARCEL_SIZE);

        // We check the target world position
        Vector2Int targetCoordinate = new Vector2Int(noThresholdXCoordinate, noThresholdZCoordinate);

        if (sceneParcels.Contains(targetCoordinate))
            return true;

        // We need to check using a threshold from the target point, in order to cover correctly the parcel "border/edge" positions
        Vector2Int coordinateMin = new Vector2Int();
        coordinateMin.x = Mathf.FloorToInt((targetWorldPosition.x - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
        coordinateMin.y = Mathf.FloorToInt((targetWorldPosition.z - ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

        Vector2Int coordinateMax = new Vector2Int();
        coordinateMax.x = Mathf.FloorToInt((targetWorldPosition.x + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);
        coordinateMax.y = Mathf.FloorToInt((targetWorldPosition.z + ParcelSettings.PARCEL_BOUNDARIES_THRESHOLD) / ParcelSettings.PARCEL_SIZE);

        // We check the east/north-threshold position
        targetCoordinate.Set(coordinateMax.x, coordinateMax.y);

        if (sceneParcels.Contains(targetCoordinate))
            return true;

        // We check the east/south-threshold position
        targetCoordinate.Set(coordinateMax.x, coordinateMin.y);

        if (sceneParcels.Contains(targetCoordinate))
            return true;

        // We check the west/north-threshold position
        targetCoordinate.Set(coordinateMin.x, coordinateMax.y);

        if (sceneParcels.Contains(targetCoordinate))
            return true;

        // We check the west/south-threshold position
        targetCoordinate.Set(coordinateMin.x, coordinateMin.y);

        if (sceneParcels.Contains(targetCoordinate))
            return true;

        return false;
    }

    public static bool IsInsideSceneOuterBounds(Bounds sceneOuterBounds, Vector3 targetUnityPosition)
    {
        targetUnityPosition.y = 0f;
        return sceneOuterBounds.Contains(targetUnityPosition);
    }

    public static bool IsInsideSceneInnerBounds(HashSet<Vector2Int> sceneParcels, float sceneHeightLimit, Bounds targetBounds)
    {
        var worldOffset = CommonScriptableObjects.worldOffset.Get();

        if (!IsInsideSceneInnerBounds(sceneParcels, sceneHeightLimit, targetBounds.min + worldOffset, targetBounds.max.y))
            return false;

        if (!IsInsideSceneInnerBounds(sceneParcels, sceneHeightLimit, targetBounds.max + worldOffset, targetBounds.max.y))
            return false;

        return true;
    }
}
