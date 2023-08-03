using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    //[CreateAssetMenu(menuName = "CharacterControllerData", fileName = "CharacterControllerData", order = 0)]
    public class CharacterControllerData : ScriptableObject
    {
        public float walkSpeed = 1;
        public float jogSpeed = 3;
        public float runSpeed = 5;
        public float acceleration = 5;
        public float airAcceleration = 5;
        public float stopTimeSec = 0.12f;
        public float gravity = -9.8f;
        public float jogJumpHeight = 3f;
        public float walkJumpHeight = 1.5f;
        public float runJumpHeight = 5f;
        public float jumpGraceTime = 0.15f;
        public float rotationSpeed = 360f;
        public float longJumpTime = 0.5f;
        public float longJumpGravityScale = 0.5f;
        public float groundDrag = 0.5f;
        public float airDrag = 0.25f;
        public float minImpulse = 1f;
        public float jumpPadForce = 50f;
        public float jumpGravityFactor = 2;
        public float animationSpeed = 1;
        public AnimationCurve accelerationCurve;
        public float maxAcceleration = 25f;
        public float accelerationTime = 0.5f;
        public float longFallStunTime = 0.75f;
        public float noSlipDistance = 0.1f;
        public float slipSpeedMultiplier = 1.2f;
        public float jumpHeightStun = 10f;
        public float jumpVelocityDrag = 3f;
    }
}
