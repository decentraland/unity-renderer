using Cinemachine;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] internal Transform cameraTransform;

    [Header("Virtual Cameras")]
    [SerializeField] internal CameraStateBase[] cameraModes;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger cameraChangeAction;

    internal Dictionary<CameraStateBase.ModeId, CameraStateBase> cachedModeToVirtualCamera;

    private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    private Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
    private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
    private Vector3Variable playerUnityToWorldOffset => CommonScriptableObjects.playerUnityToWorldOffset;

    internal CameraStateBase.ModeId currentMode = CameraStateBase.ModeId.FirstPerson;
    public CameraStateBase currentCameraState => cachedModeToVirtualCamera[currentMode];

    private void Start()
    {
        CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
        OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

        cachedModeToVirtualCamera = cameraModes.ToDictionary(x => x.cameraModeId, x => x);

        using (var iterator = cachedModeToVirtualCamera.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Init(cameraTransform);
            }
        }

        cameraChangeAction.OnTriggered += OnCameraChangeAction;
        playerUnityToWorldOffset.OnChange += PrecisionChanged;

        SetCameraMode(currentMode);
    }

    private void OnRenderingStateChanged(bool enabled, bool prevState)
    {
        cameraTransform.gameObject.SetActive(enabled);
    }

    private void OnCameraChangeAction(DCLAction_Trigger action)
    {
        if (currentMode == CameraStateBase.ModeId.FirstPerson)
        {
            SetCameraMode(CameraStateBase.ModeId.ThirdPerson);
        }
        else
        {
            SetCameraMode(CameraStateBase.ModeId.FirstPerson);
        }
    }

    public void SetCameraMode(CameraStateBase.ModeId newMode)
    {
        currentCameraState.OnUnselect();
        currentMode = newMode;
        currentCameraState.OnSelect();
    }

    private void PrecisionChanged(Vector3 newValue, Vector3 oldValue)
    {
        transform.position += newValue - oldValue;
    }

    private void Update()
    {
        cameraForward.Set(cameraTransform.forward);
        cameraRight.Set(cameraTransform.right);
        cameraPosition.Set(cameraTransform.position);

        currentCameraState?.OnUpdate();
    }

    public void SetRotation(string setRotationPayload)
    {
        var payload = Utils.FromJsonWithNulls<SetRotationPayload>(setRotationPayload);
        currentCameraState?.OnSetRotation(payload);
    }

    public void SetRotation(float x, float y, float z, Vector3? cameraTarget = null)
    {
        currentCameraState?.OnSetRotation(new SetRotationPayload() { x = x, y = y, z = z, cameraTarget = cameraTarget });
    }

    public Vector3 GetRotation()
    {
        if (currentCameraState != null)
            return currentCameraState.OnGetRotation();

        return Vector3.zero;
    }


    public Vector3 GetPosition()
    {
        return CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.State.FinalPosition;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.playerUnityToWorldOffset.OnChange -= PrecisionChanged;
        cameraChangeAction.OnTriggered -= OnCameraChangeAction;
        CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
    }

    [System.Serializable]
    public class SetRotationPayload
    {
        public float x;
        public float y;
        public float z;
        public Vector3? cameraTarget;
    }
}
