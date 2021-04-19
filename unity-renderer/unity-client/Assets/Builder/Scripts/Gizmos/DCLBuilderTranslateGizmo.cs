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
            entityTransform.position = position;

            return axisValue;
        }
    }
}