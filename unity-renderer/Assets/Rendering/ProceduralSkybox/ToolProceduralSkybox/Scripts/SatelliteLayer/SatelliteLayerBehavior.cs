using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SatelliteLayerBehavior : MonoBehaviour
{
    public GameObject satelliteOrbit;
    public GameObject satellite;
    public float satelliteSize = 1;
    public float radius = 10;
    public float thickness = 1;
    [Range(0, 360)]
    public float initialAngle = 0;
    [Range(0, 180)]
    public float horizonPlaneRotation = 0;
    [Range(0, 180)]
    public float inclination = 0;

    public bool rotate;
    public float speed;
    public float currentAngle;

    private void Update()
    {
        // Start rotating
        if (!rotate)
        {
            return;
        }
        currentAngle += speed;

        if (currentAngle > 360)
        {
            currentAngle = 0;
        }
        initialAngle = currentAngle;
        satellite.transform.localPosition = GetSatellitePosition(radius, currentAngle);
    }

    private void OnValidate()
    {
        if (satellite == null || satelliteOrbit == null)
        {
            return;
        }
        radius = Mathf.Clamp(radius, 0, radius);

        // Shift satellite to radius distance in the y direction of the orbit
        if (!rotate)
        {
            currentAngle = initialAngle;
            satellite.transform.localPosition = GetSatellitePosition(radius, initialAngle);
        }

        //  Rotate orbit plane along horizon line
        Vector3 rot = satelliteOrbit.transform.localRotation.eulerAngles;
        rot.z = 0;
        rot.y = horizonPlaneRotation;
        //  Rotate orbit plane along inclination line
        rot.x = inclination;
        satelliteOrbit.transform.localRotation = Quaternion.Euler(rot);

        // change satellite size
        satellite.transform.localScale = Vector3.one * satelliteSize;
    }

    Vector3 GetSatellitePosition(float radius, float angle)
    {

        angle = Mathf.Clamp(angle, 0, 360);
        float angleEdited = (90 - angle) * Mathf.Deg2Rad;
        float x = radius * Mathf.Cos(angleEdited);
        float y = radius * Mathf.Sin(angleEdited);
        return new Vector3(x, y, 0);
    }

    private void OnDrawGizmos()
    {
        var tr = transform;
        var position = tr.position;
        Handles.color = Color.gray;
        Handles.DrawWireDisc(position, Vector3.up, radius, thickness);

        if (satelliteOrbit == null)
        {
            return;
        }
        // Draw wire disc of green color for orbit orthogonal to y = 1
        Handles.color = Color.green;
        Handles.DrawWireDisc(position, satelliteOrbit.transform.forward, radius, thickness);
        //Handles.DrawWireArc(position, -satelliteOrbit.transform.forward, -transform.right, initialAngle, radius);
    }

}

//// Displays circles of various thickness in the scene view
//[CustomEditor(typeof(SatelliteLayerBehavior))]
//public class ExampleEditor : Editor
//{
//    public void OnSceneGUI()
//    {
//        var t = target as SatelliteLayerBehavior;
//        var tr = t.transform;
//        var position = tr.position;
//        Handles.color = Color.gray;
//        Handles.DrawWireDisc(position, Vector3.up, t.radius, t.thickness);

//        if (t.satelliteOrbit == null)
//        {
//            return;
//        }
//        // Draw wire disc of green color for orbit orthogonal to y = 1
//        Handles.color = Color.green;
//        Handles.DrawWireDisc(position, t.satelliteOrbit.transform.forward, t.radius, t.thickness);
//    }
//}