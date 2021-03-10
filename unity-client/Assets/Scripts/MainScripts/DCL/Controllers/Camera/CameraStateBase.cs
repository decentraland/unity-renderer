using Cinemachine;
using UnityEngine;

public class CameraStateBase : MonoBehaviour
{
    public CinemachineVirtualCameraBase defaultVirtualCamera;

    protected new Camera camera;
    protected Transform cameraTransform;
    public CameraMode.ModeId cameraModeId;

    public virtual void Init(Camera camera)
    {
        this.camera = camera;
        this.cameraTransform = this.camera.transform;
        gameObject.SetActive(false);
    }

    public virtual void OnSelect()
    {
        gameObject.SetActive(true);
    }

    public virtual void OnUnselect()
    {
        gameObject.SetActive(false);
    }

    public virtual void OnUpdate()
    {
    }

    public virtual void OnSetRotation(CameraController.SetRotationPayload payload)
    {
    }

    public virtual Vector3 OnGetRotation()
    {
        return Vector3.zero;
    }

    public virtual void OnBlock(bool blocked)
    {
    }
}
