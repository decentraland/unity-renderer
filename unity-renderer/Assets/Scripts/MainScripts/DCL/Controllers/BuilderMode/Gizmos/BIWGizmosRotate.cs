using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BIWGizmosRotate : BIWGizmos
{
    private Plane raycastPlane;

    public override void SetSnapFactor(BIWGizmosController.SnapInfo snapInfo) { snapFactor = snapInfo.rotation; }

    public override float TransformEntity(Transform entityTransform, BIWGizmosAxis axis, float axisValue)
    {
        Space space = worldOrientedGizmos ? Space.World : Space.Self;
        Vector3 rotationVector = activeAxis.transform.forward;

        float amount = axisValue * Mathf.Rad2Deg;
        entityTransform.Rotate(rotationVector, amount, space);
        return amount;
    }

    public override void OnBeginDrag(BIWGizmosAxis axis, Transform entityTransform)
    {
        base.OnBeginDrag(axis, entityTransform);
        raycastPlane = new Plane(activeAxis.transform.forward, transform.position);
    }

    public override bool RaycastHit(Ray ray, out Vector3 hitPoint)
    {
        float raycastHitDistance = 0.0f;

        if (raycastPlane.Raycast(ray, out raycastHitDistance))
        {
            hitPoint = ray.GetPoint(raycastHitDistance);
            return true;
        }
        hitPoint = Vector3.zero;
        return false;
    }

    internal override float GetHitPointToAxisValue(BIWGizmosAxis axis, Vector3 hitPoint, Vector2 mousePosition)
    {
        Vector3 hitDir = (hitPoint - transform.position).normalized;
        return Vector3.SignedAngle(axis.transform.up, hitDir, axis.transform.forward) * Mathf.Deg2Rad;
    }

    internal override void SetPreviousAxisValue(float axisValue, float transformValue) { previousAxisValue = axisValue; }
}