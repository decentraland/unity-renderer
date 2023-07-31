using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterState
    {
        public const float HEIGHT = 1.6f;
        public Vector3 FlatVelocity;
        public float MaxVelocity;
        public SpeedState SpeedState;
        public bool IsGrounded;
        public event Action OnJump = () => { };
        public bool IsJumping;
        public bool IsLongJump;
        public Vector3 TotalVelocity;
        public bool IsFalling;

        public void Jump()
        {
            OnJump.Invoke();
        }
    }
}
