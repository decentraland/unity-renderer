using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    private const float HEIGHT = 20f;
    private static readonly Vector3 XZ_PLANE = new Vector3(1, 0, 1);
    private static readonly float MIN_SIZE = 25;
    private static readonly float MAX_SIZE = 35;

    public bool northLocked = true;
    public new Camera camera;

    private Vector3Variable playerUnityPosition => CommonScriptableObjects.playerUnityPosition;
    private Vector3Variable playerUnityEulerAngles => CommonScriptableObjects.playerUnityEulerAngles;

    private void Awake()
    {
        SetNormalizedSize(1);
    }

    private void Start()
    {
        playerUnityPosition.onChange += OnUnityPositionChange;
        playerUnityEulerAngles.onChange += OnUnityEulerAnglesChange;
    }

    private void OnUnityPositionChange(Vector3 current, Vector3 previous)
    {
        transform.position = Vector3.Scale(XZ_PLANE, current);
    }

    private void OnUnityEulerAnglesChange(Vector3 current, Vector3 previous)
    {
        transform.eulerAngles = northLocked ? Vector3.zero : current.y * Vector3.up;
    }

    public void SetNormalizedSize(float normalizedSize)
    {
        camera.orthographicSize = Mathf.Lerp(MIN_SIZE, MAX_SIZE, normalizedSize);
    }

    private void OnDestroy()
    {
        playerUnityPosition.onChange -= OnUnityPositionChange;
        playerUnityEulerAngles.onChange -= OnUnityEulerAnglesChange;
    }
}