using System.Collections.Generic;
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
        private Dictionary<Transform, Vector3> entitiesOriginalPositions = new Dictionary<Transform, Vector3>();

        public override void Initialize(Camera camera, Transform cameraTransform)
        {
            base.Initialize(camera, cameraTransform);
            axisProportionalScale.SetGizmo(this);
        }

        public override void SetSnapFactor(DCLBuilderGizmoManager.SnapInfo snapInfo) { snapFactor = snapInfo.scale; }

        public override float TransformEntity(Transform entityTransform, DCLBuilderGizmoAxis axis, float axisValue)
        {
            // In order to avoid to make the scale of each selected entity dependent of the 'entityTransform' parent,
            // we temporally move all entities to the same position as 'entityTransform' before calculate the new scale.
            foreach (Transform entity in entityTransform)
            {
                entitiesOriginalPositions.Add(entity, entity.transform.position);
                entity.transform.position = entityTransform.position;
            }

            Vector3 scaleDirection = GetScaleDirection(entityTransform, axis);
            entityTransform.localScale = GetNewScale(entityTransform, axisValue, scaleDirection);

            // Once the new scale has been calculated, we restore the original positions of all the selected entities.
            using (var enumerator = entitiesOriginalPositions.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Key.position = enumerator.Current.Value;
                }
            }
            entitiesOriginalPositions.Clear();

            return axisValue;
        }

        private Vector3 GetScaleDirection(Transform entityTransform, DCLBuilderGizmoAxis axis)
        {
            Vector3 scaleDirection = activeAxis.transform.forward;
            if (axis == axisProportionalScale)
            {
                scaleDirection = Vector3.one;
                float inputDirection = (lastMousePosition.x - initialMousePosition.x) + (lastMousePosition.y - initialMousePosition.y);
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

            return scaleDirection;
        }

        private static Vector3 GetNewScale(Transform entityTransform, float axisValue, Vector3 scaleDirection)
        {
            Vector3 newScale = entityTransform.localScale + scaleDirection * axisValue;

            if (Mathf.Abs(newScale.x) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED)
            {
                newScale += scaleDirection * MINIMUN_SCALE_ALLOWED;
            }

            return newScale;
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