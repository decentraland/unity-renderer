using System;
using UnityEngine;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterState
    {
        public Vector3 FlatVelocity;
        public float MaxVelocity;
        public SpeedState SpeedState;
        public bool IsGrounded;
        public event Action OnJump = () => { };

        public void Jump()
        {
            OnJump.Invoke();
        }
    }
}
