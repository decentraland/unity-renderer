using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraDampOnGroundType
    {
        [System.Serializable]
        public class Settings
        {
            public bool enabled = true;
            public float dampingOnAir;
            public float dampingOnGround;
            public float dampingOnMovingPlatform;

            [Tooltip("Min raycast ground distance to be considered airborne")]
            public float groundCheckThreshold;
        }

        private FollowWithDamping followWithDamping;
        public Settings settings;

        public CameraDampOnGroundType (Settings settings, FollowWithDamping followWithDamping)
        {
            this.settings = settings;
            this.followWithDamping = followWithDamping;
        }

        public void Update(bool hitGround, RaycastHit hitInfo)
        {
            if ( !settings.enabled )
                return;

            bool isOnMovingPlatform = DCLCharacterController.i.isOnMovingPlatform;

            if ( hitGround )
            {
                if ( hitInfo.distance < settings.groundCheckThreshold )
                    followWithDamping.damping.y = isOnMovingPlatform ? settings.dampingOnMovingPlatform : settings.dampingOnGround;
                else
                    followWithDamping.damping.y = settings.dampingOnAir;
            }
        }
    }
}