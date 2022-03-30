using DCL;
using DCL.Builder;
using DCL.Configuration;
using UnityEngine;

public abstract class BIWGizmos : MonoBehaviour, IBIWGizmos
{
    [SerializeField] private string gizmoType = string.Empty;
    [SerializeField] protected BIWGizmosAxis axisX;
    [SerializeField] protected BIWGizmosAxis axisY;
    [SerializeField] protected BIWGizmosAxis axisZ;

    public bool initialized => initializedValue;
    public GameObject currentGameObject { get; private set; }
    public bool initializedValue { get; private set; }

    protected float snapFactor = 0;

    protected bool worldOrientedGizmos = true;
    private Transform targetTransform = null;

    protected Camera builderCamera;
    protected Transform cameraHolderTransform;

    private Vector3 relativeScaleRatio;
    protected bool startDragging = false;
    internal float previousAxisValue;

    public IBIWGizmosAxis activeAxis { internal set; get; }

    public abstract void SetSnapFactor(SnapInfo snapInfo);
    public abstract float TransformEntity(Transform targetTransform, IBIWGizmosAxis axis, float axisValue);

    public virtual void Initialize(Camera camera, Transform cameraHolderTransform)
    {
        this.currentGameObject = gameObject;
        initializedValue = true;
        relativeScaleRatio = transform.localScale / GetCameraPlaneDistance(cameraHolderTransform, transform.position);
        builderCamera = camera;
        this.cameraHolderTransform = cameraHolderTransform;
        axisX.SetGizmo(this);
        axisY.SetGizmo(this);
        axisZ.SetGizmo(this);
    }

    public Vector3 GetActiveAxisVector()
    {
        if (activeAxis == axisX)
            return Vector3.right;
        if (activeAxis == axisY)
            return  Vector3.up;
        if (activeAxis == axisZ)
            return  Vector3.back;

        return Vector3.zero;
    }

    public void ForceRelativeScaleRatio() { relativeScaleRatio = new Vector3(BIWSettings.GIZMOS_RELATIVE_SCALE_RATIO, BIWSettings.GIZMOS_RELATIVE_SCALE_RATIO, BIWSettings.GIZMOS_RELATIVE_SCALE_RATIO); }

    public string GetGizmoType() { return gizmoType; }

    public virtual void SetTargetTransform(Transform entityTransform)
    {
        targetTransform = entityTransform;
        SetPositionToTarget();
    }

    public virtual void OnBeginDrag(IBIWGizmosAxis axis, Transform entityTransform)
    {
        startDragging = true;
        targetTransform = entityTransform;
        activeAxis = axis;
        axis.SetColorHighlight();
    }

    public float OnDrag(Vector3 hitPoint, Vector2 mousePosition)
    {
        float axisValue = GetHitPointToAxisValue(activeAxis, hitPoint, mousePosition);

        if (startDragging)
        {
            startDragging = false;
            previousAxisValue = axisValue;
        }

        float transformValue = axisValue - previousAxisValue;
        if (Mathf.Abs(transformValue) >= snapFactor)
        {
            if (snapFactor > 0)
            {
                float sign = Mathf.Sign(transformValue);
                transformValue = transformValue + (Mathf.Abs(transformValue) % snapFactor) * -sign;
            }

            SetPreviousAxisValue(axisValue, transformValue);
            return TransformEntity(targetTransform, activeAxis, transformValue);
        }
        return 0;
    }

    public void OnEndDrag()
    {
        Environment.i.platform.physicsSyncController.MarkDirty();
        activeAxis?.SetColorDefault();
    }

    public virtual bool RaycastHit(Ray ray, out Vector3 hitPoint)
    {
        Vector3 hitPos = ray.GetPoint(Vector3.Distance(ray.origin, activeAxis.axisTransform.position));
        Vector3 hitOffset = (hitPos - activeAxis.axisTransform.position);
        hitPoint = activeAxis.axisTransform.position + Vector3.Project(hitOffset, activeAxis.axisTransform.forward);
        return true;
    }

    internal virtual float GetHitPointToAxisValue(IBIWGizmosAxis axis, Vector3 hitPoint, Vector2 mousePosition)
    {
        Vector3 dir = (hitPoint - axis.axisTransform.position).normalized;
        float sign = Vector3.Angle(dir, axis.axisTransform.forward) == 180 ? -1 : 1;
        return Vector3.Distance(activeAxis.axisTransform.position, hitPoint) * sign;
    }

    internal virtual void SetPreviousAxisValue(float axisValue, float transformValue) { previousAxisValue = axisValue - transformValue; }

    private void SetPositionToTarget()
    {
        if (!targetTransform)
            return;

        transform.position = targetTransform.position;
        if (!worldOrientedGizmos)
            transform.rotation = targetTransform.rotation;
    }

    private void Update()
    {
        SetPositionToTarget();
        if (!builderCamera)
            return;

        float dist = GetCameraPlaneDistance(cameraHolderTransform, transform.position);
        transform.localScale = relativeScaleRatio * dist;
    }

    private static float GetCameraPlaneDistance(Transform cameraTransform, Vector3 objectPosition)
    {
        Plane plane = new Plane(cameraTransform.forward, cameraTransform.position);
        return plane.GetDistanceToPoint(objectPosition);
    }
}