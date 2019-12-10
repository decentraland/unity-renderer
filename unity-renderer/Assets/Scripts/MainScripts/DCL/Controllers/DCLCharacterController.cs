using DCL;
using DCL.Configuration;
using DCL.Helpers;
using UnityEngine;
using System.Collections;

public class DCLCharacterController : MonoBehaviour
{
    public static DCLCharacterController i { get; private set; }

    [Header("Movement")]
    public float minimumYPosition = 1f;
    public float groundCheckExtraDistance = 0.25f;
    public float gravity = -55f;
    public float jumpForce = 12f;
    public float movementSpeed = 8f;
    public float runningSpeedMultiplier = 2f;

    [Tooltip("The maximum movement distance allowed on moving platforms before releasing the character")]
    public float movingPlatformAllowedPosDelta = 0.5f;

    public DCLCharacterPosition characterPosition;

    [Header("Collisions")]
    public LayerMask groundLayers;

    [System.NonSerialized]
    public bool initialPositionAlreadySet = false;

    [System.NonSerialized]
    public bool characterAlwaysEnabled = true;

    [System.NonSerialized]
    public CharacterController characterController;

    new Rigidbody rigidbody;
    new Collider collider;

    float deltaTime = 0.032f;
    float deltaTimeCap = 0.032f; // 32 milliseconds = 30FPS, 16 millisecodns = 60FPS
    float lastUngroundedTime = 0f;
    float lastJumpButtonPressedTime = 0f;
    float lastMovementReportTime;
    float originalGravity;
    Vector3 velocity = Vector3.zero;
    Vector2 aimingInput;
    bool isSprinting = false;
    bool isJumping = false;
    bool isGrounded = false;
    bool supportsMovingPlatforms = true;
    Transform groundTransform;
    Vector3 lastPosition;
    Vector3 groundLastPosition;
    Quaternion groundLastRotation;
    bool jumpButtonPressed = false;
    bool reEnablingGameObject = false;

    [Header("InputActions")]
    public InputAction_Hold jumpAction;
    public InputAction_Hold sprintAction;
    public InputAction_Measurable characterXAxis;
    public InputAction_Measurable characterYAxis;

    private InputAction_Hold.Started jumpStartedDelegate;
    private InputAction_Hold.Finished jumpFinishedDelegate;
    private InputAction_Hold.Started sprintStartedDelegate;
    private InputAction_Hold.Finished sprintFinishedDelegate;

    private Vector3NullableVariable characterForward => CommonScriptableObjects.characterForward;

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
        originalGravity = gravity;

