using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DCL.Helpers;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum CameraMode
    {
        FirstPerson,
        ThirdPerson,
    }

    [Serializable]
    public struct CameraStateToVirtualCamera
    {
        public CameraMode cameraMode;
        public CinemachineVirtualCameraBase virtualCamera;
    }

    [SerializeField] internal Transform cameraTransform;

    [Header("Virtual Cameras")]
    [SerializeField] internal CameraStateToVirtualCamera[] cameraModes;
    [SerializeField] internal CinemachineVirtualCameraBase freeLookCamera;

    [Header("InputActions")]
    [SerializeField] internal InputAction_Trigger cameraChangeAction;
    [SerializeField] internal InputAction_Hold freeCameraModeAction;

    internal Dictionary<CameraMode, CinemachineVirtualCameraBase> cachedModeToVirtualCamera;

    private Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;
    private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
    private Vector3Variable playerUnityToWorldOffset => CommonScriptableObjects.playerUnityToWorldOffset;

    internal InputAction_Hold.Started freeCameraModeStartedDelegate;
    internal InputAction_Hold.Finished freeCameraModeFinishedDelegate;

    internal CameraMode currentMode = CameraMode.FirstPerson;

    private void Awake()
    {
        cachedModeToVirtualCamera =  cameraModes.ToDictionary(x => x.cameraMode, x => x.virtualCamera);
        using (var iterator = cachedModeToVirtualCamera.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.gameObject.SetActive(false);
            }
        }

        freeCameraModeStartedDelegate = (action) => SetFreeCameraModeActive(true);
        freeCameraModeFinishedDelegate = (action) => SetFreeCameraModeActive(false);
        freeCameraModeAction.OnStarted += freeCameraModeStartedDelegate;
        freeCameraModeAction.OnFinished += freeCameraModeFinishedDelegate;
        cameraChangeAction.OnTriggered += OnCameraChangeAction;
        playerUnityToWorldOffset.OnChange += PrecisionChanged;

        SetFreeCameraModeActive(false);
        SetCameraMode(currentMode);
    }

    private void OnCameraChangeAction(DCLAction_Trigger action)
    {
        if (currentMode == CameraMode.FirstPerson)
            SetCameraMode(CameraMode.ThirdPerson);
        else
            SetCameraMode(CameraMode.FirstPerson);
    }

    internal void SetCameraMode(CameraMode newMode)
    {
        cachedModeToVirtualCamera[currentMode].gameObject.SetActive(false);
        currentMode = newMode;
        cachedModeToVirtualCamera[currentMode].gameObject.SetActive(true);
    }

    private void SetFreeCameraModeActive(bool active)
    {
        //FreeLookCamera has higher priority, so we dont have to enable/disable the other cameras.
        freeLookCamera.gameObject.SetActive(active);
    }

    private void PrecisionChanged(Vector3 newValue, Vector3 oldValue)
    {
        transform.position += newValue - oldValue;
    }

    private void Update()
    {
        cameraForward.Set(cameraTransform.forward);
        cameraPosition.Set(cameraTransform.position);

        if (CinemachineCore.Instance.IsLive(freeLookCamera))
        {
            characterForward.Set(null);
        }
        else
        {
            var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            switch (currentMode)
            {
                case CameraMode.FirstPerson:
                    characterForward.Set(xzPlaneForward);
                    break;
                case CameraMode.ThirdPerson:
                    if (!characterForward.HasValue())
                        characterForward.Set(xzPlaneForward);
                    var lerpedForward = Vector3.Slerp(characterForward.Get().Value, xzPlaneForward, 5 * Time.deltaTime);
                    characterForward.Set(lerpedForward);
                    break;
            }
        }
    }

    public void SetRotation(string setRotationPayload)
    {
        var payload = Utils.FromJsonWithNulls<SetRotationPayload>(setRotationPayload);
        var eulerDir = Vector3.zero;
        if (payload.cameraTarget.HasValue)
        {
            var newPos = new Vector3(payload.x, payload.y, payload.z);
            var cameraTarget = payload.cameraTarget.GetValueOrDefault();
            var dirToLook = (cameraTarget - newPos);
            eulerDir = Quaternion.LookRotation(dirToLook).eulerAngles;
        }

        if (cachedModeToVirtualCamera[currentMode] is CinemachineVirtualCamera vcamera)
        {
            var pov = vcamera.GetCinemachineComponent<CinemachinePOV>();
            pov.m_HorizontalAxis.Value = eulerDir.y;
            pov.m_VerticalAxis.Value = eulerDir.x;
        }
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.playerUnityToWorldOffset.OnChange -= PrecisionChanged;
        freeCameraModeAction.OnStarted -= freeCameraModeStartedDelegate;
        freeCameraModeAction.OnFinished -= freeCameraModeFinishedDelegate;
        cameraChangeAction.OnTriggered -= OnCameraChangeAction;
    }


    public class SetRotationPayload
    {
        public float x;
        public float y;
        public float z;
        public Vector3? cameraTarget;
    }
}