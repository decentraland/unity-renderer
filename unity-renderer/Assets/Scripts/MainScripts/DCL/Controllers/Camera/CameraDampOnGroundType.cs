using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraDampOnGroundType
    {
        private const float AFTER_TELEPORT_DAMP_TIME_MS = 50f;

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
        private float zeroDampTime;

        public CameraDampOnGroundType (Settings settings, FollowWithDamping followWithDamping)
        {
            this.settings = settings;
            this.followWithDamping = followWithDamping;
            DataStore.i.player.lastTeleportPosition.OnChange += OnTeleport;
        }
        private void OnTeleport(Vector3 current, Vector3 previous) { zeroDampTime = AFTER_TELEPORT_DAMP_TIME_MS; }

        public void Update(bool hitGround, RaycastHit hitInfo)
        {
            if ( !settings.enabled )
                return;

            UpdateDamp(hitGround, hitInfo);
            UpdateAfterTeleportDamp();
        }
        private void UpdateDamp(bool hitGround, RaycastHit hitInfo)
        {
            bool isOnMovingPlatform = DCLCharacterController.i.isOnMovingPlatform;

            if ( hitGround )
            {
                if ( hitInfo.distance < settings.groundCheckThreshold )
                    followWithDamping.damping.y = isOnMovingPlatform ? settings.dampingOnMovingPlatform : settings.dampingOnGround;
                else
                    followWithDamping.damping.y = settings.dampingOnAir;
            }
        }
        
        // This avoids the "Camera clips through stuff" after teleporting to different heights
        private void UpdateAfterTeleportDamp()
        {
            if (zeroDampTime > 0)
            {
                followWithDamping.damping.y = 0;
                zeroDampTime -= Time.unscaledDeltaTime;
            }
        }
    }
}