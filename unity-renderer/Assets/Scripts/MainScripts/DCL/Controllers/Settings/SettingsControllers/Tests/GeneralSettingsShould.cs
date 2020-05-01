using Cinemachine;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.TestTools;

using QualitySettings = DCL.SettingsData.QualitySettings;
using GeneralSettings = DCL.SettingsData.GeneralSettings;
using GeneralSettingsController = DCL.SettingsController.GeneralSettingsController;
using QualitySettingsController = DCL.SettingsController.QualitySettingsController;

namespace Tests
{
    public class GeneralSettingsShould : TestsBase
    {
        QualitySettings initQualitySettings;
        QualitySettings testQualitySettings;
        GeneralSettings testGeneralSettings;

        CinemachineFreeLook freeLookCamera;
        CinemachineVirtualCamera firstPersonCamera;
        CinemachinePOV povCamera;
        Light environmentLight;
        PostProcessVolume postProcessVolume;
        LightweightRenderPipelineAsset lwrpAsset;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            testQualitySettings = new QualitySettings()
            {
                textureQuality = QualitySettings.TextureQuality.HalfRes,
                antiAliasing = MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.LWRP.ShadowResolution._512,
                cameraDrawDistance = 50.1f,
                bloom = false,
                colorGrading = true
            };

            testGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };

