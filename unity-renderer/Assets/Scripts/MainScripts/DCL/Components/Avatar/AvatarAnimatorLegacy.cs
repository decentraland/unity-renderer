using System;
using DCL.Components;
using UnityEngine;

public class AvatarAnimatorLegacy : MonoBehaviour, IPoolLifecycleHandler
{
    const float IDLE_TRANSITION_TIME = 0.2f;
    const float STRAFE_TRANSITION_TIME = 0.25f;
    const float RUN_TRANSITION_TIME = 0.15f;
    const float WALK_TRANSITION_TIME = 0.15f;
    const float JUMP_TRANSITION_TIME = 0.01f;
    const float FALL_TRANSITION_TIME = 0.5f;
    const float EXPRESSION_TRANSITION_TIME = 0.2f;

    const float AIR_EXIT_TRANSITION_TIME = 0.2f;
    const float GROUND_BLENDTREE_TRANSITION_TIME = 0.15f;

    const float RUN_SPEED_THRESHOLD = 0.05f;
    const float WALK_SPEED_THRESHOLD = 0.03f;

    const float ELEVATION_OFFSET = 0.6f;
    const float RAY_OFFSET_LENGTH = 3.0f;

    const float MAX_VELOCITY = 6.25f;

    [System.Serializable]
    public class BaseClipsIds
    {
        public string idle;
        public string walk;
        public string run;
        public string jump;
        public string fall;
    }

    [System.Serializable]
    public class BlackBoard
    {
        public float walkSpeedFactor;
        public float runSpeedFactor;
        public float movementSpeed;
        public float verticalSpeed;
        public bool isGrounded;
        public string expressionTriggerId;
        public long expressionTriggerTimestamp;
        public float deltaTime;
    }

    [SerializeField] internal AvatarAnimationsVariable maleAnimations;
    [SerializeField] internal AvatarAnimationsVariable femaleAnimations;

    public new Animation animation;
    public BaseClipsIds baseClipsIds;
    public BlackBoard blackboard;
    public Transform target;

    [SerializeField] float runMinSpeed = 6f;
    [SerializeField] float walkMinSpeed = 0.1f;

    internal System.Action<BlackBoard> currentState;

    Vector3 lastPosition;
    AvatarAnimationsVariable currentAnimations;
    bool isOwnPlayer = false;

    public void Start()
    {
        OnPoolGet();
    }

    public void OnPoolGet()
    {
        if (DCLCharacterController.i != null)
        {
            isOwnPlayer = DCLCharacterController.i.transform == transform.parent;

            // NOTE: disable MonoBehaviour's update to use DCLCharacterController event instead
            this.enabled = !isOwnPlayer;

            if (isOwnPlayer)
            {
                DCLCharacterController.i.OnUpdateFinish += Update;
            }
        }

        currentState = State_Init;
    }

    public void OnPoolRelease()
    {
        if (isOwnPlayer && DCLCharacterController.i)
        {
            DCLCharacterController.i.OnUpdateFinish -= Update;
        }
    }

    void Update()
    {
        Update(Time.deltaTime);
    }

    void Update(float deltaTime)
    {
        if (target == null || animation == null)
            return;

        blackboard.deltaTime = deltaTime;
        UpdateInterface();
        currentState?.Invoke(blackboard);
    }

    void UpdateInterface()
    {
        Vector3 velocityTargetPosition = target.position;
        Vector3 flattenedVelocity = velocityTargetPosition - lastPosition;

        //NOTE(Brian): Vertical speed
        float verticalVelocity = flattenedVelocity.y;
        blackboard.verticalSpeed = verticalVelocity;

        flattenedVelocity.y = 0;

        if (isOwnPlayer)
            blackboard.movementSpeed = flattenedVelocity.magnitude - DCLCharacterController.i.movingPlatformSpeed;
        else
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

        lastPosition = velocityTargetPosition;
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
        if (bb.deltaTime <= 0)
        {
            Debug.LogError("deltaTime should be > 0", gameObject);
            return;
        }

        animation[baseClipsIds.run].normalizedSpeed = bb.movementSpeed / bb.deltaTime * bb.runSpeedFactor;
        animation[baseClipsIds.walk].normalizedSpeed = bb.movementSpeed / bb.deltaTime * bb.walkSpeedFactor;

        float movementSpeed = bb.movementSpeed / bb.deltaTime;

        if (movementSpeed > runMinSpeed)
        {
            animation.CrossFade(baseClipsIds.run, RUN_TRANSITION_TIME);
        }
        else if (movementSpeed > walkMinSpeed)
        {
            animation.CrossFade(baseClipsIds.walk, WALK_TRANSITION_TIME);
        }
        else
        {
            animation.CrossFade(baseClipsIds.idle, IDLE_TRANSITION_TIME);
        }

        if (!bb.isGrounded)
        {
            currentState = State_Air;
            Update(bb.deltaTime);
        }
    }