        SuscribeToInput();
        CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
        CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
        CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);

        characterPosition = new DCLCharacterPosition();
        characterController = GetComponent<CharacterController>();
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();

        characterPosition.OnPrecisionAdjust += OnPrecisionAdjust;
        SceneController.OnDebugModeSet += () => supportsMovingPlatforms = true;

        lastPosition = transform.position;
    }

    private void SuscribeToInput()
    {
        jumpStartedDelegate = (action) =>
        {
            lastJumpButtonPressedTime = Time.time;
            jumpButtonPressed = true;
        };
        jumpFinishedDelegate = (action) => jumpButtonPressed = false;
        jumpAction.OnStarted += jumpStartedDelegate;
        jumpAction.OnFinished += jumpFinishedDelegate;

        sprintStartedDelegate = (action) => isSprinting = true;
        sprintFinishedDelegate = (action) => isSprinting = false;
        sprintAction.OnStarted += sprintStartedDelegate;
        sprintAction.OnFinished += sprintFinishedDelegate;
    }

    // To keep the character always active, just in case
    void OnDisable()
    {
        if (characterAlwaysEnabled && !reEnablingGameObject && SceneController.i != null)
            SceneController.i.StartCoroutine(ReEnableGameObject()); // gameObject cannot start the routine as it's being deactivated
    }

    IEnumerator ReEnableGameObject()
    {
        reEnablingGameObject = true;

        yield return null;

        gameObject.SetActive(true);
        ResetGround();

        reEnablingGameObject = false;
    }

    void OnDestroy()
    {
        characterPosition.OnPrecisionAdjust -= OnPrecisionAdjust;
        jumpAction.OnStarted -= jumpStartedDelegate;
        jumpAction.OnFinished -= jumpFinishedDelegate;
        sprintAction.OnStarted -= sprintStartedDelegate;
        sprintAction.OnFinished -= sprintFinishedDelegate;
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

        lastPosition = characterPosition.worldPosition;
        characterPosition.worldPosition = newPosition;
        transform.position = characterPosition.unityPosition;

        CommonScriptableObjects.playerUnityPosition.Set(characterPosition.unityPosition);
        CommonScriptableObjects.playerCoords.Set(Utils.WorldToGridPosition(characterPosition.worldPosition));
        CommonScriptableObjects.sceneID.Set(SceneController.i.GetCurrentScene(this.characterPosition));

        if (Moved(lastPosition))
        {
            if (Moved(lastPosition, useThreshold: true))
                ReportMovement();

            OnCharacterMoved?.Invoke(characterPosition);
        }

        lastPosition = transform.position;
    }

    public void Teleport(string teleportPayload)
    {
        var payload = Utils.FromJsonWithNulls<Vector3>(teleportPayload);

        var newPosition = new Vector3(payload.x, payload.y, payload.z);
        SetPosition(newPosition);

        if (OnPositionSet != null)
        {
            OnPositionSet.Invoke(characterPosition);
        }

        if (!initialPositionAlreadySet)
        {
            initialPositionAlreadySet = true;
        }
    }

    [System.Obsolete("SetPosition is deprecated, please use Teleport instead.", true)]
    public void SetPosition(string positionVector)
    {
        Teleport(positionVector);
    }

    public void SetEnabled(bool enabled)
    {
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

        if (transform.position.y < minimumYPosition)
        {
            SetPosition(characterPosition.worldPosition);

            return;
        }

        LockCharacterScale();

        velocity.x = 0f;
        velocity.z = 0f;
        velocity.y += gravity * deltaTime;

        bool previouslyGrounded = isGrounded;
        if (!isJumping || velocity.y <= 0f)
            CheckGround();

        if (isGrounded)
        {
            isJumping = false;

            velocity.y = gravity * deltaTime; // to avoid accumulating gravity in velocity.y while grounded
        }
        else if (previouslyGrounded && !isJumping)
        {
            lastUngroundedTime = Time.time;
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            if (characterForward.HasValue())
            {
                // Horizontal movement
                var speed = movementSpeed * (isSprinting ? runningSpeedMultiplier : 1f);

                if (characterXAxis.GetValue() > 0f)
                    velocity += (transform.right * speed);
                else if (characterXAxis.GetValue() < 0f)
                    velocity += (-transform.right * speed);

                if (characterYAxis.GetValue() > 0f)
                    velocity += (transform.forward * speed);
                else if (characterYAxis.GetValue() < 0f)
                    velocity += (-transform.forward * speed);

                transform.forward = characterForward.Get().Value;
                CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
            }
        }

        if (jumpButtonPressed && (Time.time - lastJumpButtonPressedTime < 0.15f)) // almost-grounded jump button press allowed time
        {
            if (isGrounded || (Time.time - lastUngroundedTime) < 0.1f) // just-left-ground jump allowed time
            {
                Jump();
            }
        }

        if (IsOnMovingPlatform() && !characterPosition.RepositionedWorldLastFrame() && Vector3.Distance(lastPosition, transform.position) > movingPlatformAllowedPosDelta)
        {
            ResetGround();

            // As the character has already been moved faster-than-we-want, we reposition it
            characterController.transform.position = lastPosition;
        }

        characterController.Move(velocity * deltaTime);
        SetPosition(characterPosition.UnityToWorldPosition(transform.position));

        if ((DCLTime.realtimeSinceStartup - lastMovementReportTime) > PlayerSettings.POSITION_REPORTING_DELAY)
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
        isGrounded = false;

        velocity.y = jumpForce;
    }

    void LockCharacterScale()
    {
        // To keep the character always at global scale 1
        if (transform.lossyScale == Vector3.one) return;

        Transform parentTransform = null;
        if (transform.parent != null)
        {
            parentTransform = transform.parent;
            transform.SetParent(null);
        }

        transform.localScale = Vector3.one;

        if (parentTransform != null)
        {
            transform.SetParent(parentTransform);
        }
    }

    void CheckGround()
    {
        Transform transformHit = CastGroundCheckingRays();

        if (transformHit != null)
        {
            if (groundTransform == transformHit)
            {
                if (supportsMovingPlatforms && transform.parent == null && !characterPosition.RepositionedWorldLastFrame() && (transformHit.position != groundLastPosition || transformHit.rotation != groundLastRotation))
                {
                    // By letting unity parenting handle the transformations for us, the UX is smooth.
                    transform.SetParent(groundTransform);
                }
            }
            else
            {
                groundTransform = transformHit;
            }

            groundLastPosition = groundTransform.position;
            groundLastRotation = groundTransform.rotation;
        }
        else
        {
            ResetGround();
        }

        isGrounded = groundTransform != null;
    }

    // We secuentially cast rays in 4 directions (only if the previous one didn't hit anything)
    Transform CastGroundCheckingRays()
    {
        RaycastHit hitInfo;
        float rayMagnitude = (collider.bounds.extents.y + groundCheckExtraDistance);

        Ray ray = new Ray(transform.position, Vector3.down * rayMagnitude);
        if (!CastGroundCheckingRay(ray, Vector3.zero, out hitInfo, rayMagnitude) // center
            && !CastGroundCheckingRay(ray, transform.forward, out hitInfo, rayMagnitude) // forward
            && !CastGroundCheckingRay(ray, transform.right, out hitInfo, rayMagnitude) // right
            && !CastGroundCheckingRay(ray, -transform.forward, out hitInfo, rayMagnitude) // back
            && !CastGroundCheckingRay(ray, -transform.right, out hitInfo, rayMagnitude)) // left
        {
            return null;
        }

        // At this point there is a guaranteed hit, so this is not null
        return hitInfo.transform;
    }

    bool CastGroundCheckingRay(Ray ray, Vector3 originOffset, out RaycastHit hitInfo, float rayMagnitude)
    {
        ray.origin = transform.position + 0.9f * collider.bounds.extents.x * originOffset;
#if UNITY_EDITOR
        Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
        return Physics.Raycast(ray, out hitInfo, rayMagnitude, groundLayers);
    }

    public void ResetGround()
    {
        if (groundTransform == null && transform.parent == null) return;

        groundTransform = null;

        if (transform.parent != null)
        {
            transform.SetParent(null);
            velocity = Vector3.zero;
        }
    }

    bool IsOnMovingPlatform()
    {
        return isGrounded && transform.parent != null && transform.parent == groundTransform;
    }

    void ReportMovement()
    {
        float height = 0.875f;

        var reportPosition = characterPosition.worldPosition + (Vector3.up * height);
        var compositeRotation = Quaternion.LookRotation(transform.forward);
        var playerHeight = height + (characterController.height / 2);

        //NOTE(Brian): We have to wait for a Teleport before sending the ReportPosition, because if not ReportPosition events will be sent
        //             When the spawn point is being selected / scenes being prepared to be sent and the Kernel gets crazy.

        //             The race conditions that can arise from not having this flag can result in:
        //                  - Scenes not being sent for loading, making ActivateRenderer never being sent, only in WSS mode.
        //                  - Random teleports to 0,0 or other positions that shouldn't happen.
        if (initialPositionAlreadySet)
            DCL.Interface.WebInterface.ReportPosition(reportPosition, compositeRotation, playerHeight);

        lastMovementReportTime = DCLTime.realtimeSinceStartup;
    }

    public void PauseGravity()
    {
        gravity = 0f;
        velocity.y = 0f;
    }

    public void ResumeGravity()
    {
        gravity = originalGravity;
    }
}
