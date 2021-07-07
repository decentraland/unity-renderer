using Cinemachine;
using UnityEngine;

namespace DCL.Camera
{
    public class CameraFreefall
    {
        [System.Serializable]
        public class Settings
        {
            public bool enabled = true;

            public CinemachineVirtualCamera freefallVirtualCamera;

            public float fallDetectionDelay;

            [Tooltip("Min raycast ground distance to be considered a fall")]
            public float fallGroundDistanceThreshold;
        }

        public Settings settings;
        private CinemachineFreeLook defaultFreeLookCamera;

        private Vector3 lastPosition;
        private float fallDetectionTimer;
        protected Vector3Variable characterPosition => CommonScriptableObjects.playerUnityPosition;

        public CameraFreefall (Settings settings, CinemachineFreeLook defaultFreeLookCamera)
        {
            this.defaultFreeLookCamera = defaultFreeLookCamera;
            this.settings = settings;
        }

        public void Update(bool hitGround, RaycastHit hitInfo)
        {
            if ( !settings.enabled )
                return;

            bool useFallingCamera = false;

            if ( hitGround )
            {
                useFallingCamera = hitInfo.distance > settings.fallGroundDistanceThreshold;
            }
            else
            {
                useFallingCamera = true;
            }


            Vector3 velocity = characterPosition - lastPosition;

            if ( velocity.y < 0 )
            {
                fallDetectionTimer += Time.deltaTime;

                if ( fallDetectionTimer < settings.fallDetectionDelay )
                    useFallingCamera = false;
            }
            else
            {
                fallDetectionTimer = 0;
            }

            lastPosition = characterPosition;

            if ( settings.freefallVirtualCamera.gameObject.activeSelf != useFallingCamera)
                settings.freefallVirtualCamera.gameObject.SetActive(useFallingCamera);

            if ( useFallingCamera )
            {
                var orbitalTransposer = settings.freefallVirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

                if ( orbitalTransposer != null )
                    orbitalTransposer.m_XAxis = defaultFreeLookCamera.m_XAxis;
            }
        }
    }
}