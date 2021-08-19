using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWFloorLoading : MonoBehaviour
{
    private Camera builderCamera = null;

    private const float RELATIVE_SCALE_RATIO = 0.032f;

    public void Initialize(Camera camera) { builderCamera = camera; }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + builderCamera.transform.rotation * Vector3.forward,
            builderCamera.transform.rotation * Vector3.up);

        float dist = GetCameraPlaneDistance(builderCamera, transform.position);
        transform.localScale = new Vector3(RELATIVE_SCALE_RATIO * dist, RELATIVE_SCALE_RATIO * dist, RELATIVE_SCALE_RATIO * dist);
    }

    private static float GetCameraPlaneDistance(Camera camera, Vector3 objectPosition)
    {
        Plane plane = new Plane(camera.transform.forward, camera.transform.position);
        return plane.GetDistanceToPoint(objectPosition);
    }

}