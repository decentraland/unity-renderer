using System.Collections;
using System.Collections.Generic;
using DCL.Builder;
using UnityEngine;

public class BIWScaleGizmos : BIWGizmos
{
    const float MINIMUN_SCALE_ALLOWED = 0.01f;

    [SerializeField] internal BIWGizmosAxis axisProportionalScale = null;

    internal Vector2 lastMousePosition;
    private Vector2 initialMousePosition;
    private Vector3 initialHitPoint;
    internal Vector3 lastHitPoint;
    private Dictionary<Transform, Vector3> entitiesOriginalPositions = new Dictionary<Transform, Vector3>();

    public override void Initialize(Camera camera, Transform cameraTransform)
    {
        base.Initialize(camera, cameraTransform);
        axisProportionalScale.SetGizmo(this);
    }

    public override void SetSnapFactor(SnapInfo snapInfo) { snapFactor = snapInfo.scale; }

    public override float TransformEntity(Transform entityTransform, IBIWGizmosAxis axis, float axisValue)
    {
        Vector3 scaleDirection = GetScaleDirection(entityTransform, axis);

        // In order to avoid to make the scale of each selected entity dependent of the 'entityTransform' parent,
        // we temporally move all entities to the same position as 'entityTransform' before calculate the new scale.
        foreach (Transform entity in entityTransform)
        {
            entitiesOriginalPositions.Add(entity, entity.transform.position);
            entity.transform.position = entityTransform.position;
        }

        if (axis == axisProportionalScale)
        {
            // New scale calculation (for proportional scale gizmo)
            entityTransform.localScale = GetNewScale(entityTransform, axisValue, scaleDirection, false);
        }
        else
        {
            // New scale calculation (for XYZ-axis scale gizmo)
            foreach (var originalEntity in entitiesOriginalPositions)
            {
                Transform entity = originalEntity.Key;
                entity.transform.SetParent(null);
                entity.localScale = GetNewScale(entity.transform, axisValue, scaleDirection, true);
                entity.SetParent(entityTransform);
            }
        }

        // Once the new scale has been calculated, we restore the original positions of all the selected entities.
        foreach (var originalEntity in entitiesOriginalPositions)
        {
            Transform entity = originalEntity.Key;
            Vector3 originalPosition = originalEntity.Value;

            entity.position = originalPosition;
        }

        entitiesOriginalPositions.Clear();

        return axisValue;
    }

    private Vector3 GetScaleDirection(Transform entityTransform, IBIWGizmosAxis axis)
    {
        Vector3 scaleDirection = activeAxis.axisTransform.forward;
        if (axis == axisProportionalScale)
        {
            scaleDirection = Vector3.one;
            float inputDirection = (lastMousePosition.x - initialMousePosition.x) + (lastMousePosition.y - initialMousePosition.y);
            if (inputDirection < 0)
                scaleDirection = -Vector3.one;

            initialMousePosition = lastMousePosition;
            initialHitPoint = lastHitPoint;
        }
        else if (worldOrientedGizmos)
        {
            scaleDirection = entityTransform.rotation * activeAxis.axisTransform.forward;
            scaleDirection.x = Mathf.Abs(scaleDirection.x);
            scaleDirection.y = Mathf.Abs(scaleDirection.y);
            scaleDirection.z = Mathf.Abs(scaleDirection.z);
        }

        return scaleDirection;
    }

    private Vector3 GetNewScale(Transform entityTransform, float axisValue, Vector3 scaleDirection, bool applyRounding)
    {
        Vector3 initialEntityScale = entityTransform.localScale;
        if (applyRounding && snapFactor > 0)
            initialEntityScale = GetScaleRoundedToSnapFactor(entityTransform.localScale, axisValue);

        Vector3 newScale = initialEntityScale + scaleDirection * axisValue;

        if (Mathf.Abs(newScale.x) < MINIMUN_SCALE_ALLOWED || Mathf.Abs(newScale.y) < MINIMUN_SCALE_ALLOWED)
            newScale += scaleDirection * MINIMUN_SCALE_ALLOWED;

        return newScale;
    }

    private Vector3 GetScaleRoundedToSnapFactor(Vector3 scale, float axisValue)
    {
        Vector3 activeAxisVector = GetActiveAxisVector();

        scale = new Vector3(
            activeAxisVector == Vector3.right ? (axisValue >= 0 ? Mathf.Floor(scale.x / snapFactor) : Mathf.Ceil(scale.x / snapFactor)) * snapFactor : scale.x,
            activeAxisVector == Vector3.up ? (axisValue >= 0 ? Mathf.Floor(scale.y / snapFactor) : Mathf.Ceil(scale.y / snapFactor)) * snapFactor : scale.y,
            activeAxisVector == Vector3.back ? (axisValue >= 0 ? Mathf.Floor(scale.z / snapFactor) : Mathf.Ceil(scale.z / snapFactor)) * snapFactor : scale.z);

        return scale;
    }

    internal override void SetPreviousAxisValue(float axisValue, float transformValue)
    {
        if (activeAxis == axisProportionalScale)
            previousAxisValue = 0;
        else
            previousAxisValue = axisValue;
    }

    internal override float GetHitPointToAxisValue(IBIWGizmosAxis axis, Vector3 hitPoint, Vector2 mousePosition)
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

        return axis.axisTransform.InverseTransformPoint(hitPoint).z;
    }
}