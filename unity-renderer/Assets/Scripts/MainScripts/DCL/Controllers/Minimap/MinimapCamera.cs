using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    private const float HEIGHT = 400f;
    private static readonly Vector3 XZ_PLANE = new Vector3(1, 0, 1);

    public float closestZoom = 25;
    public float farthestZoom = 50;

    public bool northLocked = true;
    public new Camera camera;

    private Vector3Variable playerUnityPosition => CommonScriptableObjects.playerUnityPosition;
    private Vector3Variable playerUnityEulerAngles => CommonScriptableObjects.playerUnityEulerAngles;

    private FloatVariable minimapZoom => CommonScriptableObjects.minimapZoom;


    private void Awake()
    {
        minimapZoom.OnChange += OnZoomChange;
        SetNormalizedSize(minimapZoom.Get());
    }

    private void Start()
    {
        SetNormalizedSize(1);
        OnUnityPositionChange(playerUnityPosition, Vector3.zero);
        playerUnityPosition.OnChange += OnUnityPositionChange;
        playerUnityEulerAngles.OnChange += OnUnityEulerAnglesChange;
    }

    private void OnZoomChange(float current, float previous)
    {
        SetNormalizedSize(current);
    }

    private void OnUnityPositionChange(Vector3 current, Vector3 previous)
    {
        transform.position = Vector3.up * HEIGHT + Vector3.Scale(XZ_PLANE, current);
    }

    private void OnUnityEulerAnglesChange(Vector3 current, Vector3 previous)
    {
        transform.eulerAngles = northLocked ? Vector3.zero : current.y * Vector3.up;
    }

    public void SetNormalizedSize(float normalizedSize)
    {
        camera.orthographicSize = Mathf.Lerp(closestZoom, farthestZoom, normalizedSize);
    }

    private void OnDestroy()
    {
        playerUnityPosition.OnChange -= OnUnityPositionChange;
        playerUnityEulerAngles.OnChange -= OnUnityEulerAnglesChange;
        minimapZoom.OnChange -= OnZoomChange;
    }
}
