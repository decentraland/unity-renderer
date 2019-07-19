using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGizmoAxis : GizmoAxis
{
    public Camera cam;
    public float rotateFactor = 20;

    public override void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, RaycastHit hitInfo)
    {
        if (originPointerPosition == Vector3.zero)
        {
            originPointerPosition = inputPosition;
        }
        if (selectedObject != null)
        {
            Vector3 rotationAngleEuler = new Vector3(axis.x, axis.y, axis.z * cam.transform.rotation.z);
            Vector3 pointerDelta = originPointerPosition - inputPosition;
            float finalRotationFactor = (pointerDelta.x + pointerDelta.y + pointerDelta.z) * rotateFactor;

            selectedObject.transform.Rotate(rotationAngleEuler,
                                                finalRotationFactor,
                                                Space.World);
        }
        originPointerPosition = inputPosition;
    }
}
