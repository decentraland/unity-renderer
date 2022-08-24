using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraStateTPS : CameraStateBase
    {
        private CinemachineFreeLook defaultVirtualCameraAsFreeLook => defaultVirtualCamera as CinemachineFreeLook;

        [SerializeField] private CinemachineVirtualCamera fallingVirtualCamera;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;
        private CinemachineTransposer freeLookTopRig;
        private CinemachineTransposer freeLookMidRig;
        private CinemachineTransposer freeLookBotRig;
        private Vector3 freeLookTopRigOriginalBodyDamping;
        private Vector3 freeLookMidRigOriginalBodyDamping;
        private Vector3 freeLookBotRigOriginalBodyDamping;

        protected Vector3Variable characterPosition => CommonScriptableObjects.playerUnityPosition;
        protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;
        protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
        protected Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
        protected Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;

        public FollowWithDamping cameraTargetProbe;

        [Header("Avatar Feel Settings")]
        public float avatarRotationLerpSpeed = 10;

        [SerializeField]
        private CameraFreefall.Settings cameraFreefallSettings = new CameraFreefall.Settings();

        public CameraFreefall cameraFreefall;

        [SerializeField]
        private CameraDampOnGroundType.Settings cameraDampOnGroundTypeSettings = new CameraDampOnGroundType.Settings();

        public CameraDampOnGroundType cameraDampOnGroundType;

        [SerializeField]
        private CameraDampOnSprint.Settings cameraSprintDampingSettings = new CameraDampOnSprint.Settings();

        public CameraDampOnSprint cameraDampOnSprint;

        public override void Initialize(UnityEngine.Camera camera)
        {
            freeLookTopRig = defaultVirtualCameraAsFreeLook.GetRig(0).GetCinemachineComponent<CinemachineTransposer>();
            freeLookTopRigOriginalBodyDamping = new Vector3(freeLookTopRig.m_XDamping, freeLookTopRig.m_YDamping, freeLookTopRig.m_ZDamping);
            freeLookMidRig = defaultVirtualCameraAsFreeLook.GetRig(1).GetCinemachineComponent<CinemachineTransposer>();
            freeLookMidRigOriginalBodyDamping = new Vector3(freeLookMidRig.m_XDamping, freeLookMidRig.m_YDamping, freeLookMidRig.m_ZDamping);
            freeLookBotRig = defaultVirtualCameraAsFreeLook.GetRig(2).GetCinemachineComponent<CinemachineTransposer>();
            freeLookBotRigOriginalBodyDamping = new Vector3(freeLookBotRig.m_XDamping, freeLookBotRig.m_YDamping, freeLookBotRig.m_ZDamping);

            cameraDampOnSprint = new CameraDampOnSprint(cameraSprintDampingSettings, defaultVirtualCameraAsFreeLook, characterYAxis);
            cameraDampOnGroundType = new CameraDampOnGroundType(cameraDampOnGroundTypeSettings, cameraTargetProbe);
            cameraFreefall = new CameraFreefall(cameraFreefallSettings, defaultVirtualCameraAsFreeLook);

            base.Initialize(camera);
        }

        public override void Cleanup()
        {
            if (cameraTargetProbe != null)
                Destroy(cameraTargetProbe.gameObject);
        }

        private void OnEnable()
        {
            CommonScriptableObjects.playerIsOnMovingPlatform.OnChange += UpdateMovingPlatformCamera;
        }

        private void OnDisable() { CommonScriptableObjects.playerIsOnMovingPlatform.OnChange -= UpdateMovingPlatformCamera; }

        void UpdateMovingPlatformCamera(bool isOnMovingPlatform, bool wasOnMovingPlatform)
        {
            if (isOnMovingPlatform)
            {
                freeLookTopRig.m_XDamping = 0;
                freeLookTopRig.m_YDamping = 0;
                freeLookTopRig.m_ZDamping = 0;

                freeLookMidRig.m_XDamping = 0;
                freeLookMidRig.m_YDamping = 0;
                freeLookMidRig.m_ZDamping = 0;

                freeLookBotRig.m_XDamping = 0;
                freeLookBotRig.m_YDamping = 0;
                freeLookBotRig.m_ZDamping = 0;
            }
            else
            {
                freeLookTopRig.m_XDamping = freeLookTopRigOriginalBodyDamping.x;
                freeLookTopRig.m_YDamping = freeLookTopRigOriginalBodyDamping.y;
                freeLookTopRig.m_ZDamping = freeLookTopRigOriginalBodyDamping.z;

                freeLookMidRig.m_XDamping = freeLookMidRigOriginalBodyDamping.x;
                freeLookMidRig.m_YDamping = freeLookMidRigOriginalBodyDamping.y;
                freeLookMidRig.m_ZDamping = freeLookMidRigOriginalBodyDamping.z;

                freeLookBotRig.m_XDamping = freeLookBotRigOriginalBodyDamping.x;
                freeLookBotRig.m_YDamping = freeLookBotRigOriginalBodyDamping.y;
                freeLookBotRig.m_ZDamping = freeLookBotRigOriginalBodyDamping.z;
            }
        }

        public override void OnSelect()
        {
            if (characterForward.Get().HasValue)
            {
                defaultVirtualCameraAsFreeLook.m_XAxis.Value = Quaternion.LookRotation(characterForward.Get().Value, Vector3.up).eulerAngles.y;
                defaultVirtualCameraAsFreeLook.m_YAxis.Value = 0.5f;
            }

            base.OnSelect();
        }

        public override void OnUpdate()
        {
            defaultVirtualCameraAsFreeLook.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;

            bool didHit = DCLCharacterController.i.CastGroundCheckingRays(20, 0.1f, out RaycastHit hitInfo);

            cameraDampOnSprint.Update();
            cameraDampOnGroundType.Update(didHit, hitInfo);
            cameraFreefall.Update(didHit, hitInfo);

            UpdateAvatarRotationDamping();
        }

        /// <summary>
        /// This methods ensures the Avatar rotates smoothly when changing direction.
        /// Note that this will NOT affect the player movement, is only cosmetic.
        /// TODO(Brian): This is not camera feel, move this elsewhere
        /// </summary>
        private void UpdateAvatarRotationDamping()
        {
            var xzPlaneForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1));
            var xzPlaneRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1));

            if (characterYAxis.GetValue() != 0f || characterXAxis.GetValue() != 0f)
            {
                Vector3 forwardTarget = Vector3.zero;

                if (characterYAxis.GetValue() > 0)
                    forwardTarget += xzPlaneForward;

                if (characterYAxis.GetValue() < 0)
                    forwardTarget -= xzPlaneForward;

                if (characterXAxis.GetValue() > 0)
                    forwardTarget += xzPlaneRight;

                if (characterXAxis.GetValue() < 0)
                    forwardTarget -= xzPlaneRight;

                forwardTarget.Normalize();

                if (!characterForward.HasValue())
                {
                    characterForward.Set(forwardTarget);
                }
                else
                {
                    var lerpedForward = Vector3.Slerp(characterForward.Get().Value, forwardTarget, avatarRotationLerpSpeed * Time.deltaTime);
                    characterForward.Set(lerpedForward);
                }
            }
        }

        public override Vector3 OnGetRotation() { return new Vector3(defaultVirtualCameraAsFreeLook.m_YAxis.Value, defaultVirtualCameraAsFreeLook.m_XAxis.Value, 0); }

        public override void OnSetRotation(CameraController.SetRotationPayload payload)
        {
            var eulerDir = Vector3.zero;
            //Default looking direction is north
            var verticalAxisLookAt = Vector3.forward;

            if (payload.cameraTarget.HasValue)
            {
                var cameraTarget = payload.cameraTarget.GetValueOrDefault();

                var horizontalAxisLookAt = payload.y - cameraTarget.y;
                verticalAxisLookAt = new Vector3(cameraTarget.x - payload.x, 0, cameraTarget.z - payload.z);

                eulerDir.y = Vector3.SignedAngle(Vector3.forward, verticalAxisLookAt, Vector3.up);
                eulerDir.x = Mathf.Atan2(horizontalAxisLookAt, verticalAxisLookAt.magnitude) * Mathf.Rad2Deg;
            }

            defaultVirtualCameraAsFreeLook.m_XAxis.Value = eulerDir.y;

            //value range 0 to 1, being 0 the bottom orbit and 1 the top orbit
            var yValue = Mathf.InverseLerp(-90, 90, eulerDir.x);
            defaultVirtualCameraAsFreeLook.m_YAxis.Value = yValue;
            
            characterForward.Set(verticalAxisLookAt); 
        }

        public override void OnBlock(bool blocked)
        {
            base.OnBlock(blocked);
            defaultVirtualCameraAsFreeLook.enabled = !blocked;
        }
    }
}