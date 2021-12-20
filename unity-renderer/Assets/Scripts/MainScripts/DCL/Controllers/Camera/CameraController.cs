using Cinemachine;
using DCL.Helpers;
using System.Collections.Generic;
using System.Linq;
using DCL;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.Camera
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        internal new UnityEngine.Camera camera;

        private Transform cameraTransform;

        [Header("Virtual Cameras")]
        [SerializeField]
        internal CinemachineBrain cameraBrain;

        [SerializeField]
        internal CameraStateBase[] cameraModes;

        [Header("InputActions")]
        [SerializeField]
        internal InputAction_Trigger cameraChangeAction;

        internal Dictionary<CameraMode.ModeId, CameraStateBase> cachedModeToVirtualCamera;

        public delegate void CameraBlendStarted();

        public event CameraBlendStarted onCameraBlendStarted;

        public delegate void CameraBlendFinished();

        public event CameraBlendFinished onCameraBlendFinished;

        private bool wasBlendingLastFrame;

        private Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
        private Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
        private Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
        private Vector3Variable worldOffset => CommonScriptableObjects.worldOffset;
        private BooleanVariable cameraIsBlending => CommonScriptableObjects.cameraIsBlending;

        public CameraStateBase currentCameraState => cachedModeToVirtualCamera[CommonScriptableObjects.cameraMode];

        [HideInInspector]
        public System.Action<CameraMode.ModeId> onSetCameraMode;

        private void Awake()
        {
            cameraTransform = this.camera.transform;

            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

            CommonScriptableObjects.cameraBlocked.OnChange += CameraBlocked_OnChange;

            cachedModeToVirtualCamera = cameraModes.ToDictionary(x => x.cameraModeId, x => x);

            using (var iterator = cachedModeToVirtualCamera.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.Value.Initialize(camera);
                }
            }

            cameraChangeAction.OnTriggered += OnCameraChangeAction;
            worldOffset.OnChange += OnWorldReposition;

            SetCameraMode(CommonScriptableObjects.cameraMode);

            if (CommonScriptableObjects.isFullscreenHUDOpen)
                OnFullscreenUIVisibilityChange(CommonScriptableObjects.isFullscreenHUDOpen.Get(), !CommonScriptableObjects.isFullscreenHUDOpen.Get());

            CommonScriptableObjects.isFullscreenHUDOpen.OnChange += OnFullscreenUIVisibilityChange;
            wasBlendingLastFrame = false;
        }

        private float prevRenderScale = 1.0f;

        void OnFullscreenUIVisibilityChange(bool visibleState, bool prevVisibleState)
        {
            if (visibleState == prevVisibleState)
                return;

            camera.enabled = !visibleState && CommonScriptableObjects.rendererState.Get();
        }

        public bool TryGetCameraStateByType<T>(out CameraStateBase searchedCameraState)
        {
            foreach (CameraStateBase cameraMode in cameraModes)
            {
                if (cameraMode.GetType() == typeof(T))
                {
                    searchedCameraState = cameraMode;
                    return true;
                }
            }

            searchedCameraState = null;
            return false;
        }

        private void OnRenderingStateChanged(bool enabled, bool prevState) { camera.enabled = enabled && !CommonScriptableObjects.isFullscreenHUDOpen; }

        private void CameraBlocked_OnChange(bool current, bool previous)
        {
            foreach (CameraStateBase cam in cameraModes)
            {
                cam.OnBlock(current);
            }
        }

        private void OnCameraChangeAction(DCLAction_Trigger action)
        {
            if (CommonScriptableObjects.cameraMode == CameraMode.ModeId.FirstPerson)
            {
                SetCameraMode(CameraMode.ModeId.ThirdPerson);
            }
            else
            {
                SetCameraMode(CameraMode.ModeId.FirstPerson);
            }
        }

        public void SetCameraMode(CameraMode.ModeId newMode)
        {
            currentCameraState.OnUnselect();
            CommonScriptableObjects.cameraMode.Set(newMode);
            currentCameraState.OnSelect();

            DCL.Interface.WebInterface.ReportCameraChanged(newMode);

            onSetCameraMode?.Invoke(newMode);
        }

        public CameraStateBase GetCameraMode( CameraMode.ModeId mode ) { return cameraModes.FirstOrDefault( x => x.cameraModeId == mode ); }

        private void OnWorldReposition(Vector3 newValue, Vector3 oldValue) { transform.position += newValue - oldValue; }

        private void Update()
        {
            cameraForward.Set(cameraTransform.forward);
            cameraRight.Set(cameraTransform.right);
            DataStore.i.camera.rotation.Set(cameraTransform.rotation);
            cameraPosition.Set(cameraTransform.position);
            cameraIsBlending.Set(cameraBrain.IsBlending);

            if (cameraBrain.IsBlending)
            {
                if (!wasBlendingLastFrame)
                    onCameraBlendStarted?.Invoke();

                wasBlendingLastFrame = true;
            }
            else if (wasBlendingLastFrame)
            {
                onCameraBlendFinished?.Invoke();

                wasBlendingLastFrame = false;
            }

            currentCameraState?.OnUpdate();
        }

        public void SetRotation(string setRotationPayload)
        {
            var payload = Utils.FromJsonWithNulls<SetRotationPayload>(setRotationPayload);
            currentCameraState?.OnSetRotation(payload);
        }

        public void SetRotation(float x, float y, float z, Vector3? cameraTarget = null) { currentCameraState?.OnSetRotation(new SetRotationPayload() { x = x, y = y, z = z, cameraTarget = cameraTarget }); }

        public Vector3 GetRotation()
        {
            if (currentCameraState != null)
                return currentCameraState.OnGetRotation();

            return Vector3.zero;
        }

        public Vector3 GetPosition() { return CinemachineCore.Instance.GetActiveBrain(0).ActiveVirtualCamera.State.FinalPosition; }

        private void OnDestroy()
        {
            if (cachedModeToVirtualCamera != null)
            {
                using (var iterator = cachedModeToVirtualCamera.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        iterator.Current.Value.Cleanup();
                    }
                }
            }

            worldOffset.OnChange -= OnWorldReposition;
            cameraChangeAction.OnTriggered -= OnCameraChangeAction;
            CommonScriptableObjects.rendererState.OnChange -= OnRenderingStateChanged;
            CommonScriptableObjects.cameraBlocked.OnChange -= CameraBlocked_OnChange;
            CommonScriptableObjects.isFullscreenHUDOpen.OnChange -= OnFullscreenUIVisibilityChange;
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
}