using UnityEngine;

public class SkyboxCameraBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject target;
    private Camera cam;
    private Camera targetCam;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        this.transform.position = target.transform.position;
        this.transform.rotation = target.transform.rotation;
        cam.fieldOfView = targetCam.fieldOfView;
    }

    public void AssignCamera(Camera targetCamComponent, Camera skyboxCamera)
    {
        target = targetCamComponent.gameObject;
        cam = skyboxCamera;
        targetCam = targetCamComponent;
    }
}
