using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderTranslateGizmo : DCLBuilderGizmo
    {
        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo) { snapFactor = snapInfo.position; }

        public override float TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            Vector3 move = activeAxis.transform.forward * axisValue;
            Vector3 position = entityTransform.position + move;

            if (snapFactor > 0)
                position = GetPositionRoundedToSnapFactor(position);

            entityTransform.position = position;

            return axisValue;
        }

        private Vector3 GetPositionRoundedToSnapFactor(Vector3 position)
        {
            Vector3 activeAxisVector = GetActiveAxisVector();

            position = new Vector3(
                activeAxisVector == Vector3.right ? Mathf.Round(position.x / snapFactor) * snapFactor : position.x,
                activeAxisVector == Vector3.up ? Mathf.Round(position.y / snapFactor) * snapFactor : position.y,
                activeAxisVector == Vector3.back ? Mathf.Round(position.z / snapFactor) * snapFactor : position.z);

            return position;
        }
    }
}