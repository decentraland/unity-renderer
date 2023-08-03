using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterState
    {
        public const float HEIGHT = 1.6f;
        public float MaxVelocity;
        public SpeedState SpeedState;
        public bool IsGrounded;
        public event Action OnJump = () => { };
        public bool IsJumping;
        public bool IsLongJump;
        public bool IsLongFall;
        public Vector3 TotalVelocity;
        public bool IsFalling;
        public Vector3 ExternalImpulse;
        public Vector3 ExternalVelocity;
        public float currentAcceleration;
        public bool IsStunned;

        public void Jump()
        {
            OnJump.Invoke();
        }
    }
}
