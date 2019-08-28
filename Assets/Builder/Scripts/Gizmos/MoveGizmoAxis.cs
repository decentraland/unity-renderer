using UnityEngine;

namespace Builder
{
    public class MoveGizmoAxis : GizmoAxis
    {
        public float moveFactor = 100;

        Vector3 difV;


        public override void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, Vector3 hitPoint)
        {
            if (originPointerPosition == Vector3.zero)
            {
                originPointerPosition = pointerPosition;
                difV = selectedObject.transform.position - hitPoint;
            }

            if (selectedObject != null)
            {
                selectedObject.transform.position = new Vector3(axis.x == 1 ? hitPoint.x + difV.x : selectedObject.transform.position.x,
                                                                selectedObject.transform.position.y,
                                                                axis.z == 1 ? hitPoint.z + difV.z : selectedObject.transform.position.z);
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
}