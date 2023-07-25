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
        public float gravity = -9.8f;
        public float jumpHeight = 2;
    }
}
