using System;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers.ParcelSceneDebug
{
    public class SceneDebugPlane : IDisposable
    {
        GameObject[] sceneParcelPlanes;

        public SceneDebugPlane(LoadParcelScenesMessage.UnityParcelScene sceneData, Transform parent)
        {
            int sceneDataParcelsLength = sceneData.parcels.Length;
            sceneParcelPlanes = new GameObject[sceneDataParcelsLength];

            for (int j = 0; j < sceneDataParcelsLength; j++)
            {
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                sceneParcelPlanes[j] = plane;

                UnityEngine.Object.Destroy(plane.GetComponent<MeshCollider>());

                plane.name = $"parcel:{sceneData.parcels[j].x},{sceneData.parcels[j].y}";

                plane.transform.SetParent(parent);

                // the plane mesh with scale 1 occupies a 10 units space
                plane.transform.localScale = new Vector3(ParcelSettings.PARCEL_SIZE * 0.1f, 1f,
                    ParcelSettings.PARCEL_SIZE * 0.1f);

                Vector3 position = Utils.GridToWorldPosition(sceneData.parcels[j].x, sceneData.parcels[j].y);
                // SET TO A POSITION RELATIVE TO basePosition

                position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT,
                    position.z + ParcelSettings.PARCEL_SIZE / 2);

                plane.transform.position = PositionUtils.WorldToUnityPosition(position);

                plane.GetComponent<MeshRenderer>().sharedMaterial = Utils.EnsureResourcesMaterial("Materials/DefaultPlane");
            }
        }

        public void Dispose()
        {
            if (sceneParcelPlanes == null)
            {
                return;
            }

            for (int i = 0; i < sceneParcelPlanes.Length; i++)
            {
                UnityEngine.Object.Destroy(sceneParcelPlanes[i]);
            }

            sceneParcelPlanes = null;
        }
    }
}
