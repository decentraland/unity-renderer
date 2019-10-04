using UnityEngine;

public class AvatarAnimatorLegacy : MonoBehaviour
{
    const float IDLE_TRANSITION_TIME = 0.15f;
    const float STRAFE_TRANSITION_TIME = 0.25f;
    const float RUN_TRANSITION_TIME = 0.01f;
    const float WALK_TRANSITION_TIME = 0.01f;
    const float JUMP_TRANSITION_TIME = 0.01f;
    const float FALL_TRANSITION_TIME = 0.5f;

    const float AIR_EXIT_TRANSITION_TIME = 0.2f;
    const float GROUND_BLENDTREE_TRANSITION_TIME = 0.15f;

    const float RUN_SPEED_THRESHOLD = 0.05f;
    const float WALK_SPEED_THRESHOLD = 0.03f;

    const float ELEVATION_OFFSET = 0.6f;
    const float RAY_OFFSET_LENGTH = 3.0f;

    const float MAX_VELOCITY = 6.25f;

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
        public float movementSpeed;
        public float verticalSpeed;
        public bool isGrounded;
    }

    public new Animation animation;
    public Clips clips;
    public BlackBoard blackboard;
    public Transform target;

    public AnimationCurve walkBlendtreeCurve;
    public AnimationCurve runBlendtreeCurve;
    public AnimationCurve idleBlendtreeCurve;


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
    }


    void UpdateInterface()
    {
        Vector3 flattenedVelocity = target.position - lastPosition;

        //NOTE(Brian): Vertical speed
        float verticalVelocity = flattenedVelocity.y;
        blackboard.verticalSpeed = verticalVelocity;

        flattenedVelocity.y = 0;

        blackboard.movementSpeed = flattenedVelocity.magnitude;

        Vector3 rayOffset = Vector3.up * RAY_OFFSET_LENGTH;
        //NOTE(Brian): isGrounded?
        blackboard.isGrounded = Physics.Raycast(target.transform.position + rayOffset,
                                                Vector3.down,
                                                RAY_OFFSET_LENGTH - ELEVATION_OFFSET,
                                                DCLCharacterController.i.groundLayers);

#if UNITY_EDITOR
        Debug.DrawRay(target.transform.position + rayOffset, Vector3.down * (RAY_OFFSET_LENGTH - ELEVATION_OFFSET), blackboard.isGrounded ? Color.green : Color.red);
#endif

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
        animation[clips.run].normalizedSpeed = bb.movementSpeed / Time.deltaTime * bb.runSpeedFactor;
        animation[clips.walk].normalizedSpeed = bb.movementSpeed / Time.deltaTime * bb.walkSpeedFactor;

        float normalizedSpeed = bb.movementSpeed / Time.deltaTime / MAX_VELOCITY;

        float idleWeight = idleBlendtreeCurve.Evaluate(normalizedSpeed);
        float runWeight = runBlendtreeCurve.Evaluate(normalizedSpeed);
        float walkWeight = walkBlendtreeCurve.Evaluate(normalizedSpeed);

        //NOTE(Brian): Normalize weights
        float weightSum = idleWeight + runWeight + walkWeight;

        idleWeight /= weightSum;
        runWeight /= weightSum;
        walkWeight /= weightSum;

        animation.Blend(clips.idle, idleWeight, GROUND_BLENDTREE_TRANSITION_TIME);
        animation.Blend(clips.run, runWeight, GROUND_BLENDTREE_TRANSITION_TIME);
        animation.Blend(clips.walk, walkWeight, GROUND_BLENDTREE_TRANSITION_TIME);

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
            animation.CrossFade(clips.jump, JUMP_TRANSITION_TIME, PlayMode.StopAll);
        }
        else
        {
            animation.CrossFade(clips.fall, FALL_TRANSITION_TIME, PlayMode.StopAll);
        }

        if (bb.isGrounded)
        {
            animation.Blend(clips.jump, 0, AIR_EXIT_TRANSITION_TIME);
            animation.Blend(clips.fall, 0, AIR_EXIT_TRANSITION_TIME);
            currentState = State_Ground;
            Update();
        }
    }

    public void Reset()
    {
        //It will set the animation to the first frame, but due to the nature of the script and its Update. It wont stop the animation from playing
        animation.Stop();
    }
}
