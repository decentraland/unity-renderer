using System.Collections.Generic;
using System.Reflection;
using Cinemachine;
using DCL.Rendering;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsCommon.SettingsControllers.SpecificControllers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DCL.Camera;

namespace DCL.SettingsCommon.SettingsControllers.Tests
{
    public class SettingsControlsShould
    {
        private SettingsControlController settingController;

        private CinemachineFreeLook freeLookCamera;
        private CinemachineVirtualCamera firstPersonCamera;
        private CinemachineFreeLook thirdPersonCamera;
        private CameraController cameraController;
        private CinemachinePOV povCamera;
        private Light environmentLight;
        private Volume postProcessVolume;
        private UniversalRenderPipelineAsset urpAsset;
        private FieldInfo lwrpaShadowField = null;
        private FieldInfo lwrpaShadowResolutionField = null;
        private FieldInfo lwrpaSoftShadowField = null;

        [SetUp]
        public void SetUp()
        {
            SetupReferences();
        }

        [TearDown]
        public void TearDown()
        {
            Settings.i.LoadDefaultSettings();

            foreach ( var go in legacySystems )
            {
                Object.Destroy(go);
            }
        }

        private List<GameObject> legacySystems = new List<GameObject>();

        private void SetupReferences()
        {
            legacySystems.Add(MainSceneFactory.CreateEnvironment());
            legacySystems.AddRange(MainSceneFactory.CreatePlayerSystems());

            urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            Assert.IsNotNull(urpAsset, "urpAsset is null!");

            lwrpaShadowField = urpAsset.GetType().GetField("m_MainLightShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaShadowField, "lwrpaShadowField is null!");

            lwrpaShadowResolutionField = urpAsset.GetType().GetField("m_MainLightShadowmapResolution", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaShadowResolutionField, "lwrpaShadowResolutionField is null!");

            lwrpaSoftShadowField = urpAsset.GetType().GetField("m_SoftShadowsSupported", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(lwrpaSoftShadowField, "lwrpaSoftShadowField is null!");

            SceneReferences sceneReferences = SceneReferences.i;
            Assert.IsNotNull(sceneReferences, "SceneReferences is invalid");

            freeLookCamera = sceneReferences.thirdPersonCamera;
            Assert.IsNotNull(freeLookCamera, "GeneralSettingsController: thirdPersonCamera reference missing");

            CinemachineVirtualCamera virtualCamera = sceneReferences.firstPersonCamera;
            Assert.IsNotNull(virtualCamera, "GeneralSettingsController: firstPersonCamera reference missing");
            povCamera = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            Assert.IsNotNull(povCamera, "GeneralSettingsController: firstPersonCamera doesn't have CinemachinePOV component");

            environmentLight = sceneReferences.environmentLight;
            Assert.IsNotNull(environmentLight, "QualitySettingsController: environmentLight reference missing");

            postProcessVolume = sceneReferences.postProcessVolume;
            Assert.IsNotNull(postProcessVolume, "QualitySettingsController: postProcessVolume reference missing");

            firstPersonCamera = sceneReferences.firstPersonCamera;
            Assert.IsNotNull(firstPersonCamera, "QualitySettingsController: firstPersonCamera reference missing");
            Assert.IsNotNull(sceneReferences.thirdPersonCamera, "QualitySettingsController: thirdPersonCamera reference missing");

            thirdPersonCamera = sceneReferences.thirdPersonCamera;
            Assert.IsNotNull(thirdPersonCamera, "Third person camera reference is missing");
            Assert.IsNotNull(sceneReferences.thirdPersonCamera, "Scene reference third person camera reference is missing");

            cameraController = sceneReferences.cameraController;
            Assert.IsNotNull(cameraController, "Camera controller reference is missing");
            Assert.IsNotNull(sceneReferences.cameraController, "Scene camera controller reference is missing");
        }

        [Test]
        public void ChangeAllowVoiceChatCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<AllowVoiceChatControlController>();
            settingController.Initialize();

            // Act
            int newValue = (int)GeneralSettings.VoiceChatAllow.FRIENDS_ONLY;
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
            QualitySettings.BaseResolution newValue = QualitySettings.BaseResolution.BaseRes_Normal;
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
            var scriptableObject = ScriptableObject.CreateInstance<DetailObjectCullingSizeControlController>();
            scriptableObject.cullingControllerSettingsData = ScriptableObject.CreateInstance<CullingControllerSettingsData>();
            scriptableObject.cullingControllerSettingsData.rendererProfileMax = new CullingControllerProfile();
            scriptableObject.cullingControllerSettingsData.rendererProfileMin = new CullingControllerProfile();
            scriptableObject.cullingControllerSettingsData.skinnedRendererProfileMax = new CullingControllerProfile();
            scriptableObject.cullingControllerSettingsData.skinnedRendererProfileMin = new CullingControllerProfile();
            settingController = scriptableObject;
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
        public void ChangeCameraFOVCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<FOVControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Camera FOV stored value mismatch");
            Assert.AreEqual(firstPersonCamera.m_Lens.FieldOfView, newValue, "1st person camera FOV value mismatch");
        }

        [Test]
        public void ChangeInvertYMouseCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<InvertYAxisControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Invert input Y axis stored value mismatch");
            Assert.AreEqual(thirdPersonCamera.m_YAxis.m_InvertInput, !newValue, "Third person camera invert Y axis mismatch");
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

        [Test]
        public void ChangeAvatarSFXVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<AvatarSFXVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Avatar SFX Volume stored value mismatch");
        }

        [Test]
        public void ChangeChatSFXToggleCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ChatSFXToggleControlController>();
            settingController.Initialize();

            // Act
            AudioSettings.ChatNotificationType newValue = AudioSettings.ChatNotificationType.All;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual((int) newValue, settingController.GetStoredValue(), "Chat SFX Toggle stored value mismatch");
        }

        [Test]
        public void ChangeMasterVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<MasterVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Master Volume stored value mismatch");
        }

        [Test]
        public void ChangeMusicVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<MusicVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Music Volume stored value mismatch");
        }

        [Test]
        public void ChangeSceneSFXVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<SceneSFXVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "Scene SFX Volume stored value mismatch");
        }

        [Test]
        public void ChangeUISFXVolumeCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<UISFXVolumeControlController>();
            settingController.Initialize();

            // Act
            float newValue = 90f;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "UI SFX Volume stored value mismatch");
        }

        [Test]
        public void ChangeHideUICorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<HideUIControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "hideUI stored value mismatch");
        }

        [Test]
        public void ChangeShowAvatarNamesCorrectly()
        {
            // Arrange
            settingController = ScriptableObject.CreateInstance<ShowAvatarNamesControlController>();
            settingController.Initialize();

            // Act
            bool newValue = true;
            settingController.UpdateSetting(newValue);

            // Assert
            Assert.AreEqual(newValue, settingController.GetStoredValue(), "showAvatarNames stored value mismatch");
        }
    }
}
