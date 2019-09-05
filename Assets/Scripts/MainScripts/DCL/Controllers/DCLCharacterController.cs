using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

public class DCLCharacterController : MonoBehaviour
{
    public class TeleportPayload
    {
        public float x;
        public float y;
        public float z;
        public Vector3? cameraTarget;
    }


    public static DCLCharacterController i { get; private set; }

    [Header("Aiming")]
    public float aimingHorizontalSpeed = 300f;

    public float aimingVerticalSpeed = 300f;
    public float aimingVerticalMinimumAngle = -89f;
    public float aimingVerticalMaximumAngle = 89f;

    [Header("Movement")]
    public float minimumYPosition = 1f;

    public float gravity = -55f;
    public float movementSpeed = 8f;
    public float jumpForce = 20f;
    public DCLCharacterPosition characterPosition;

    [SerializeField]
    private new Camera camera;

    [SerializeField]
    private AudioListener audioListener;

    private Transform cameraTransformValue;

    public Transform cameraTransform
    {
        get
        {
            if (cameraTransformValue == null)
            {
                cameraTransformValue = camera.transform;
            }

            return cameraTransformValue;
        }
    }

    [Header("Collisions")]
    public LayerMask groundLayers;

    [System.NonSerialized]
    public bool initialPositionAlreadySet = false;

    [System.NonSerialized]
    public CharacterController characterController;

    new Rigidbody rigidbody;
    new Collider collider;

    float deltaTime = 0.032f;
    float deltaTimeCap = 0.032f; // 32 milliseconds = 30FPS, 16 millisecodns = 60FPS
    float aimingHorizontalAngle;
    float aimingVerticalAngle;
    float lastUngroundedTime = 0f;
    float lastJumpButtonPressedTime = 0f;
    float lastMovementReportTime;
    Vector3 velocity = Vector3.zero;
    Vector2 aimingInput;
    Vector2 movementInput;
    bool isSprinting = false;
    bool isJumping = false;
    bool isGrounded = false;
    bool jumpButtonPressed = false;
    bool jumpButtonPressedThisFrame = false;

    public static System.Action<DCLCharacterPosition> OnCharacterMoved;
    public static System.Action<DCLCharacterPosition> OnPositionSet;

    void Awake()
    {
        if (i != null)
        {
            Destroy(gameObject);
            return;
        }

        i = this;

        CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
        CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
        CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);

