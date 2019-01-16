using DCL.Configuration;
using UnityEngine;

public class DCLCharacterController : MonoBehaviour
{
    [Header("Aiming")]
    public float aimingHorizontalSpeed = 100f;
    public float aimingVerticalSpeed = 100f;
    public float aimingVerticalMinimumAngle = -89f;
    public float aimingVerticalMaximumAngle = 89f;

    [Header("Movement")]
    public float gravity = -1f;
    public float movementSpeed = 10f;
    public float jumpForce = 20f;

    [Header("Collisions")]
    public Transform raycastsParent;
    public LayerMask collidingLayers;

    new Transform camera;
    new Rigidbody rigidbody;
    new Collider collider;

    float aimingHorizontalAngle;
    float aimingVerticalAngle;
    float lastUngroundedTime = 0f;
    float lastJumpButtonPressedTime = 0f;
    Vector3 velocity = Vector3.zero;
    Vector2 aimingInput;
    Vector2 movementInput;
    bool isSprinting = false;
    bool isJumping = false;
    bool isGrounded = false;
    bool jumpButtonPressed = false;
    bool jumpButtonPressedThisFrame = false;
    CharacterController characterController;

    public delegate void CharacterControllerEventDelegate();
    public event CharacterControllerEventDelegate OnCharacterMoved;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
        camera = GetComponentInChildren<Camera>().transform;
    }

    void SetPosition(string positionVector)
    {
        transform.position = JsonUtility.FromJson<Vector3>(positionVector);

        ReportMovement();
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
        else
        {
            velocity.y += gravity;

            if (previouslyGrounded && !isJumping)
            {
                lastUngroundedTime = Time.time;
            }
        }

        if (!Cursor.visible)
        {
            DetectInput();

            // Rotation
            transform.rotation = Quaternion.Euler(0f, aimingHorizontalAngle, 0f);
            camera.localRotation = Quaternion.Euler(-aimingVerticalAngle, 0f, 0f);

            // Horizontal movement
            var speed = isSprinting ? movementSpeed * 2f : movementSpeed;

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

        if (jumpButtonPressed && (Time.time - lastJumpButtonPressedTime < 0.15f)) // almost grounded jump button press allowed time
        {
            if (isGrounded || (Time.time - lastUngroundedTime) < 0.1f) // just-left-ground jump allowed time
            {
                Jump();
            }
        }

        if (velocity != Vector3.zero)
        {
            characterController.Move(velocity * Time.deltaTime);

            ReportMovement();
        }
    }

    void Jump()
    {
        if (isJumping) return;

        Debug.Log("JUMP");

        isJumping = true;

        velocity.y = jumpForce;
    }

    void DetectInput()
    {
        aimingInput.x = Input.GetAxis("Mouse X");
        aimingInput.y = Input.GetAxis("Mouse Y");

        aimingHorizontalAngle += Mathf.Clamp(aimingInput.x, -1, 1) * aimingHorizontalSpeed;
        aimingVerticalAngle += Mathf.Clamp(aimingInput.y, -1, 1) * aimingVerticalSpeed;

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
        return characterController.isGrounded || Physics.Raycast(transform.position, Vector3.down, collider.bounds.extents.y + 0.1f, collidingLayers);
    }

    void ReportMovement()
    {
        DCL.Interface.WebInterface.ReportPosition(transform.position, transform.rotation);

        if (OnCharacterMoved != null)
        {
            OnCharacterMoved();
        }
    }
}
