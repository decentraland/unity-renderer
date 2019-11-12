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
                Vector3 position = selectedObject.transform.position;
                if (axis.x == 1)
                {
                    position.x = hitPoint.x + difV.x;
                    if (snapFactor > 0)
                    {
                        position.x = position.x - (position.x % snapFactor);
                    }
                }
                if (axis.z == 1)
                {
                    position.z = hitPoint.z + difV.z;
                    if (snapFactor > 0)
                    {
                        position.z = position.z - (position.z % snapFactor);
                    }
                }

                selectedObject.transform.position = position;

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

        public override void SetSnapFactor(float position, float rotation, float scale)
        {
            snapFactor = position;
        }

    }
}