using Cinemachine;
using UnityEngine;

public class CameraStateTPS : CameraStateBase
{
    private CinemachineFreeLook defaultVirtualCameraAsFreeLook => defaultVirtualCamera as CinemachineFreeLook;

    [SerializeField] private InputAction_Measurable characterYAxis;
    [SerializeField] private InputAction_Measurable characterXAxis;
    private CinemachineTransposer freeLookTopRig;
    private CinemachineTransposer freeLookMidRig;
    private CinemachineTransposer freeLookBotRig;
    private Vector3 freeLookTopRigOriginalBodyDamping;
    private Vector3 freeLookMidRigOriginalBodyDamping;
    private Vector3 freeLookBotRigOriginalBodyDamping;
    private float originalXAxisMaxSpeed;
    private float originalYAxisMaxSpeed;

    protected Vector3Variable characterPosition => CommonScriptableObjects.playerUnityPosition;
    protected Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;
    protected Vector3Variable cameraForward => CommonScriptableObjects.cameraForward;
    protected Vector3Variable cameraRight => CommonScriptableObjects.cameraRight;
    protected Vector3Variable cameraPosition => CommonScriptableObjects.cameraPosition;
    
    public float rotationLerpSpeed = 10;

    public override void Init(Camera camera)
    {
        freeLookTopRig = defaultVirtualCameraAsFreeLook.GetRig(0).GetCinemachineComponent<CinemachineTransposer>();
        freeLookTopRigOriginalBodyDamping = new Vector3(freeLookTopRig.m_XDamping, freeLookTopRig.m_YDamping, freeLookTopRig.m_ZDamping);
        freeLookMidRig = defaultVirtualCameraAsFreeLook.GetRig(1).GetCinemachineComponent<CinemachineTransposer>();
        freeLookMidRigOriginalBodyDamping = new Vector3(freeLookMidRig.m_XDamping, freeLookMidRig.m_YDamping, freeLookMidRig.m_ZDamping);
        freeLookBotRig = defaultVirtualCameraAsFreeLook.GetRig(2).GetCinemachineComponent<CinemachineTransposer>();
        freeLookBotRigOriginalBodyDamping = new Vector3(freeLookBotRig.m_XDamping, freeLookBotRig.m_YDamping, freeLookBotRig.m_ZDamping);

        originalXAxisMaxSpeed = defaultVirtualCameraAsFreeLook.m_XAxis.m_MaxSpeed;
        originalYAxisMaxSpeed = defaultVirtualCameraAsFreeLook.m_YAxis.m_MaxSpeed;

        base.Init(camera);
    }

    private void OnEnable()
    {
        CommonScriptableObjects.playerIsOnMovingPlatform.OnChange += UpdateMovingPlatformCamera;
    }

    private void OnDisable()
    {
        CommonScriptableObjects.playerIsOnMovingPlatform.OnChange -= UpdateMovingPlatformCamera;
    }

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
                var lerpedForward = Vector3.Slerp(characterForward.Get().Value, forwardTarget, rotationLerpSpeed * Time.deltaTime);
                characterForward.Set(lerpedForward);
            }
        }
    }

    public override Vector3 OnGetRotation()
    {
        return new Vector3(defaultVirtualCameraAsFreeLook.m_YAxis.Value, defaultVirtualCameraAsFreeLook.m_XAxis.Value, 0);
    }

    public override void OnSetRotation(CameraController.SetRotationPayload payload)
    {
        var eulerDir = Vector3.zero;

        if (payload.cameraTarget.HasValue)
        {
            var newPos = new Vector3(payload.x, payload.y, payload.z);
            var cameraTarget = payload.cameraTarget.GetValueOrDefault();
            var dirToLook = (cameraTarget - newPos);
            eulerDir = Quaternion.LookRotation(dirToLook).eulerAngles;
        }

        defaultVirtualCameraAsFreeLook.m_XAxis.Value = eulerDir.y;
        defaultVirtualCameraAsFreeLook.m_YAxis.Value = eulerDir.x;
    }

    public override void OnBlock(bool blocked)
    {
        base.OnBlock(blocked);

        defaultVirtualCameraAsFreeLook.enabled = !blocked;
    }
}
