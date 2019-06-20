using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGizmoAxis : GizmoAxis
{

    public float rotateFactor = 100;

    public override void UpdateTransformation(Vector3 pointerPosition, GameObject selectedObject)
    {
        if (originPointerPosition == Vector3.zero)
        {
            originPointerPosition = pointerPosition;
        }
        if (selectedObject != null)
        {
            selectedObject.transform.Rotate(axis,
                                            ((originPointerPosition.x - pointerPosition.x) + (originPointerPosition.y - pointerPosition.y) + (originPointerPosition.z - pointerPosition.z)) * rotateFactor,
                                            Space.World);
        }
        originPointerPosition = pointerPosition;
    }
}