            initQualitySettings = DCL.Settings.i.qualitySettings;
            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);

            yield return InitScene();
        }

        public IEnumerator InitScene()
        {
            yield return InitUnityScene("InitialScene");
            GameObject.DestroyImmediate(DCL.WSSController.i.gameObject);
        }

        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();
            DCL.Settings.i.ApplyQualitySettings(initQualitySettings);
        }

        [UnityTest]
        public IEnumerator HaveItControllersSetupCorrectly()
        {
            GeneralSettingsController generalSettingsController = GameObject.FindObjectOfType<GeneralSettingsController>();
            QualitySettingsController qualitySettingsController = GameObject.FindObjectOfType<QualitySettingsController>();

            Assert.IsNotNull(generalSettingsController, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsController, "QualitySettingsController not found in scene");
            Assert.IsNotNull(generalSettingsController.thirdPersonCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsController.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(virtualCamera.GetCinemachineComponent<CinemachinePOV>(), "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            Assert.IsNotNull(qualitySettingsController.environmentLight, "QualitySettingsController: environmentLight reference missing");
            Assert.IsNotNull(qualitySettingsController.postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");
            Assert.IsNotNull(qualitySettingsController.firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsController.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
            yield break;
        }

        [UnityTest]
        public IEnumerator HaveQualityPresetSetCorrectly()
        {
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.Length > 0, "QualitySettingsData: No presets created");
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.defaultIndex > 0
                && DCL.Settings.i.qualitySettingsPresets.defaultIndex < DCL.Settings.i.qualitySettingsPresets.Length, "QualitySettingsData: Wrong default preset index");
            yield break;
        }

        [UnityTest]
        public IEnumerator ApplyCorrectly()
        {
            // NOTE: settings here were set before scene loading
            SetupReferences();

            CheckIfTestQualitySettingsAreSet();
            CheckIfTestGeneralSettingsAreSet();

            // NOTE: wait for next frame
            yield return null;

            CheckIfQualitySettingsAreApplied();
            CheckIfGeneralSettingsAreApplied();

            // NOTE: change settings in runtime
            testQualitySettings = DCL.Settings.i.qualitySettingsPresets[0];
            testGeneralSettings = new GeneralSettings()
            {
                sfxVolume = 1,
                mouseSensitivity = 0
            };
            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);

            CheckIfTestQualitySettingsAreSet();
            CheckIfTestGeneralSettingsAreSet();

            // NOTE: wait for next frame
            yield return null;

            CheckIfQualitySettingsAreApplied();
            CheckIfGeneralSettingsAreApplied();

            yield break;
        }

        public void SetupReferences()
        {
            lwrpAsset = GraphicsSettings.renderPipelineAsset as LightweightRenderPipelineAsset;
            GeneralSettingsController generalSettingsController = GameObject.FindObjectOfType<GeneralSettingsController>();
            QualitySettingsController qualitySettingsController = GameObject.FindObjectOfType<QualitySettingsController>();

            Assert.IsNotNull(generalSettingsController, "GeneralSettingsController not found in scene");
            Assert.IsNotNull(qualitySettingsController, "QualitySettingsController not found in scene");

            freeLookCamera = generalSettingsController.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsController.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            environmentLight = qualitySettingsController.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = qualitySettingsController.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = qualitySettingsController.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsController.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        private void CheckIfTestQualitySettingsAreSet()
        {
            Assert.IsTrue(DCL.Settings.i.qualitySettings.Equals(testQualitySettings), "Quality Setting mismatch");
        }

        private void CheckIfTestGeneralSettingsAreSet()
        {
            Assert.IsTrue(DCL.Settings.i.generalSettings.Equals(testGeneralSettings), "General Settings mismatch");
        }

        private void CheckIfQualitySettingsAreApplied()
        {
            Assert.IsTrue(lwrpAsset.msaaSampleCount == (int)DCL.Settings.i.qualitySettings.antiAliasing, "antiAliasing mismatch");
            Assert.IsTrue(lwrpAsset.renderScale == DCL.Settings.i.qualitySettings.renderScale, "renderScale mismatch");
            Assert.IsTrue(lwrpAsset.supportsMainLightShadows == DCL.Settings.i.qualitySettings.shadows, "shadows mismatch");
            Assert.IsTrue(lwrpAsset.supportsSoftShadows == DCL.Settings.i.qualitySettings.softShadows, "softShadows mismatch");
            Assert.IsTrue(lwrpAsset.mainLightShadowmapResolution == (int)DCL.Settings.i.qualitySettings.shadowResolution, "shadowResolution mismatch");

            LightShadows shadowType = LightShadows.None;
            if (DCL.Settings.i.qualitySettings.shadows)
            {
                shadowType = DCL.Settings.i.qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
            }
            Assert.IsTrue(environmentLight.shadows == shadowType, "shadows (environmentLight) mismatch");
            Bloom bloom;
            if (postProcessVolume.profile.TryGetSettings(out bloom))
            {
                Assert.IsTrue(bloom.enabled.value == DCL.Settings.i.qualitySettings.bloom, "bloom mismatch");
            }
            ColorGrading colorGrading;
            if (postProcessVolume.profile.TryGetSettings(out colorGrading))
            {
                Assert.IsTrue(colorGrading.enabled.value == DCL.Settings.i.qualitySettings.colorGrading, "colorGrading mismatch");
            }
            Assert.IsTrue(firstPersonCamera.m_Lens.FarClipPlane == DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (firstPersonCamera) mismatch");
            Assert.IsTrue(freeLookCamera.m_Lens.FarClipPlane == DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (freeLookCamera) mismatch");
        }

        private void CheckIfGeneralSettingsAreApplied()
        {
            Assert.IsTrue(freeLookCamera.m_XAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_XAxis) mouseSensitivity mismatch");
            Assert.IsTrue(freeLookCamera.m_YAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_YAxis) mouseSensitivity mismatch");
            Assert.IsTrue(povCamera.m_HorizontalAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_HorizontalAxis) mouseSensitivity mismatch");
            Assert.IsTrue(povCamera.m_VerticalAxis.m_AccelTime == DCL.Settings.i.generalSettings.mouseSensitivity, "freeLookCamera (m_VerticalAxis) mouseSensitivity mismatch");
            Assert.IsTrue(AudioListener.volume == DCL.Settings.i.generalSettings.sfxVolume, "audioListener sfxVolume mismatch");
        }
    }
}
