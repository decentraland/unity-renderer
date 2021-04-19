using Cinemachine;
using UnityEngine;

namespace DCL.SettingsController
{
    /// <summary>
    /// This MonoBehaviour will only contain the external references needed for the general settings.
    /// </summary>
    public class GeneralSettingsReferences : MonoBehaviour
    {
        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        public static GeneralSettingsReferences i { get; private set; }

        private void Awake()
        {
            i = this;
        }
    }
}