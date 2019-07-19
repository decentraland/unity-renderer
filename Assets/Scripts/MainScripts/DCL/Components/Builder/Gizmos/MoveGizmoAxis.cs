using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGizmoAxis : GizmoAxis
{
    LayerMask groundMask;

    public float moveFactor = 100;

    Vector3 difV;


    public override void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, RaycastHit hitInfo)
    {
        groundMask = LayerMask.GetMask("Ground");

        if (originPointerPosition == Vector3.zero)
        {
            originPointerPosition = pointerPosition;
            difV = selectedObject.transform.position - hitInfo.point;
        }

        if (selectedObject != null)
        {
            selectedObject.transform.position = new Vector3(axis.x == 1 ? hitInfo.point.x + difV.x : selectedObject.transform.position.x,
                                                            selectedObject.transform.position.y,
                                                            axis.z == 1 ? hitInfo.point.z + difV.z : selectedObject.transform.position.z);
            if (axis.y == 1)
            {
                Vector3 posDif = originPointerPosition - pointerPosition;
                posDif.Scale(axis);
                posDif *= moveFactor;
                Vector3 newPos = selectedObject.transform.position - posDif;
                selectedObject.transform.position = newPos;
            }

        }
        originPointerPosition = pointerPosition;
    }


}