    void State_Air(BlackBoard bb)
    {
        if (bb.verticalSpeed > 0)
        {
            animation.CrossFade(baseClipsIds.jump, JUMP_TRANSITION_TIME, PlayMode.StopAll);
        }
        else
        {
            animation.CrossFade(baseClipsIds.fall, FALL_TRANSITION_TIME, PlayMode.StopAll);
        }

        if (bb.isGrounded)
        {
            animation.Blend(baseClipsIds.jump, 0, AIR_EXIT_TRANSITION_TIME);
            animation.Blend(baseClipsIds.fall, 0, AIR_EXIT_TRANSITION_TIME);
            currentState = State_Ground;
            Update(bb.deltaTime);
        }
    }

    internal void State_Expression(BlackBoard bb)
    {
        var animationInfo = animation[bb.expressionTriggerId];
        animation.CrossFade(bb.expressionTriggerId, EXPRESSION_TRANSITION_TIME, PlayMode.StopAll);

        var mustExit = Math.Abs(bb.movementSpeed) > Mathf.Epsilon || animationInfo.length - animationInfo.time < EXPRESSION_TRANSITION_TIME || !bb.isGrounded;
        if (mustExit)
        {
            animation.Blend(bb.expressionTriggerId, 0, EXPRESSION_TRANSITION_TIME);
            bb.expressionTriggerId = null;
            if (!bb.isGrounded)
                currentState = State_Air;
            else
                currentState = State_Ground;

            Update(bb.deltaTime);
        }
    }

    public void SetExpressionValues(string expressionTriggerId, long expressionTriggerTimestamp)
    {
        if (animation == null)
            return;

        var mustTriggerAnimation = !string.IsNullOrEmpty(expressionTriggerId) && blackboard.expressionTriggerTimestamp != expressionTriggerTimestamp;

        blackboard.expressionTriggerId = expressionTriggerId;
        blackboard.expressionTriggerTimestamp = expressionTriggerTimestamp;

        if (mustTriggerAnimation)
        {
            if (!string.IsNullOrEmpty(expressionTriggerId))
            {
                animation.Stop(expressionTriggerId);
            }

            currentState = State_Expression;
            Update();
        }
    }

    public void Reset()
    {
        if (animation == null) return;

        //It will set the animation to the first frame, but due to the nature of the script and its Update. It wont stop the animation from playing
        animation.Stop();
    }

    public void SetIdleFrame()
    {
        animation.Play(baseClipsIds.idle);
    }

    public void BindBodyShape(Animation animation, string bodyShapeType, Transform target)
    {
        this.target = target;
        this.animation = animation;

        if (bodyShapeType.Contains(WearableLiterals.BodyShapes.MALE))
        {
            currentAnimations = maleAnimations;
        }
        else if (bodyShapeType.Contains(WearableLiterals.BodyShapes.FEMALE))
        {
            currentAnimations = femaleAnimations;
        }

        for (var i = 0; i < currentAnimations.Get().Length; i++)
        {
            var animationToId = currentAnimations.Get()[i];
            if (this.animation.GetClip(animationToId.id) == null)
            {
                // animationToId.id and animationToId.clip.name must be the same or we get big performance drop here
                // Already coordinated with art team to have the animations with the correct ids
                this.animation.AddClip(animationToId.clip, animationToId.id);
            }
        }

        SetIdleFrame();
    }
}
