using DCL.Configuration;
using UnityEngine;

public interface IBIWGizmos
{
    public abstract void SetSnapFactor(BIWGizmosController.SnapInfo snapInfo);

    public bool initialized { get; }
    public GameObject gameObject { get; }
    public void Initialize(Camera camera, Transform cameraHolderTransform);
    public Vector3 GetActiveAxisVector();
    public void OnEndDrag();
    public void ForceRelativeScaleRatio();
    public string GetGizmoType();
    public void SetTargetTransform(Transform entityTransform);
    public void OnBeginDrag(BIWGizmosAxis axis, Transform entityTransform);
    public float OnDrag(Vector3 hitPoint, Vector2 mousePosition);
    public bool RaycastHit(Ray ray, out Vector3 hitPoint);
}

public abstract class BIWGizmos : MonoBehaviour, IBIWGizmos
{
    [SerializeField] private string gizmoType = string.Empty;
    [SerializeField] protected BIWGizmosAxis axisX;
    [SerializeField] protected BIWGizmosAxis axisY;
    [SerializeField] protected BIWGizmosAxis axisZ;

    public bool initialized => initializedValue;
    public GameObject gameObject => gameObject;
    public bool initializedValue { get; private set; }

    protected float snapFactor = 0;

    protected bool worldOrientedGizmos = true;
    private Transform targetTransform = null;

    protected Camera builderCamera;
    protected Transform cameraHolderTransform;

    private Vector3 relativeScaleRatio;
    protected bool startDragging = false;
    protected float prevAxisValue;

    public BIWGizmosAxis activeAxis { protected set; get; }

    public abstract void SetSnapFactor(BIWGizmosController.SnapInfo snapInfo);
    public abstract float TransformEntity(Transform targetTransform, BIWGizmosAxis axis, float axisValue);

    public virtual void Initialize(Camera camera, Transform cameraHolderTransform)
    {
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

    public virtual void OnBeginDrag(BIWGizmosAxis axis, Transform entityTransform)
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
            prevAxisValue = axisValue;
        }

        float transformValue = axisValue - prevAxisValue;
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

    public void OnEndDrag() { activeAxis.SetColorDefault(); }

    public virtual bool RaycastHit(Ray ray, out Vector3 hitPoint)
    {
        Vector3 hitPos = ray.GetPoint(Vector3.Distance(ray.origin, activeAxis.transform.position));
        Vector3 hitOffset = (hitPos - activeAxis.transform.position);
        hitPoint = activeAxis.transform.position + Vector3.Project(hitOffset, activeAxis.transform.forward);
        return true;
    }

    protected virtual float GetHitPointToAxisValue(BIWGizmosAxis axis, Vector3 hitPoint, Vector2 mousePosition)
    {
        Vector3 dir = (hitPoint - axis.transform.position).normalized;
        float sign = Vector3.Angle(dir, axis.transform.forward) == 180 ? -1 : 1;
        return Vector3.Distance(activeAxis.transform.position, hitPoint) * sign;
    }

    protected virtual void SetPreviousAxisValue(float axisValue, float transformValue) { prevAxisValue = axisValue - transformValue; }

    private void SetPositionToTarget()
    {
        if (targetTransform)
        {
            transform.position = targetTransform.position;
            if (!worldOrientedGizmos)
            {
                transform.rotation = targetTransform.rotation;
            }
        }
    }

    private void Update()
    {
        SetPositionToTarget();
        if (builderCamera)
        {
            float dist = GetCameraPlaneDistance(cameraHolderTransform, transform.position);
            transform.localScale = relativeScaleRatio * dist;
        }
    }

    private static float GetCameraPlaneDistance(Transform cameraTransform, Vector3 objectPosition)
    {
        Plane plane = new Plane(cameraTransform.forward, cameraTransform.position);
        return plane.GetDistanceToPoint(objectPosition);
    }
}