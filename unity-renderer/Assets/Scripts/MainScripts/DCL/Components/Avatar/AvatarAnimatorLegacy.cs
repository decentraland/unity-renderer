using System;
using AvatarSystem;
using DCL;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using Environment = DCL.Environment;

public enum AvatarAnimation
{
    IDLE,
    RUN,
    WALK,
    EMOTE,
    JUMP,
    FALL,
}

public class AvatarAnimatorLegacy : MonoBehaviour, IPoolLifecycleHandler, IAnimator
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
    
    // Time it takes to determine if a character is grounded when vertical velocity is 0
    const float FORCE_GROUND_TIME = 0.05f;
    
    // Minimum vertical speed used to consider whether an avatar is on air
    const float MIN_VERTICAL_SPEED_AIR = 0.025f;


    [System.Serializable]
    public class AvatarLocomotion
    {
        public AnimationClip idle;
        public AnimationClip walk;
        public AnimationClip run;
        public AnimationClip jump;
        public AnimationClip fall;
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

    [SerializeField] internal AvatarLocomotion femaleLocomotions;
    [SerializeField] internal AvatarLocomotion maleLocomotions;
    AvatarLocomotion currentLocomotions;

    public new Animation animation;
    public BlackBoard blackboard;
    public Transform target;

    [SerializeField] float runMinSpeed = 6f;
    [SerializeField] float walkMinSpeed = 0.1f;

    internal System.Action<BlackBoard> currentState;

    Vector3 lastPosition;
    bool isOwnPlayer = false;
    private AvatarAnimationEventHandler animEventHandler;
    
    private float lastOnAirTime = 0;
    
    private string runAnimationName;
    private string walkAnimationName;
    private string idleAnimationName;
    private string jumpAnimationName;
    private string fallAnimationName;
    private AvatarAnimation latestAnimation;
    private AnimationState runAnimationState;
    private AnimationState walkAnimationState;
    
    private Ray rayCache;

    public void Start()
    {
        OnPoolGet();
    }
    
    // AvatarSystem entry points
    public bool Prepare(string bodyshapeId, GameObject container)
    {
        if (!container.transform.TryFindChildRecursively("Armature", out Transform armature))
        {
            Debug.LogError($"Couldn't find Armature for AnimatorLegacy in path: {transform.GetHierarchyPath()}");
            return false;
        }
        Transform armatureParent = armature.parent;
        animation = armatureParent.gameObject.GetOrCreateComponent<Animation>();
        armatureParent.gameObject.GetOrCreateComponent<StickerAnimationListener>();

        PrepareLocomotionAnims(bodyshapeId);
        SetIdleFrame();
        animation.Sample();
        InitializeAvatarAudioAndParticleHandlers(animation);

        if (isOwnPlayer)
        {
            DCLCharacterController.i.OnUpdateFinish += OnUpdateWithDeltaTime;
        }
        else
        {
            Environment.i.platform.updateEventHandler.AddListener(IUpdateEventHandler.EventType.Update, OnEventHandlerUpdate);
        }

        return true;
    }

    private void PrepareLocomotionAnims(string bodyshapeId)
    {
        if (bodyshapeId.Contains(WearableLiterals.BodyShapes.MALE))
        {
            currentLocomotions = maleLocomotions;
        }
        else if (bodyshapeId.Contains(WearableLiterals.BodyShapes.FEMALE))
        {
            currentLocomotions = femaleLocomotions;
        }

        EquipEmote(currentLocomotions.idle.name, currentLocomotions.idle);
        EquipEmote(currentLocomotions.walk.name, currentLocomotions.walk);
        EquipEmote(currentLocomotions.run.name, currentLocomotions.run);
        EquipEmote(currentLocomotions.jump.name, currentLocomotions.jump);
        EquipEmote(currentLocomotions.fall.name, currentLocomotions.fall);
        
        idleAnimationName = currentLocomotions.idle.name;
        walkAnimationName = currentLocomotions.walk.name;
        runAnimationName = currentLocomotions.run.name;
        jumpAnimationName = currentLocomotions.jump.name;
        fallAnimationName = currentLocomotions.fall.name;
        
        runAnimationState = animation[runAnimationName];
        walkAnimationState = animation[walkAnimationName];
    }
    private void OnEventHandlerUpdate()
    {
        OnUpdateWithDeltaTime(Time.deltaTime);
    }

    public void OnPoolGet()
    {
        if (DCLCharacterController.i != null)
        {
            isOwnPlayer = DCLCharacterController.i.transform == transform.parent;

            // NOTE: disable MonoBehaviour's update to use DCLCharacterController event instead
            this.enabled = !isOwnPlayer;
        }

        currentState = State_Init;
    }

    public void OnPoolRelease()
    {
        if (isOwnPlayer && DCLCharacterController.i)
        {
            DCLCharacterController.i.OnUpdateFinish -= OnUpdateWithDeltaTime;
        }
        else
        {
            Environment.i.platform.updateEventHandler.RemoveListener(IUpdateEventHandler.EventType.Update, OnEventHandlerUpdate);
        }
    }
    
    void OnUpdateWithDeltaTime(float deltaTime)
    {
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

        //NOTE(Kinerius): if we have more or less than zero we consider that we are either jumping or falling
        if (Mathf.Abs(verticalVelocity) > MIN_VERTICAL_SPEED_AIR)
        {
            lastOnAirTime = Time.time;
        }
        
        blackboard.verticalSpeed = verticalVelocity;

        flattenedVelocity.y = 0;

        if (isOwnPlayer)
            blackboard.movementSpeed = flattenedVelocity.magnitude - DCLCharacterController.i.movingPlatformSpeed;
        else
            blackboard.movementSpeed = flattenedVelocity.magnitude;

        Vector3 rayOffset = Vector3.up * RAY_OFFSET_LENGTH;
        
        //NOTE(Kinerius): This check is just for the playing character, it uses a combination of collision flags and raycasts to determine the ground state, its precise
        bool isGroundedByCharacterController = isOwnPlayer && DCLCharacterController.i.isGrounded;
        
        //NOTE(Kinerius): This check is for interpolated avatars (the other players) as we dont have a Character Controller, we determine their ground state by checking its vertical velocity
        //                this check is cheap and fast but not precise
        bool isGroundedByVelocity = !isOwnPlayer && Time.time - lastOnAirTime > FORCE_GROUND_TIME;

        //NOTE(Kinerius): This additional check is both for the player and interpolated avatars, we cast an additional raycast per avatar to check ground state
        bool isGroundedByRaycast = false;
        if (!isGroundedByCharacterController && !isGroundedByVelocity)
        {
            rayCache.origin = velocityTargetPosition + rayOffset;
            rayCache.direction = Vector3.down;

            isGroundedByRaycast = Physics.Raycast(rayCache,
                RAY_OFFSET_LENGTH - ELEVATION_OFFSET,
                DCLCharacterController.i.groundLayers);

        }

        blackboard.isGrounded = isGroundedByCharacterController || isGroundedByVelocity || isGroundedByRaycast;

        lastPosition = velocityTargetPosition;
    }

    void State_Init(BlackBoard bb)
    {
        if (bb.isGrounded)
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
        if (bb.deltaTime <= 0) return;

        float movementSpeed = bb.movementSpeed / bb.deltaTime;

        runAnimationState.normalizedSpeed = movementSpeed * bb.runSpeedFactor;
        walkAnimationState.normalizedSpeed = movementSpeed * bb.walkSpeedFactor;
        
        if (movementSpeed > runMinSpeed)
        {
            CrossFadeTo(AvatarAnimation.RUN, runAnimationName, RUN_TRANSITION_TIME);
        }
        else if (movementSpeed > walkMinSpeed)
        {
            CrossFadeTo(AvatarAnimation.WALK, walkAnimationName, WALK_TRANSITION_TIME);
        }
        else 
        {
            CrossFadeTo(AvatarAnimation.IDLE, idleAnimationName, IDLE_TRANSITION_TIME);
        }

        if (!bb.isGrounded)
        {
            currentState = State_Air;
            OnUpdateWithDeltaTime(bb.deltaTime);
        }
    }
    private void CrossFadeTo(AvatarAnimation avatarAnimation, string animationName, float runTransitionTime, PlayMode playMode = PlayMode.StopSameLayer)
    {
        if (latestAnimation == avatarAnimation)
            return;

        animation.CrossFade(animationName, runTransitionTime, playMode);
        latestAnimation = avatarAnimation;
    }

    void State_Air(BlackBoard bb)
    {
        if (bb.verticalSpeed > 0)
        {
            CrossFadeTo(AvatarAnimation.JUMP, jumpAnimationName, JUMP_TRANSITION_TIME, PlayMode.StopAll);
        }
        else
        {
            CrossFadeTo(AvatarAnimation.FALL, fallAnimationName, FALL_TRANSITION_TIME, PlayMode.StopAll);
        }

        if (bb.isGrounded)
        {
            animation.Blend(jumpAnimationName, 0, AIR_EXIT_TRANSITION_TIME);
            animation.Blend(fallAnimationName, 0, AIR_EXIT_TRANSITION_TIME);
            currentState = State_Ground;
            OnUpdateWithDeltaTime(bb.deltaTime);
        }
    }

    internal void State_Expression(BlackBoard bb)
    {
        var animationInfo = animation[bb.expressionTriggerId];
        latestAnimation = AvatarAnimation.IDLE;
        CrossFadeTo(AvatarAnimation.EMOTE, bb.expressionTriggerId, EXPRESSION_TRANSITION_TIME, PlayMode.StopAll);
        bool mustExit;

        //Introduced the isMoving variable that is true if there is user input, substituted the old Math.Abs(bb.movementSpeed) > Mathf.Epsilon that relies of too much precision
        if (isOwnPlayer) 
            mustExit = DCLCharacterController.i.isMovingByUserInput || animationInfo.length - animationInfo.time < EXPRESSION_TRANSITION_TIME || !bb.isGrounded;
        else
            mustExit = Math.Abs(bb.movementSpeed) > 0.07f || animationInfo.length - animationInfo.time < EXPRESSION_TRANSITION_TIME || !bb.isGrounded;

        if (mustExit)
        {
            animation.Blend(bb.expressionTriggerId, 0, EXPRESSION_TRANSITION_TIME);
            bb.expressionTriggerId = null;
            if (!bb.isGrounded)
                currentState = State_Air;
            else
                currentState = State_Ground;

            OnUpdateWithDeltaTime(bb.deltaTime);
        }
        else
        {
            animation.Blend(bb.expressionTriggerId, 1, EXPRESSION_TRANSITION_TIME / 2f);
        }
    }

    public void SetExpressionValues(string expressionTriggerId, long expressionTriggerTimestamp)
    {
        if (animation == null)
            return;

        if (string.IsNullOrEmpty(expressionTriggerId))
            return;

        if (animation.GetClip(expressionTriggerId) == null)
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
            OnUpdateWithDeltaTime(Time.deltaTime);
        }
    }

    public void Reset()
    {
        if (animation == null)
            return;

        //It will set the animation to the first frame, but due to the nature of the script and its Update. It wont stop the animation from playing
        animation.Stop();
    }

    public void SetIdleFrame() { animation.Play(currentLocomotions.idle.name); }

    public void PlayEmote(string emoteId, long timestamps) { SetExpressionValues(emoteId, timestamps); }

    public void EquipEmote(string emoteId, AnimationClip clip)
    {
        if (animation == null)
            return;

        if (animation.GetClip(emoteId) != null)
            animation.RemoveClip(emoteId);
        animation.AddClip(clip, emoteId);
    }

    public void UnequipEmote(string emoteId)
    {
        if (animation == null)
            return;

        if (animation.GetClip(emoteId) == null)
            return;
        animation.RemoveClip(emoteId);
    }

    private void InitializeAvatarAudioAndParticleHandlers(Animation createdAnimation)
    {
        //NOTE(Mordi): Adds handler for animation events, and passes in the audioContainer for the avatar
        AvatarAnimationEventHandler animationEventHandler = createdAnimation.gameObject.GetOrCreateComponent<AvatarAnimationEventHandler>();
        AudioContainer audioContainer = transform.GetComponentInChildren<AudioContainer>();
        if (audioContainer != null)
        {
            animationEventHandler.Init(audioContainer);

            //NOTE(Mordi): If this is a remote avatar, pass the animation component so we can keep track of whether it is culled (off-screen) or not
            AvatarAudioHandlerRemote audioHandlerRemote = audioContainer.GetComponent<AvatarAudioHandlerRemote>();
            if (audioHandlerRemote != null)
            {
                audioHandlerRemote.Init(createdAnimation.gameObject);
            }
        }

        animEventHandler = animationEventHandler;
    }

    private void OnDestroy()
    {
        if (animEventHandler != null)
            Destroy(animEventHandler);
    }
}