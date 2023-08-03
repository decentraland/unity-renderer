using AvatarSystem;
using DCL;
using DCL.Emotes;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterAnimationController : MonoBehaviour, IAnimator
    {
        private static readonly int EMOTE_LOOP = Animator.StringToHash("EmoteLoop");
        private static readonly int MOVEMENT_BLEND = Animator.StringToHash("MovementBlend");
        private static readonly int EMOTE = Animator.StringToHash("Emote");
        private static readonly int GROUNDED = Animator.StringToHash("IsGrounded");
        private static readonly int JUMPING = Animator.StringToHash("IsJumping");
        private static readonly int FALLING = Animator.StringToHash("IsFalling");
        private static readonly int LONG_JUMP = Animator.StringToHash("IsLongJump");
        private static readonly int JUMP = Animator.StringToHash("Jump");
        private static readonly int EMOTE_REFRESH = Animator.StringToHash("EmoteRefresh");
        private static readonly int ANGLE = Animator.StringToHash("Angle");
        private static readonly int ANGLE_DIR = Animator.StringToHash("AngleDir");
        private static readonly int LONG_FALL = Animator.StringToHash("IsLongFall");
        private static readonly int STUNNED = Animator.StringToHash("IsStunned");

        [SerializeField] private DefaultLocomotionData[] defaultLocomotion;
        [SerializeField] private CharacterControllerData data;

        private readonly Dictionary<string, AnimatorOverrideController> locomotion = new ();
        private readonly Dictionary<string, EmoteClipData> externalClips = new ();
        private readonly List<KeyValuePair<AnimationClip,AnimationClip>> currentOverrides = new();

        private AnimatorOverrideController animatorOverrideController;
        private Animator animator;
        private CharacterState characterState;
        private bool isPlayingEmote;
        private Quaternion currentRotation;
        private GameObject viewContainer;

        private FollowWithDamping cameraFollow;

        private void Awake()
        {
            foreach (DefaultLocomotionData locomotionData in defaultLocomotion)
                locomotion.Add(locomotionData.bodyShapeId, locomotionData.animatorOverride);

            currentRotation = transform.rotation;
        }

        public void Prepare(string bodyshapeId, GameObject container)
        {
            viewContainer = container;
            animator = viewContainer.gameObject.GetOrCreateComponent<Animator>();
            viewContainer.gameObject.GetOrCreateComponent<StickerAnimationListener>();

            if (!locomotion.ContainsKey(bodyshapeId))
            {
                Debug.LogError("Body shape " + bodyshapeId + " has no default animations");
            }
            else
            {
                animatorOverrideController = new AnimatorOverrideController(locomotion[bodyshapeId]);
                animatorOverrideController.GetOverrides(currentOverrides);
                animator.runtimeAnimatorController = animatorOverrideController;
            }

            InitializeAvatarAudioAndParticleHandlers(this.viewContainer);

            cameraFollow.target = container.transform;
        }

        public void SetupCharacterState(CharacterState characterState)
        {
            this.characterState = characterState;
            characterState.OnJump += OnJump;
        }

        // todo: move this elsewhere
        private void InitializeAvatarAudioAndParticleHandlers(GameObject container)
        {
            AvatarAnimationEventHandler animationEventHandler = container.GetOrCreateComponent<AvatarAnimationEventHandler>();
            AudioContainer audioContainer = transform.GetComponentInChildren<AudioContainer>();

            if (audioContainer != null)
            {
                animationEventHandler.Init(audioContainer);

                AvatarAudioHandlerRemote audioHandlerRemote = audioContainer.GetComponent<AvatarAudioHandlerRemote>();

                if (audioHandlerRemote != null)
                    audioHandlerRemote.Init(container);
            }
        }

        private void OnJump()
        {
            animator.SetTrigger(JUMP);
        }

        private void LateUpdate()
        {
            if (characterState == null || animator == null) return;
            UpdateAnimatorState();
        }

        private void UpdateAnimatorState()
        {
            animator.speed = data.animationSpeed;
            var velocity = characterState.TotalVelocity;
            velocity.y = 0;
            var maxVelocity = characterState.MaxVelocity;
            var speedState = characterState.SpeedState;

            animator.SetBool(GROUNDED, characterState.IsGrounded);
            animator.SetBool(JUMPING, characterState.IsJumping);
            animator.SetBool(FALLING, characterState.IsFalling);
            animator.SetBool(LONG_JUMP, characterState.IsLongJump);
            animator.SetBool(LONG_FALL, characterState.IsLongFall);
            animator.SetBool(STUNNED, characterState.IsStunned);

            // state idle ----- walk ----- jog ----- run
            // blend  0  -----   1  -----  2  -----  3

            int movementBlendId = GetMovementBlendId(velocity, speedState);

            var currentBlend = 0f;

            if (maxVelocity > 0)
            {
                float velocityMagnitude = velocity.magnitude;
                currentBlend = velocityMagnitude / maxVelocity * movementBlendId;

                if (velocityMagnitude > 0)
                    StopEmote();
            }

            animator.SetFloat(MOVEMENT_BLEND, currentBlend);

            //Quaternion transformRotation = characterState.IsGrounded && velocity.sqrMagnitude > 0.1f ? Quaternion.LookRotation(-velocity.normalized) : transform.rotation;
            Quaternion transformRotation = transform.rotation;

            animator.SetFloat(ANGLE, Quaternion.Angle(currentRotation, transformRotation));
            animator.SetFloat(ANGLE_DIR, Quaternion.Dot(currentRotation, transformRotation));

            currentRotation = Quaternion.RotateTowards(currentRotation, transformRotation, data.rotationSpeed * Time.deltaTime * (1+currentBlend));
            viewContainer.transform.rotation = currentRotation;
        }

        private int GetMovementBlendId(Vector3 velocity, SpeedState speedState)
        {
            if (velocity.sqrMagnitude <= 0)
                return 0;

            return speedState switch
                   {
                       SpeedState.WALK => 1,
                       SpeedState.JOG => 2,
                       SpeedState.RUN => 3,
                       _ => throw new ArgumentOutOfRangeException(),
                   };
        }

        public void PlayEmote(string emoteId, long timestamps)
        {
            if (string.IsNullOrEmpty(emoteId))
                return;

            if (isPlayingEmote)
                // This trigger enforces the emote loop flip to flop so the animation re-starts
                animator.SetTrigger(EMOTE_REFRESH);

            if (externalClips.TryGetValue(emoteId, out EmoteClipData data))
            {
                animatorOverrideController["Emote"] = data.clip;
                animator.SetBool(EMOTE_LOOP, data.loop);
                animator.SetBool(EMOTE, true);
                isPlayingEmote = true;
            }
            else
                StopEmote();
        }

        private void StopEmote()
        {
            animator.SetBool(EMOTE, false);
            isPlayingEmote = false;
        }

        public void UnequipEmote(string emoteId)
        {
            externalClips.Remove(emoteId);
        }

        public void EquipEmote(string emoteId, EmoteClipData emoteClipData)
        {
            if (emoteClipData.clip.legacy)
            {
                Debug.LogWarning($"Watch out, {emoteClipData.clip.name} is legacy", emoteClipData.clip);
                return;
            }

            externalClips[emoteId] = emoteClipData;
        }

        public void PostStart(FollowWithDamping cameraFollow)
        {
            this.cameraFollow = cameraFollow;
        }
    }
}
