using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterState
    {
        public event Action OnJump = () => { };
        public event Action OnWallHit = () => { };
        public event Action OnWallHitReset = () => { };

        public SpeedState SpeedState;
        public Vector3 TotalVelocity;

        public float MaxVelocity;
        public bool IsGrounded;
        public bool IsJumping;
        public bool IsLongJump;
        public bool IsLongFall;
        public bool IsFalling;
        public bool IsStunned;

        public void Jump()
        {
            OnJump.Invoke();
        }

        public void WallHit()
        {
            OnWallHit.Invoke();
        }

        public void ResetWallHit()
        {
            OnWallHitReset.Invoke();
        }
    }
}
