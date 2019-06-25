using UnityEngine;

public class AvatarAnimatorLegacy : MonoBehaviour
{
    private const float IDLE_TRANSITION_TIME = 0.15f;
    private const float STRAFE_TRANSITION_TIME = 0.25f;
    private const float RUN_TRANSITION_TIME = 0.01f;
    private const float WALK_TRANSITION_TIME = 0.01f;
    private const float JUMP_TRANSITION_TIME = 0.05f;
    private const float FALL_TRANSITION_TIME = 0.1f;

    private const float RUN_SPEED_THRESHOLD = 0.15f;

    [System.Serializable]
    public class Clips
    {
        public string idle;
        public string walk;
        public string run;
        public string jump;
        public string fall;
        public string special; //TODO(Brian): Not implemented yet
    }

    [System.Serializable]
    public class BlackBoard
    {
        public float walkSpeedFactor;
        public float runSpeedFactor;
        public float forwardSpeed;
        public float verticalSpeed;
        public float strafeSpeed;
        public bool isGrounded;
    }

    public new Animation animation;
    public Clips clips;
    public BlackBoard blackboard;
    public Transform target;

    System.Action<BlackBoard> currentState;

    Vector3 lastPosition;

    void Start()
    {
        currentState = State_Init;
    }

    void Update()
    {
        if (target == null || animation == null)
            return;

        UpdateInterface();
        currentState?.Invoke(blackboard);
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }


    void UpdateInterface()
    {
        Vector3 flattenedVelocity = target.position - lastPosition;

        //NOTE(Brian): Vertical speed
        float verticalVelocity = flattenedVelocity.y;
        blackboard.verticalSpeed = verticalVelocity;

        //NOTE(Brian): Forward/backwards speed
        flattenedVelocity.y = 0;
        float forwardFactor = Vector3.Dot(target.forward, flattenedVelocity.normalized);
        float strafeFactor = Mathf.Max(Vector3.Dot(target.right, flattenedVelocity.normalized),
                                       Vector3.Dot(target.right * -1, flattenedVelocity.normalized));

        blackboard.forwardSpeed = flattenedVelocity.magnitude * forwardFactor;
        blackboard.strafeSpeed = flattenedVelocity.magnitude * strafeFactor;

        //NOTE(Brian): isGrounded?
        blackboard.isGrounded = Physics.Raycast(target.transform.position + Vector3.up,
                                                Vector3.down,
                                                1.001f,
                                                DCLCharacterController.i.groundLayers);

        lastPosition = target.position;
    }



    void State_Init(BlackBoard bb)
    {
        if (bb.isGrounded == true)
        {
            currentState = State_Ground;
        }
        else
        {
            currentState = State_Air;
        }
    }

    void State_Ground(BlackBoard bb)
    {
        if (Mathf.Abs(bb.forwardSpeed) < 0.01f && Mathf.Abs(bb.strafeSpeed) < 0.01f)
        {
            animation.CrossFade(clips.idle, IDLE_TRANSITION_TIME);
        }
        else
        {
            if (bb.strafeSpeed > 0.1f)
            {
                animation.CrossFade(clips.walk, STRAFE_TRANSITION_TIME);
            }
            else
            {
                float moveSpeed = DCLCharacterController.i.movementSpeed;

                if (bb.forwardSpeed > RUN_SPEED_THRESHOLD)
                {
                    animation[clips.walk].normalizedSpeed = Mathf.Sign(bb.forwardSpeed) * (moveSpeed / 2.0f) * bb.runSpeedFactor;
                    animation.CrossFade(clips.run, RUN_TRANSITION_TIME);
                }
                else
                {
                    animation[clips.walk].normalizedSpeed = Mathf.Sign(bb.forwardSpeed) * (moveSpeed / 2.0f) * bb.walkSpeedFactor;
                    animation.CrossFade(clips.walk, WALK_TRANSITION_TIME);
                }
            }
        }

        if (!bb.isGrounded)
        {
            currentState = State_Air;
            Update();
        }
    }

    void State_Air(BlackBoard bb)
    {
        if (bb.verticalSpeed > 0)
        {
            animation.CrossFade(clips.jump, JUMP_TRANSITION_TIME);
        }
        else
        {
            animation.CrossFade(clips.fall, FALL_TRANSITION_TIME);
        }

        if (bb.isGrounded)
        {
            currentState = State_Ground;
            Update();
        }
    }
}
