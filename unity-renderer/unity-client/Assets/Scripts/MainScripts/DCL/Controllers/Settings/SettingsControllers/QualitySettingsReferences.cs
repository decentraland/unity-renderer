using Cinemachine;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL.SettingsController
{
    /// <summary>
    /// This MonoBehaviour will only contain the external references needed for the quality settings.
    /// </summary>
    public class QualitySettingsReferences : MonoBehaviour
    {
        public Light environmentLight = null;
        public Volume postProcessVolume = null;
        public CinemachineFreeLook thirdPersonCamera = null;
        public CinemachineVirtualCamera firstPersonCamera = null;
        public CullingControllerSettingsData cullingControllerSettingsData = null;

        public static QualitySettingsReferences i { get; private set; }

        private void Awake()
        {
            i = this;
        }
    }
}