using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;


namespace DCL.Camera
{
    /// <summary>
    /// This class ensures that upon sprinting forward, the camera slowly backs down.
    /// Camera will slowly recover when not sprinting or moving in any other direction that's not forward.
    /// </summary>
    public class CameraDampOnSprint
    {
        [System.Serializable]
        public class Settings
        {
            public bool enabled = true;
            public float distanceMin;
            public float distanceMax;
            public float fovMax;
            public float fovMin;
            public float inDampingDelay = 5;
            public float inDampingTime = 5f;
            public float outDampingTime = 2.5f;
        }

        private CinemachineFreeLook sourceFreeLook;
        private InputAction_Measurable characterYAxis;
        public Settings settings;

        private float currentDampingDelay = 0;

        public CameraDampOnSprint (Settings settings, CinemachineFreeLook freeLook, InputAction_Measurable characterYAxis)
        {
            this.settings = settings;
            this.sourceFreeLook = freeLook;
            this.characterYAxis = characterYAxis;
        }

        public void Update()
        {
            if ( !settings.enabled )
                return;

            var orbit = sourceFreeLook.m_Orbits[1];
            if (characterYAxis.GetValue() > 0 && !DCLCharacterController.i.isWalking)
            {
                currentDampingDelay += Time.deltaTime;

                if ( currentDampingDelay > settings.inDampingDelay )
                {
                    orbit.m_Radius += Damper.Damp(settings.distanceMax - orbit.m_Radius, settings.inDampingTime, Time.deltaTime);
                    sourceFreeLook.m_Lens.FieldOfView += Damper.Damp(settings.fovMax - sourceFreeLook.m_Lens.FieldOfView, settings.inDampingTime, Time.deltaTime);
                }
            }
            else
            {
                currentDampingDelay = 0;
                orbit.m_Radius += Damper.Damp(settings.distanceMin - orbit.m_Radius, settings.outDampingTime, Time.deltaTime);
                sourceFreeLook.m_Lens.FieldOfView += Damper.Damp(settings.fovMin - sourceFreeLook.m_Lens.FieldOfView, settings.outDampingTime, Time.deltaTime);
            }

            sourceFreeLook.m_Orbits[1] = orbit;
        }
    }
}