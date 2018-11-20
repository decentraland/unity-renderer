using UnityEngine;
using DCL.Configuration;

namespace DCL.Helpers {
  public static class LandHelpers {
    public static Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition) {
      return new Vector3(
        x: xGridPosition * ParcelSettings.PARCEL_SIZE,
        y: 0f,
        z: yGridPosition * ParcelSettings.PARCEL_SIZE
      );
    }
  }
}
