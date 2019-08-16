using DCL.Configuration;
using UnityEngine;

public class DCLCharacterController : MonoBehaviour
{
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
    public DCLCharacterPosition characterPosition = new DCLCharacterPosition();

    public Transform cameraTransform
    {
        get { return camera.transform; }
    }

    [Header("Collisions")]
    public LayerMask groundLayers;

    [System.NonSerialized]
    public bool initialPositionAlreadySet = false;

    [System.NonSerialized]
    public CharacterController characterController;

    new Transform camera;
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

        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        camera = GetComponentInChildren<Camera>().transform;
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

    public void Teleport(string positionVector)
    {
        var newPosition = JsonUtility.FromJson<Vector3>(positionVector);

        SetPosition(newPosition);

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
            transform.rotation = Quaternion.Euler(0f, aimingHorizontalAngle, 0f);
            camera.localRotation = Quaternion.Euler(-aimingVerticalAngle, 0f, 0f);

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
        var localRotation = camera.localRotation.eulerAngles;
        var rotation = transform.rotation.eulerAngles;
        var feetY = characterPosition.worldPosition.y - characterController.height / 2;
        var playerHeight = camera.position.y - feetY;
        var compositeRotation = Quaternion.Euler(localRotation.x, rotation.y, localRotation.z);

        var reportPosition = characterPosition.worldPosition;
        reportPosition.y += camera.localPosition.y;

        DCL.Interface.WebInterface.ReportPosition(reportPosition, compositeRotation, playerHeight);

        lastMovementReportTime = Time.realtimeSinceStartup;
    }
}
