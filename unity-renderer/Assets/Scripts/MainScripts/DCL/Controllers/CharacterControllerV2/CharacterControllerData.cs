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
        public float stopTimeSec = 0.25f;
        public float gravity = -9.8f;
        public float jogJumpHeight = 3f;
        public float walkJumpHeight = 1.5f;
        public float runJumpHeight = 5f;
        public float jumpGraceTime = 0.15f;
        public float rotationSpeed = 360f;
        public float jumpFakeTime = 0.15f;
        public float jumpFakeCatchupSpeed = 25f;
        public float longJumpTime = 0.5f;
        public float longJumpGravityScale = 0.5f;
    }
}
