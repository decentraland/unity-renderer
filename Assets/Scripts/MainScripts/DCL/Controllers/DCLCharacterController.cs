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
    public float gravity = -1f;
    public float movementSpeed = 10f;
    public float jumpForce = 20f;

    [Header("Collisions")]
    public LayerMask groundLayers;

    new Transform camera;
    new Rigidbody rigidbody;
    new Collider collider;

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
    CharacterController characterController;

    public static System.Action<Vector3> OnCharacterMoved;

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
    }

    public void SetPosition(string positionVector)
    {
        var newPosition = JsonUtility.FromJson<Vector3>(positionVector);

        // failsafe in case something teleports the player below ground collisions
        if (newPosition.y < 1f)
        {
            newPosition.y = 3f;
        }

        Vector3 previousPosition = transform.position;
        transform.position = newPosition;

        if (Moved(previousPosition))
        {
            ReportMovement();
        }
    }

    bool Moved(Vector3 previousPosition)
    {
        return Vector3.Distance(transform.position, previousPosition) > 0.001f;
    }

    void Update()
    {
        velocity.x = 0f;
        velocity.z = 0f;

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

        velocity.y += gravity * Time.deltaTime;

        if (!Cursor.visible)
        {
            DetectInput();

            // Rotation
            transform.rotation = Quaternion.Euler(0f, aimingHorizontalAngle, 0f);
            camera.localRotation = Quaternion.Euler(-aimingVerticalAngle, 0f, 0f);

            // Horizontal movement
            var speed = movementSpeed * (isSprinting ? 2f : 1f);

            if (movementInput.x > 0f)
            {
                velocity += (transform.right * speed) * Time.deltaTime;
            }
            else if (movementInput.x < 0f)
            {
                velocity += (-transform.right * speed) * Time.deltaTime;
            }

            if (movementInput.y > 0f)
            {
                velocity += (transform.forward * speed) * Time.deltaTime;
            }
            else if (movementInput.y < 0f)
            {
                velocity += (-transform.forward * speed) * Time.deltaTime;
            }
        }

        // Jump
        if (jumpButtonPressedThisFrame)
        {
            lastJumpButtonPressedTime = Time.time;
        }

        if (jumpButtonPressed && (Time.time - lastJumpButtonPressedTime < 0.15f)) // almost-grounded jump button press allowed time
        {
            if (isGrounded || (Time.time - lastUngroundedTime) < 0.1f) // just-left-ground jump allowed time
            {
                Jump();
            }
        }

        Vector3 previousPosition = transform.position;
        characterController.Move(velocity * Time.deltaTime);

        if (Moved(previousPosition) || (Time.realtimeSinceStartup - lastMovementReportTime) > PlayerSettings.POSITION_REPORTING_DELAY)
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

        aimingHorizontalAngle += Mathf.Clamp(aimingInput.x, -1, 1) * aimingHorizontalSpeed * Time.deltaTime;
        aimingVerticalAngle += Mathf.Clamp(aimingInput.y, -1, 1) * aimingVerticalSpeed * Time.deltaTime;

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
        return characterController.isGrounded || Physics.Raycast(transform.position, Vector3.down, collider.bounds.extents.y + 0.1f, groundLayers);
    }

    void ReportMovement()
    {
        DCL.Interface.WebInterface.ReportPosition(transform.position, transform.rotation);

        if (OnCharacterMoved != null)
        {
            OnCharacterMoved(transform.position);
        }

        lastMovementReportTime = Time.realtimeSinceStartup;
    }
}