        characterPosition = new DCLCharacterPosition();
        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;
    }

    void OnDestroy()
    {
        characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;
    }

    void OnPrecisionAdjust(DCLCharacterPosition charPos)
    {
        this.transform.position = charPos.unityPosition;
    }

    public void SetPosition(Vector3 newPosition)
    {
        // failsafe in case something teleports the player below ground collisions
        if (newPosition.y < minimumYPosition)
        {
            newPosition.y = minimumYPosition + 2f;
        }

        Vector3 previousPosition = characterPosition.worldPosition;
        characterPosition.worldPosition = newPosition;
        transform.position = characterPosition.unityPosition;

        CommonScriptableObjects.playerUnityPosition.Set(characterPosition.unityPosition);
        CommonScriptableObjects.playerCoords.Set(Utils.WorldToGridPosition(characterPosition.worldPosition));

        if (Moved(previousPosition))
        {
            if (Moved(previousPosition, useThreshold: true))
                ReportMovement();

            OnCharacterMoved?.Invoke(characterPosition);
        }

        if (!initialPositionAlreadySet)
        {
            initialPositionAlreadySet = true;
        }
    }

    public void SetEulerRotation(Vector3 eulerRotation)
    {
        transform.rotation = Quaternion.Euler(0f, eulerRotation.y, 0f);
        cameraTransform.localRotation = Quaternion.Euler(eulerRotation.x, 0f, 0f);
    }

    public void Teleport(string teleportPayload)
    {
        var payload = Utils.FromJsonWithNulls<TeleportPayload>(teleportPayload);

        var newPosition = new Vector3(payload.x, payload.y, payload.z);
        SetPosition(newPosition);

        if (payload.cameraTarget != null)
        {
            var lookDir = payload.cameraTarget - newPosition;
            var eulerRotation = Quaternion.LookRotation(lookDir.Value).eulerAngles;
            aimingVerticalAngle = -eulerRotation.x;
            aimingHorizontalAngle = eulerRotation.y;
            SetEulerRotation(eulerRotation);
        }

        if (OnPositionSet != null)
        {
            OnPositionSet.Invoke(characterPosition);
        }
    }

    [System.Obsolete("SetPosition is deprecated, please use Teleport instead.", true)]
    public void SetPosition(string positionVector)
    {
        Teleport(positionVector);
    }

    public void SetEnabled(bool enabled)
    {
        camera.enabled = enabled;
        audioListener.enabled = enabled;
        this.enabled = enabled;
    }

    bool Moved(Vector3 previousPosition, bool useThreshold = false)
    {
        if (useThreshold)
            return Vector3.Distance(characterPosition.worldPosition, previousPosition) > 0.001f;
        else
            return characterPosition.worldPosition != previousPosition;
    }

    void Update()
    {
        deltaTime = Mathf.Min(deltaTimeCap, Time.deltaTime);

        if (characterPosition.worldPosition.y < minimumYPosition)
        {
            SetPosition(characterPosition.worldPosition);

            return;
        }

        bool previouslyGrounded = isGrounded;
        isGrounded = IsGrounded();

        if (isGrounded)
        {
            isJumping = false;

            velocity.y = 0;
        }
        else if (previouslyGrounded && !isJumping)
        {
            lastUngroundedTime = Time.time;
        }

        velocity.x = 0f;
        velocity.z = 0f;
        velocity.y += gravity * deltaTime;

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            DetectInput();

            // Rotation
            var eulerRotation = new Vector3(-aimingVerticalAngle, aimingHorizontalAngle, 0);
            SetEulerRotation(eulerRotation);
            CommonScriptableObjects.playerUnityEulerAngles.Set( eulerRotation);

            // Horizontal movement
            var speed = movementSpeed * (isSprinting ? 2f : 1f);

            if (movementInput.x > 0f)
            {
                velocity += (transform.right * speed);
            }
            else if (movementInput.x < 0f)
            {
                velocity += (-transform.right * speed);
            }

            if (movementInput.y > 0f)
            {
                velocity += (transform.forward * speed);
            }
            else if (movementInput.y < 0f)
            {
                velocity += (-transform.forward * speed);
            }
        }

        // Jump
        if (jumpButtonPressedThisFrame)
        {
            lastJumpButtonPressedTime = Time.time;
        }

        if (jumpButtonPressed && (Time.time - lastJumpButtonPressedTime < 0.15f)
        ) // almost-grounded jump button press allowed time
        {
            if (isGrounded || (Time.time - lastUngroundedTime) < 0.1f) // just-left-ground jump allowed time
            {
                Jump();
            }
        }

        Vector3 previousPosition = characterPosition.worldPosition;

        characterController.Move(velocity * deltaTime);

        SetPosition(characterPosition.UnityToWorldPosition(transform.position));

        if ((Time.realtimeSinceStartup - lastMovementReportTime) > PlayerSettings.POSITION_REPORTING_DELAY)
        {
            ReportMovement();
        }
    }

    void Jump()
    {
        if (isJumping)
        {
            return;
        }

        isJumping = true;

        velocity.y = jumpForce;
    }

    void DetectInput()
    {
        aimingInput.x = Input.GetAxis("Mouse X");
        aimingInput.y = Input.GetAxis("Mouse Y");

        aimingHorizontalAngle += Mathf.Clamp(aimingInput.x, -1, 1) * aimingHorizontalSpeed * deltaTime;
        aimingVerticalAngle += Mathf.Clamp(aimingInput.y, -1, 1) * aimingVerticalSpeed * deltaTime;

        // Limit vertical aiming angle
        aimingVerticalAngle = Mathf.Clamp(aimingVerticalAngle, aimingVerticalMinimumAngle, aimingVerticalMaximumAngle);

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical");

        jumpButtonPressedThisFrame = Input.GetKeyDown(KeyCode.Space);
        jumpButtonPressed = Input.GetKey(KeyCode.Space);
    }

    bool IsGrounded()
    {
        return characterController.isGrounded || Physics.Raycast(characterPosition.unityPosition, Vector3.down,
                   collider.bounds.extents.y + 0.1f, groundLayers);
    }

    void ReportMovement()
    {
        var localRotation = cameraTransform.localRotation.eulerAngles;
        var rotation = transform.rotation.eulerAngles;
        var feetY = characterPosition.worldPosition.y - characterController.height / 2;
        var playerHeight = cameraTransform.position.y - feetY;
        var compositeRotation = Quaternion.Euler(localRotation.x, rotation.y, localRotation.z);

        var reportPosition = characterPosition.worldPosition;
        reportPosition.y += cameraTransform.localPosition.y;

        DCL.Interface.WebInterface.ReportPosition(reportPosition, compositeRotation, playerHeight);

        lastMovementReportTime = Time.realtimeSinceStartup;
    }
}