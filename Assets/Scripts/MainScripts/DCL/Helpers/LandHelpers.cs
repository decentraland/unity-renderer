using UnityEngine;
using DCL.Configuration;

namespace DCL.Helpers {
  public static class LandHelpers {

    /**
     * Transforms a grid position into a world-relative 3d position
     */
    public static Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition) {
      return new Vector3(
        x: xGridPosition * ParcelSettings.PARCEL_SIZE,
        y: 0f,
        z: yGridPosition * ParcelSettings.PARCEL_SIZE
      );
    }


    /**
     * Transforms a world position into a grid position
     */
    public static Vector2 worldToGrid(Vector3 vector) {
      return new Vector2(
        Mathf.Floor(vector.x / ParcelSettings.PARCEL_SIZE),
        Mathf.Floor(vector.z / ParcelSettings.PARCEL_SIZE)
      );
    }
  }
}
