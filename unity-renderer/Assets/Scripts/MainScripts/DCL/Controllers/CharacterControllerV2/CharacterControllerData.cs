using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    // This config was separated into different mechanic headers for better readability
    public class CharacterControllerData : ScriptableObject
    {
        [Header("General config")]
        public float walkSpeed = 1;
        public float jogSpeed = 3;
        public float runSpeed = 5;
        public float airAcceleration = 5;
        public float gravity = -9.8f;
        public float jogJumpHeight = 3f;
        public float runJumpHeight = 5f;
        public float characterControllerRadius = 0.5f;

        [Header("Impulse Specifics")]
        public float groundDrag = 0.5f;
        public float airDrag = 0.25f;
        public float minImpulse = 1f;

        [Header("Velocity Drag")]
        public float jumpVelocityDrag = 3f;

        [Header("Smooth acceleration")]
        public AnimationCurve accelerationCurve;
        public float acceleration = 5;
        public float maxAcceleration = 25f;
        public float accelerationTime = 0.5f;

        [Header("De-acceleration dampening")]
        public float stopTimeSec = 0.12f;

        [Header("Long Jump")]
        public float longJumpTime = 0.5f;
        public float longJumpGravityScale = 0.5f;

        [Header("Faster Jumps")]
        public float jumpGravityFactor = 2;

        [Header("Coyote timer")]
        public float jumpGraceTime = 0.15f;

        [Header("Hard fall stun")]
        public float jumpHeightStun = 10f;
        public float longFallStunTime = 0.75f;

        [Header("Edge slip")]
        public float noSlipDistance = 0.1f;
        public float edgeSlipSpeed = 1.2f;

        [Header("Animation")]
        public float rotationSpeed = 360f;
        public float movAnimBlendSpeed = 3f;

        [Header("Cheat/Debug/Misc")]
        public float jumpPadForce = 50f;
        public float animationSpeed = 1;
    }
}
