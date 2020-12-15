using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using DCL.SettingsHUD;
using QualitySettings = DCL.SettingsData.QualitySettings;
using GeneralSettings = DCL.SettingsData.GeneralSettings;

namespace Tests
{
    public class SettingsHUDControllerShould : TestsBase
    {
        private SettingsHUDController controller;
        QualitySettings testQualitySettings;
        GeneralSettings testGeneralSettings;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            testQualitySettings = new QualitySettings()
            {
                baseResolution = QualitySettings.BaseResolution.BaseRes_1080,
                antiAliasing = UnityEngine.Rendering.Universal.MsaaQuality._4x,
                renderScale = 0.1f,
                shadows = false,
                softShadows = false,
                shadowResolution = UnityEngine.Rendering.Universal.ShadowResolution._512,
                cameraDrawDistance = 51f,
                bloom = true,
                colorGrading = true
            };

            testGeneralSettings = new GeneralSettings()
            {
                mouseSensitivity = 1,
                sfxVolume = 0
            };

            DCL.Settings.i.ApplyQualitySettings(testQualitySettings);
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);
            controller = new SettingsHUDController();
            controller.SetVisibility(true);
        }

        protected override IEnumerator TearDown()
        {
            // Reset volume setting
            testGeneralSettings = new GeneralSettings()
            {
                sfxVolume = 1
            };
            DCL.Settings.i.ApplyGeneralSettings(testGeneralSettings);

            controller.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
            yield break;
        }

        [UnityTest]
        public IEnumerator ReflectGeneralSettingsValuesCorrectly()
        {
            SettingsGeneralView generalContent = controller.view.GetComponentInChildren<SettingsGeneralView>();
            Assert.IsTrue(generalContent.qualityPresetSpinBox.label == SettingsGeneralView.TEXT_QUALITY_CUSTOM, "qualityPresetSpinBox missmatch");
            Assert.IsTrue(generalContent.baseResSpinBox.value == 1, "textureResSpinBox missmatch");
            Assert.IsTrue(generalContent.shadowResSpinBox.label == "512", "shadowResSpinBox missmatch");
            Assert.IsTrue(generalContent.soundToggle.isOn == false, "soundToggle missmatch");
            Assert.IsTrue(generalContent.colorGradingToggle.isOn == true, "colorGradingToggle missmatch");
            Assert.IsTrue(generalContent.shadowToggle.isOn == false, "shadowToggle missmatch");
            Assert.IsTrue(generalContent.softShadowToggle.isOn == false, "softShadowToggle missmatch");
            Assert.IsTrue(generalContent.bloomToggle.isOn == true, "bloomToggle missmatch");
            Assert.IsTrue(generalContent.mouseSensitivitySlider.value == generalContent.mouseSensitivitySlider.maxValue, "mouseSensitivitySlider missmatch");
            Assert.IsTrue(generalContent.antiAliasingSlider.value == 2, "antiAliasingSlider missmatch");
            Assert.IsTrue(generalContent.renderingScaleSlider.value == 0.1f, "renderingScaleSlider missmatch");
            Assert.IsTrue(generalContent.drawDistanceSlider.value == 51f, "drawDistanceSlider missmatch");
            yield break;
        }
    }
}