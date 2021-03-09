using Cinemachine;
using DCL.SettingsCommon;
using DCL.SettingsController;
using DCL.SettingsControls;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class SettingsControlsShould
    {
        private const string TEST_SCENE_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/Settings/SettingsControllers/Tests/TestScenes";
        private const string TEST_SCENE_NAME = "SettingsTestScene";

        private SettingsControlController settingController;

        private CinemachineFreeLook freeLookCamera;
        private CinemachineVirtualCamera firstPersonCamera;
        private CinemachinePOV povCamera;
        private Light environmentLight;
        private Volume postProcessVolume;
        private UniversalRenderPipelineAsset urpAsset;
        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;
        private FieldInfo lwrpaSoftShadowField = null;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode($"{TEST_SCENE_PATH}/{TEST_SCENE_NAME}.unity", new LoadSceneParameters(LoadSceneMode.Additive));

            SetupReferences();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            ScriptableObject.Destroy(settingController);

            yield return EditorSceneManager.UnloadSceneAsync(TEST_SCENE_NAME);
        }

        private void SetupReferences()
        {
            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            Assert.IsNotNull(urpAsset, "urpAsset is null!");

            lwrpaShadowField = urpAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaShadowField, "lwrpaShadowField is null!");

            lwrpaShadowResolutionField = urpAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaShadowResolutionField, "lwrpaShadowResolutionField is null!");

            lwrpaSoftShadowField = urpAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaSoftShadowField, "lwrpaSoftShadowField is null!");

            GeneralSettingsReferences generalSettingsReferences = GameObject.FindObjectOfType<GeneralSettingsReferences>();
            QualitySettingsReferences qualitySettingsReferences = GameObject.FindObjectOfType<QualitySettingsReferences>();

            Assert.IsNotNull(generalSettingsReferences, "GeneralSettingsReferences not found in scene");
            Assert.IsNotNull(qualitySettingsReferences, "QualitySettingsReferences not found in scene");

            freeLookCamera = generalSettingsReferences.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = generalSettingsReferences.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            environmentLight = qualitySettingsReferences.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = qualitySettingsReferences.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = qualitySettingsReferences.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(qualitySettingsReferences.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");
        }

        [Test]
        public void ChangeAllowVoiceChatCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<AllowVoiceChatControlController>();
            settingController.Initialize();

            // Act
            int newValue = (int)DCL.SettingsData.GeneralSettings.VoiceChatAllow.FRIENDS_ONLY;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "voiceChatAllow stored value mismatch");
        }

        [Test]
        public void ChangeAntialiasingChatCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<AntiAliasingControlController>();
            settingController.Initialize();

            // Act
            float newValue = (float)MsaaQuality._8x;
            settingController.UpdateSetting(newValue);

            // Assert
            int antiAliasingValue = 1 << (int)newValue;
            Assert.AreEqual((antiAliasingValue >> 2) + 1, settingController.GetStoredValue(), "antiAliasing stored value mismatch");
            Assert.AreEqual(antiAliasingValue, urpAsset.msaaSampleCount, "antiAliasing mismatch");
        }

        [Test]
        public void ChangeBaseResolutionCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<BaseResolutionControlController>();
            settingController.Initialize();

            // Act
            DCL.SettingsData.QualitySettings.BaseResolution newValue = DCL.SettingsData.QualitySettings.BaseResolution.BaseRes_1080;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual((int)newValue, settingController.GetStoredValue(), "baseResolution stored value mismatch");
        }

        [Test]
        public void ChangeBloomCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<BloomControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "bloom stored value mismatch");
            if (postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
            {
                Assert.AreEqual(newValue, bloom.active, "bloom mismatch");
            }
        }

        [Test]
        public void ChangeColorGradingCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ColorGradingControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "colorGrading stored value mismatch");
            Tonemapping toneMapping;
            if (QualitySettingsReferences.i.postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
            {
                Assert.AreEqual(newValue, toneMapping.active, "bloom mismatch");
            }
        }

        [Test]
        public void ChangeDetailObjectCullingCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<DetailObjectCullingControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "enableDetailObjectCulling stored value mismatch");
            Assert.AreNotEqual(newValue, CommonSettingsScriptableObjects.detailObjectCullingDisabled.Get());
        }

        [Test]
        public void ChangeDetailObjectCullingSizeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<DetailObjectCullingSizeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 20f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "detailObjectCullingThreshold stored value mismatch");
        }

        [Test]
        public void ChangeDrawDistanceCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<DrawDistanceControlController>();
            settingController.Initialize();

            // Act
            float newValue = 50f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "cameraDrawDistance stored value mismatch");
            Assert.AreEqual(freeLookCamera.m_Lens.FarClipPlane, newValue, "3rd person camera FarClipPlane value mismatch");
            Assert.AreEqual(firstPersonCamera.m_Lens.FarClipPlane, newValue, "1st person camera FarClipPlane value mismatch");
            Assert.AreEqual(RenderSettings.fogEndDistance, newValue, "fogEndDistance value mismatch");
            Assert.AreEqual(RenderSettings.fogStartDistance, newValue * 0.8f, "fogStartDistance value mismatch");
        }

        [Test]
        public void ChangeFPSLimitCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<FPSLimitControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "fpsCap stored value mismatch");
        }

        [Test]
        public void ChangeMouseSensivityCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<MouseSensivityControlController>();
            settingController.Initialize();

            // Act
            float newValue = 80f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "mouseSensitivity stored value mismatch");
            var povSpeed = Mathf.Lerp(MouseSensivityControlController.FIRST_PERSON_MIN_SPEED, MouseSensivityControlController.FIRST_PERSON_MAX_SPEED, newValue);
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povSpeed, povCamera.m_HorizontalAxis.m_MaxSpeed, "povCamera.m_HorizontalAxis.m_MaxSpeed value mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(povSpeed, povCamera.m_VerticalAxis.m_MaxSpeed, "povCamera.m_VerticalAxis.m_MaxSpeed value mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(
                Mathf.Lerp(MouseSensivityControlController.THIRD_PERSON_X_MIN_SPEED, MouseSensivityControlController.THIRD_PERSON_X_MAX_SPEED, newValue),
                freeLookCamera.m_XAxis.m_MaxSpeed,
                "freeLookCamera.m_XAxis.m_MaxSpeed value mismatch");
            UnityEngine.Assertions.Assert.AreApproximatelyEqual(
                Mathf.Lerp(MouseSensivityControlController.THIRD_PERSON_Y_MIN_SPEED, MouseSensivityControlController.THIRD_PERSON_Y_MAX_SPEED, newValue),
                freeLookCamera.m_YAxis.m_MaxSpeed,
                "freeLookCamera.m_YAxis.m_MaxSpeed value mismatch");
        }

        [Test]
        public void ChangeMuteSoundCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<MuteSoundControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "muteSound stored value mismatch");
            Assert.AreEqual(newValue ? 1f : 0f, AudioListener.volume, "sfxVolume value mismatch");
        }

        [Test]
        public void ChangeRenderingScaleCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<RenderingScaleControlController>();
            settingController.Initialize();

            // Act
            float newValue = 0.5f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "renderScale stored value mismatch");
            Assert.AreEqual(newValue, urpAsset.renderScale, "renderScale value mismatch");
        }

        [Test]
        public void ChangeShadowsCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ShadowControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "shadows stored value mismatch");
            Assert.AreEqual(newValue, lwrpaShadowField.GetValue(urpAsset), "lwrpaShadowField value mismatch");
            Assert.AreNotEqual(newValue, CommonSettingsScriptableObjects.shadowsDisabled.Get());
        }

        [Test]
        public void ChangeShadowDistanceCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ShadowDistanceControlController>();
            settingController.Initialize();

            // Act
            float newValue = 50f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "shadowDistance stored value mismatch");
            Assert.AreEqual(newValue, urpAsset.shadowDistance, "shadowDistance value mismatch");
        }

        [Test]
        public void ChangeShadowresolutionCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ShadowResolutionControlController>();
            settingController.Initialize();

            // Act
            int newValue = 4;
            settingController.UpdateSetting(newValue);

            // Assert
            UnityEngine.Rendering.Universal.ShadowResolution newValueFormatted = (UnityEngine.Rendering.Universal.ShadowResolution)(256 << newValue);
            Assert.AreEqual(
                (int)Mathf.Log((int)newValueFormatted, 2) - 8,
                settingController.GetStoredValue(),
                "shadowResolution stored value mismatch");
            Assert.AreEqual(newValueFormatted, lwrpaShadowResolutionField.GetValue(urpAsset), "lwrpaShadowResolutionField value mismatch");
        }

        [Test]
        public void ChangeSoftShadowsCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<SoftShadowsControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "softShadows stored value mismatch");
            Assert.AreEqual(newValue, lwrpaSoftShadowField.GetValue(urpAsset), "lwrpaShadowResolutionField value mismatch");
        }

        [Test]
        public void ChangeVoiceChatVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<VoiceChatVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "voiceChatVolume stored value mismatch");
        }
    }
}
