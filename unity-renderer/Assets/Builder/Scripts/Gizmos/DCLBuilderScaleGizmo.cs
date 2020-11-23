using UnityEngine;

namespace Builder.Gizmos
{
    public class DCLBuilderScaleGizmo : DCLBuilderGizmo
    {
        const float MINIMUN_SCALE_ALLOWED = 0.01f;

        [SerializeField] DCLBuilderGizmoAxis axisProportionalScale = null;

        private Vector2 lastMousePosition;
        private Vector2 initialMousePosition;
        private Vector3 initialHitPoint;
        private Vector3 lastHitPoint;

        public override void Initialize(Camera camera, Transform cameraTransform)
        {
            base.Initialize(camera, cameraTransform);
            axisProportionalScale.SetGizmo(this);
        }

        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo)
        {
            snapFactor = snapInfo.scale;
        }

        public override void TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            Vector3 scaleDirection = activeAxis.transform.forward;
            if (axis == axisProportionalScale)
            {
                scaleDirection = Vector3.one;
                float inputDirection = lastMousePosition.y - initialMousePosition.y;
                if (inputDirection < 0)
                {
                    scaleDirection = -Vector3.one;
                }
                initialMousePosition = lastMousePosition;
                initialHitPoint = lastHitPoint;
            }
            else if (worldOrientedGizmos)
            {
                scaleDirection = entityTransform.rotation * activeAxis.transform.forward;
                scaleDirection.x = Mathf.Abs(scaleDirection.x);
                scaleDirection.y = Mathf.Abs(scaleDirection.y);
                scaleDirection.z = Mathf.Abs(scaleDirection.z);
            }

            Vector3 newScale = entityTransform.localScale + scaleDirection * axisValue;

            if (Mathf.Abs(newScale.x) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED)
            {
                newScale += scaleDirection * MINIMUN_SCALE_ALLOWED;
            }

            entityTransform.localScale = newScale;
        }

        protected override void SetPreviousAxisValue(float axisValue, float transformValue)
        {
            if (activeAxis == axisProportionalScale)
            {
                prevAxisValue = 0;
            }
            else
            {
                prevAxisValue = axisValue;
            }
        }

        protected override float GetHitPointToAxisValue(DCLBuilderGizmoAxis axis, Vector3 hitPoint, Vector2 mousePosition)
        {
            if (axis == axisProportionalScale)
            {
                if (startDragging)
                {
                    initialMousePosition = mousePosition;
                    initialHitPoint = hitPoint;
                }
                lastMousePosition = mousePosition;
                lastHitPoint = hitPoint;
                return Vector3.Distance(initialHitPoint, hitPoint);
            }
            return axis.transform.InverseTransformPoint(hitPoint).z;
        }
    }
}