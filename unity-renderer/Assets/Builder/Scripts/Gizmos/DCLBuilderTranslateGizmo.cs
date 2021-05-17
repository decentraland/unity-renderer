using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderTranslateGizmo : DCLBuilderGizmo
    {
        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo) { snapFactor = snapInfo.position; }

        public override float TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            Vector3 initialEntityPosition = entityTransform.position;
            if (snapFactor > 0)
                initialEntityPosition = GetPositionRoundedToSnapFactor(entityTransform.position, axisValue);

            Vector3 move = activeAxis.transform.forward * axisValue;
            Vector3 position = initialEntityPosition + move;

            entityTransform.position = position;

            return axisValue;
        }

        private Vector3 GetPositionRoundedToSnapFactor(Vector3 position, float axisValue)
        {
            Vector3 activeAxisVector = GetActiveAxisVector();

            position = new Vector3(
                activeAxisVector == Vector3.right ? (axisValue >= 0 ? Mathf.Floor(position.x / snapFactor) : Mathf.Ceil(position.x / snapFactor)) * snapFactor : position.x,
                activeAxisVector == Vector3.up ? (axisValue >= 0 ? Mathf.Floor(position.y / snapFactor) : Mathf.Ceil(position.y / snapFactor)) * snapFactor : position.y,
                activeAxisVector == Vector3.back ? (axisValue >= 0 ? Mathf.Floor(position.z / snapFactor) : Mathf.Ceil(position.z / snapFactor)) * snapFactor : position.z);

            return position;
        }
    }
}