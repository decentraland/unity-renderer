using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGizmoAxis : GizmoAxis
{

    public float moveFactor = 100;

    public override void UpdateTransformation(Vector3 pointerPosition, GameObject selectedObject)
    {
        if (originPointerPosition == Vector3.zero)
        {
            originPointerPosition = pointerPosition;
        }
        Vector3 posDif = originPointerPosition - pointerPosition;
        posDif.Scale(axis);
        posDif *= moveFactor;
        Vector3 newPos = selectedObject.transform.position - posDif;
        selectedObject.transform.position = newPos;
        originPointerPosition = pointerPosition;
    }
}
