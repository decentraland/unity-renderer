using UnityEngine;

namespace Builder
{
    public class RotateGizmoAxis : GizmoAxis
    {
        public Camera builderCamera;
        public float rotateFactor = 20;

        public override void UpdateTransformation(Vector3 inputPosition, Vector3 pointerPosition, GameObject selectedObject, Vector3 hitPoint)
        {
            if (originPointerPosition == Vector3.zero)
            {
                originPointerPosition = inputPosition;
            }
            if (selectedObject != null)
            {
                Vector3 rotationAngleEuler = new Vector3(axis.x, axis.y, axis.z * builderCamera.transform.rotation.z);
                Vector3 pointerDelta = originPointerPosition - inputPosition;
                float finalRotationFactor = (pointerDelta.x + pointerDelta.y + pointerDelta.z) * rotateFactor;

                selectedObject.transform.Rotate(rotationAngleEuler,
                                                    finalRotationFactor,
                                                    Space.World);
            }
            originPointerPosition = inputPosition;
        }

        public override void SetSnapFactor(float position, float rotation, float scale)
        {
            snapFactor = rotation;
        }
    }
}