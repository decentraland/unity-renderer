using Cinemachine;
using UnityEngine;
public class CameraStateBase : MonoBehaviour
{    

    public CinemachineVirtualCameraBase defaultVirtualCamera;

    protected Transform cameraTransform;
    public CameraMode.ModeId cameraModeId;

    public virtual void Init(Transform cameraTransform)
    {
        this.cameraTransform = cameraTransform;
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
}
