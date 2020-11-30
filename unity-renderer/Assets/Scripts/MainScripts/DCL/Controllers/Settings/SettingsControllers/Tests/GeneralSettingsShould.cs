using Cinemachine;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using GeneralSettings = DCL.SettingsData.GeneralSettings;
using GeneralSettingsController = DCL.SettingsController.GeneralSettingsController;
using QualitySettings = DCL.SettingsData.QualitySettings;
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

        Volume postProcessVolume;
        UniversalRenderPipelineAsset urpAsset;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            testQualitySettings = new QualitySettings()
            {
                baseResolution = QualitySettings.BaseResolution.BaseRes_720,
                antiAliasing = MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = true,
                shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512,
                cameraDrawDistance = 50.1f,
                bloom = false,
                colorGrading = true,
                detailObjectCullingThreshold = 0,
                enableDetailObjectCulling = true
            };

            testGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };

            initQualitySettings = DCL.Settings.i.qualitySettings;
            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);
        }

        protected override IEnumerator TearDown()
        {
            DCL.Settings.i.ApplyQualitySettings(initQualitySettings);
            yield return base.TearDown();
        }

        [Test]
        public void HaveItControllersSetupCorrectly()
        {
            GeneralSettingsController generalSettingsController = Object.FindObjectOfType<GeneralSettingsController>();
            QualitySettingsController qualitySettingsController = Object.FindObjectOfType<QualitySettingsController>();

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
        }

        [Test]
        public void HaveQualityPresetSetCorrectly()
        {
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.Length > 0, "QualitySettingsData: No presets created");
            Assert.IsTrue(DCL.Settings.i.qualitySettingsPresets.defaultIndex > 0
                          && DCL.Settings.i.qualitySettingsPresets.defaultIndex < DCL.Settings.i.qualitySettingsPresets.Length, "QualitySettingsData: Wrong default preset index");
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
        }

        public void SetupReferences()
        {
            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
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
            Assert.IsTrue(urpAsset.msaaSampleCount == (int) DCL.Settings.i.qualitySettings.antiAliasing, "antiAliasing mismatch");
            Assert.IsTrue(urpAsset.renderScale == DCL.Settings.i.qualitySettings.renderScale, "renderScale mismatch");
            Assert.IsTrue(urpAsset.supportsMainLightShadows == DCL.Settings.i.qualitySettings.shadows, "shadows mismatch");
            Assert.IsTrue(urpAsset.supportsSoftShadows == DCL.Settings.i.qualitySettings.softShadows, "softShadows mismatch");
            Assert.IsTrue(urpAsset.mainLightShadowmapResolution == (int) DCL.Settings.i.qualitySettings.shadowResolution, "shadowResolution mismatch");

            LightShadows shadowType = LightShadows.None;
            if (DCL.Settings.i.qualitySettings.shadows)
            {
                shadowType = DCL.Settings.i.qualitySettings.softShadows ? LightShadows.Soft : LightShadows.Hard;
            }

            Assert.IsTrue(environmentLight.shadows == shadowType, "shadows (environmentLight) mismatch");

            if (postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
            {
                Assert.IsTrue(bloom.active == DCL.Settings.i.qualitySettings.bloom, "bloom mismatch");
            }

            if (postProcessVolume.profile.TryGet<Tonemapping>(out Tonemapping toneMapping))
            {
                Assert.IsTrue(toneMapping.active == DCL.Settings.i.qualitySettings.colorGrading, "colorGrading mismatch");
            }

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(firstPersonCamera.m_Lens.FarClipPlane, DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (firstPersonCamera) mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_Lens.FarClipPlane, DCL.Settings.i.qualitySettings.cameraDrawDistance, "cameraDrawDistance (freeLookCamera) mismatch");
        }

        private void CheckIfGeneralSettingsAreApplied()
        {
            var povSpeed = Mathf.Lerp(GeneralSettingsController.FIRST_PERSON_MIN_SPEED, GeneralSettingsController.FIRST_PERSON_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_HorizontalAxis.m_MaxSpeed, povSpeed, "pov (m_HorizontalAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povCamera.m_VerticalAxis.m_MaxSpeed, povSpeed, "pov (m_VerticalAxis) mouseSensitivity mismatch");

            var freeLookXSpeed = Mathf.Lerp(GeneralSettingsController.THIRD_PERSON_X_MIN_SPEED, GeneralSettingsController.THIRD_PERSON_X_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            var freeLookYSpeed = Mathf.Lerp(GeneralSettingsController.THIRD_PERSON_Y_MIN_SPEED, GeneralSettingsController.THIRD_PERSON_Y_MAX_SPEED, DCL.Settings.i.generalSettings.mouseSensitivity);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_XAxis.m_MaxSpeed, freeLookXSpeed, "freeLookCamera (m_XAxis) mouseSensitivity mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(freeLookCamera.m_YAxis.m_MaxSpeed, freeLookYSpeed, "freeLookCamera (m_YAxis) mouseSensitivity mismatch");

            UnityEngine.Assertions.Assert.AreApproximatelyEqual(AudioListener.volume, DCL.Settings.i.generalSettings.sfxVolume, "audioListener sfxVolume mismatch");
        }
    }
}